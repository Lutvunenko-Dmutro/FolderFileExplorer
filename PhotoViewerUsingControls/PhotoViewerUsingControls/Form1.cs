using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PhotoViewerUsingControls
{
    public partial class Form1 : Form
    {
        private ListBox listBoxImages;
        private PictureBox pictureBox;
        private string folderPath;

        public Form1()
        {
            InitializeComponent();
            InitializeControls();

            // Встановлюємо початковий шлях 
            folderPath = @"C:\Users\dmutr\Desktop\1\С++\PhotoViewerUsingControls\PhotoViewerUsingControls\Properties";

            PopulateListBox();
        }

        private void InitializeControls()
        {
            // Ініціалізація ListBox
            listBoxImages = new ListBox();
            listBoxImages.Dock = DockStyle.Left;
            listBoxImages.SelectedIndexChanged += listBoxImages_SelectedIndexChanged;

            // Ініціалізація PictureBox
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            // Додаємо ListBox та PictureBox на форму
            Controls.Add(listBoxImages);
            Controls.Add(pictureBox);
        }

        private void PopulateListBox()
        {
            string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

            foreach (string file in imageFiles)
            {
                listBoxImages.Items.Add(Path.GetFileName(file));
            }
        }

        private void listBoxImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxImages.SelectedIndex != -1)
            {
                string selectedImage = listBoxImages.SelectedItem.ToString();
                string imagePath = Path.Combine(folderPath, selectedImage);

                // Використовуємо клас Bitmap для завантаження зображення
                Bitmap image = new Bitmap(imagePath);

                // Масштабуємо зображення до розмірів PictureBox
                pictureBox.Image = (Image)image.Clone();
            }
        }
    }
}
