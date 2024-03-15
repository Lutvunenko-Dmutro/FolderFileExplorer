using System;
using System.IO;
using System.Windows.Forms;

namespace FolderFileExplorer
{
    public partial class Form1 : Form
    {
        private TreeView treeView;

        public Form1()
        {
            InitializeComponent();
            InitializeTreeView();
            PopulateTreeView();
        }

        private void InitializeTreeView()
        {
            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.BeforeExpand += treeView_BeforeExpand;
            treeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            Controls.Add(treeView);
        }

        private void PopulateTreeView()
        {
            treeView.Nodes.Clear();
            string[] drives = Environment.GetLogicalDrives();

            foreach (string drive in drives)
            {
                TreeNode driveNode = new TreeNode(drive);
                driveNode.Tag = drive;
                driveNode.Nodes.Add("*");
                treeView.Nodes.Add(driveNode);
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "*")
            {
                e.Node.Nodes.Clear();
                string[] dirs;
                string[] files;

                try
                {
                    dirs = Directory.GetDirectories(e.Node.Tag.ToString());
                    files = Directory.GetFiles(e.Node.Tag.ToString());
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }

                foreach (string dir in dirs)
                {
                    TreeNode dirNode = new TreeNode(Path.GetFileName(dir));
                    dirNode.Tag = dir;
                    dirNode.Nodes.Add("*");
                    e.Node.Nodes.Add(dirNode);
                }

                foreach (string file in files)
                {
                    TreeNode fileNode = new TreeNode(Path.GetFileName(file));
                    fileNode.Tag = file;
                    e.Node.Nodes.Add(fileNode);
                }
            }
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null && File.Exists(e.Node.Tag.ToString()))
            {
                System.Diagnostics.Process.Start(e.Node.Tag.ToString());
            }
        }
    }
}
