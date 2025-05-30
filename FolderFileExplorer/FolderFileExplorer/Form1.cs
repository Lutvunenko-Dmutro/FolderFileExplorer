using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FolderFileExplorer
{
    public partial class Form1 : Form
    {
        // Основні поля для дерева, іконок, контекстного меню та кешу іконок
        private TreeView treeView;
        private ImageList imageList;
        private ContextMenuStrip contextMenuStrip;
        private Dictionary<string, int> extensionIconIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private CancellationTokenSource searchCts;

        public Form1()
        {
            InitializeComponent();
            InitializeImageList();
            InitializeTreeView();
            PopulateTreeView();
            treeView.AfterSelect += treeView_AfterSelect;
            InitializeContextMenu();
            ApplyDarkTheme();
        }

        // Застосування темної теми до всіх основних елементів форми
        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;

            if (treeView != null)
            {
                treeView.BackColor = Color.FromArgb(40, 40, 40);
                treeView.ForeColor = Color.White;
                treeView.LineColor = Color.Gray;
            }

            if (statusStrip1 != null)
            {
                statusStrip1.BackColor = Color.FromArgb(32, 32, 32);
                statusStrip1.ForeColor = Color.White;
            }
            if (toolStripStatusLabel1 != null)
            {
                toolStripStatusLabel1.ForeColor = Color.White;
                toolStripStatusLabel1.BackColor = Color.FromArgb(32, 32, 32);
            }
            if (contextMenuStrip != null)
            {
                contextMenuStrip.BackColor = Color.FromArgb(40, 40, 40);
                contextMenuStrip.ForeColor = Color.White;
            }
        }

        // Ініціалізація списку іконок для дерева
        private void InitializeImageList()
        {
            imageList = new ImageList();
            imageList.Images.Add("folder", GetFolderIcon());
            imageList.Images.Add("file", SystemIcons.Application);
            extensionIconIndex["folder"] = 0;
            extensionIconIndex["file"] = 1;
        }

        // Отримання стандартної жовтої іконки папки через WinAPI
        private Icon GetFolderIcon()
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hImg = SHGetFileInfo(
                null,
                0x10, // FILE_ATTRIBUTE_DIRECTORY
                ref shinfo,
                (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES
            );
            if (shinfo.hIcon != IntPtr.Zero)
            {
                Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                DestroyIcon(shinfo.hIcon);
                return icon;
            }
            else
            {
                return SystemIcons.Application;
            }
        }

        // Додаємо дерево у праву панель SplitContainer
        private void InitializeTreeView()
        {
            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.BeforeExpand += treeView_BeforeExpand;
            treeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            treeView.ImageList = imageList;
            splitContainer1.Panel2.Controls.Add(treeView);
            splitContainer1.Panel2.Controls.SetChildIndex(treeView, 2);
        }

        // Заповнення дерева дисками
        private void PopulateTreeView()
        {
            treeView.Nodes.Clear();
            string[] drives = Environment.GetLogicalDrives();

            foreach (string drive in drives)
            {
                TreeNode driveNode = new TreeNode(drive);
                driveNode.Tag = drive;
                driveNode.Nodes.Add("*");
                driveNode.ImageKey = "folder";
                driveNode.SelectedImageKey = "folder";
                treeView.Nodes.Add(driveNode);
            }
        }

        // Створення вузла для папки
        private TreeNode CreateDirectoryNode(string dir)
        {
            var node = new TreeNode(Path.GetFileName(dir));
            node.Tag = dir;
            node.Nodes.Add("*");
            node.ImageIndex = extensionIconIndex["folder"];
            node.SelectedImageIndex = extensionIconIndex["folder"];
            return node;
        }

        // Створення вузла для файлу з відповідною іконкою
        private TreeNode CreateFileNode(string file)
        {
            string ext = Path.GetExtension(file);
            int iconIndex;
            if (!extensionIconIndex.TryGetValue(ext, out iconIndex))
            {
                using (Icon icon = Icon.ExtractAssociatedIcon(file))
                {
                    imageList.Images.Add(ext, icon ?? SystemIcons.Application);
                    iconIndex = imageList.Images.Count - 1;
                    extensionIconIndex[ext] = iconIndex;
                }
            }
            var node = new TreeNode(Path.GetFileName(file));
            node.Tag = file;
            node.ImageIndex = iconIndex;
            node.SelectedImageIndex = iconIndex;
            return node;
        }

        // Додавання папок і файлів при розгортанні вузла
        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "*")
            {
                e.Node.Nodes.Clear();
                string[] dirs = null;
                string[] files = null;

                try
                {
                    dirs = Directory.GetDirectories(e.Node.Tag.ToString());
                    files = Directory.GetFiles(e.Node.Tag.ToString());
                }
                catch (UnauthorizedAccessException) { return; }
                catch (IOException) { return; }
                catch (System.Security.SecurityException) { return; }

                treeView.BeginUpdate();
                try
                {
                    foreach (string dir in dirs)
                    {
                        e.Node.Nodes.Add(CreateDirectoryNode(dir));
                    }
                    foreach (string file in files)
                    {
                        e.Node.Nodes.Add(CreateFileNode(file));
                    }
                    toolStripStatusLabel1.Text = $"Папок: {dirs.Length}, Файлів: {files.Length}";
                }
                finally
                {
                    treeView.EndUpdate();
                }
            }
        }

        // Відкриття файлу подвійним кліком
        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                string path = e.Node.Tag.ToString();
                if (File.Exists(path))
                {
                    try
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo(path)
                        {
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка при відкритті файлу: " + ex.Message);
                    }
                }
                // Для папок нічого не робимо
            }
        }

        // Оновлення статусбару при виборі вузла
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                toolStripStatusLabel1.Text = "";
                return;
            }
            string path = e.Node.Tag.ToString();
            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                toolStripStatusLabel1.Text = $"Папка: {dirInfo.FullName} | Змінено: {dirInfo.LastWriteTime}";
            }
            else if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                toolStripStatusLabel1.Text = $"Файл: {fileInfo.Name} | Розмір: {fileInfo.Length} байт | Змінено: {fileInfo.LastWriteTime}";
            }
            else
            {
                toolStripStatusLabel1.Text = "";
            }
        }

        // Ініціалізація контекстного меню без іконок
        private void InitializeContextMenu()
        {
            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.ShowImageMargin = false;
            var openItem = new ToolStripMenuItem("Відкрити", null, ContextMenu_Open_Click) { Image = null };
            var showInExplorerItem = new ToolStripMenuItem("Показати в провіднику", null, ContextMenu_ShowInExplorer_Click) { Image = null };
            var deleteItem = new ToolStripMenuItem("Видалити", null, ContextMenu_Delete_Click) { Image = null };
            var propsItem = new ToolStripMenuItem("Властивості", null, ContextMenu_Props_Click) { Image = null };
            contextMenuStrip.Items.AddRange(new[] { openItem, showInExplorerItem, deleteItem, propsItem });
            treeView.ContextMenuStrip = contextMenuStrip;
        }

        // Відкрити файл або папку через контекстне меню
        private void ContextMenu_Open_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag == null) return;
            string path = treeView.SelectedNode.Tag.ToString();
            if (File.Exists(path))
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при відкритті: " + ex.Message);
                }
            }
            else if (Directory.Exists(path))
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo("explorer.exe", $"\"{path}\"")
                    {
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при відкритті папки: " + ex.Message);
                }
            }
        }

        // Показати файл/папку у провіднику Windows
        private void ContextMenu_ShowInExplorer_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag == null) return;
            string path = treeView.SelectedNode.Tag.ToString();
            try
            {
                if (File.Exists(path))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
                    {
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                else if (Directory.Exists(path))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo("explorer.exe", $"\"{path}\"")
                    {
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при відкритті провідника: " + ex.Message);
            }
        }

        // Видалення файлу або папки
        private void ContextMenu_Delete_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag == null) return;
            string path = treeView.SelectedNode.Tag.ToString();
            var result = MessageBox.Show($"Видалити '{path}'?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                else if (Directory.Exists(path))
                    Directory.Delete(path, true);
                treeView.SelectedNode.Remove();
                toolStripStatusLabel1.Text = "Видалено";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні: " + ex.Message);
            }
        }

        // Відкрити вікно властивостей файлу/папки
        private void ContextMenu_Props_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag == null) return;
            string path = treeView.SelectedNode.Tag.ToString();
            ShowFileProperties(path);
        }

        // Відкрити стандартне вікно властивостей Windows
        private void ShowFileProperties(string filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpFile = filename;
            info.nShow = 5;
            info.fMask = 0x0000000C; // SEE_MASK_INVOKEIDLIST | SEE_MASK_NOCLOSEPROCESS
            info.lpVerb = "properties";
            info.hwnd = this.Handle;
            ShellExecuteEx(ref info);
        }

        // Структури та імпорти для WinAPI
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        // Асинхронний пошук по всіх дисках з індикатором "Пошук..."
        private async void searchButton_Click(object sender, EventArgs e)
        {
            string searchTerm = searchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Введіть ім'я для пошуку.");
                return;
            }

            // Скасовуємо попередній пошук, якщо він ще йде
            searchCts?.Cancel();
            searchCts = new CancellationTokenSource();
            var token = searchCts.Token;

            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            toolStripStatusLabel1.Text = "Пошук...";
            try
            {
                await Task.Run(() =>
                {
                    string[] drives = Environment.GetLogicalDrives();
                    foreach (string drive in drives)
                    {
                        if (token.IsCancellationRequested) return;
                        try
                        {
                            if (Directory.Exists(drive))
                                SearchAndAddNodesAsync(drive, searchTerm, treeView.Nodes, token);
                        }
                        catch { }
                    }
                });
                if (treeView.Nodes.Count == 0)
                {
                    toolStripStatusLabel1.Text = "Нічого не знайдено";
                    MessageBox.Show("Нічого не знайдено.");
                }
                else
                {
                    toolStripStatusLabel1.Text = $"Знайдено: {treeView.Nodes.Count} кореневих елементів";
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        // Рекурсивний асинхронний пошук з оновленням дерева через Invoke
        private void SearchAndAddNodesAsync(string path, string searchTerm, TreeNodeCollection nodes, CancellationToken token)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    if (token.IsCancellationRequested) return;
                    string dirName = Path.GetFileName(dir);
                    bool match = dirName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
                    TreeNodeCollection parentNodes = nodes;
                    TreeNode node = null;
                    if (match)
                    {
                        node = CreateDirectoryNode(dir);
                        this.Invoke((MethodInvoker)(() => nodes.Add(node)));
                        parentNodes = node.Nodes;
                    }
                    if (Directory.Exists(dir))
                        SearchAndAddNodesAsync(dir, searchTerm, match ? parentNodes : nodes, token);
                }
                foreach (var file in Directory.GetFiles(path))
                {
                    if (token.IsCancellationRequested) return;
                    string fileName = Path.GetFileName(file);
                    if (fileName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        TreeNode fileNode = CreateFileNode(file);
                        this.Invoke((MethodInvoker)(() => nodes.Add(fileNode)));
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (System.Security.SecurityException) { }
            catch (IOException) { }
            catch (Exception) { }
        }

        // Подія завантаження форми: фокус на поле пошуку
        private void Form1_Load(object sender, EventArgs e)
        {
            searchTextBox.Focus();
        }
    }
}

