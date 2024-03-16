using System;
using System.Drawing;
using System.Windows.Forms;

namespace SlideShowApp
{
    public partial class Form1 : Form
    {
        private PictureBox pictureBox;
        private Button previousButton;
        private Button nextButton;
        private Button rotateButton;
        private System.Windows.Forms.Timer timer;

        private string[] imagePaths = { @"C:\Users\dmutr\Desktop\1\С++\SlideShowApp\SlideShowApp\Properties\foto1.jpg", @"C:\Users\dmutr\Desktop\1\С++\SlideShowApp\SlideShowApp\Properties\foto2.jpg", @"C:\Users\dmutr\Desktop\1\С++\SlideShowApp\SlideShowApp\Properties\foto3.jpg" };
        private int currentImageIndex = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Dock = DockStyle.Fill;
            this.Controls.Add(pictureBox);

            previousButton = new Button();
            previousButton.Text = "Previous";
            previousButton.Size = new Size(80, 30);
            previousButton.Location = new Point(20, this.ClientSize.Height - 50);
            previousButton.Click += PreviousButton_Click;
            this.Controls.Add(previousButton);

            nextButton = new Button();
            nextButton.Text = "Next";
            nextButton.Size = new Size(80, 30);
            nextButton.Location = new Point(previousButton.Right + 20, this.ClientSize.Height - 50);
            nextButton.Click += NextButton_Click;
            this.Controls.Add(nextButton);

            rotateButton = new Button();
            rotateButton.Text = "Rotate";
            rotateButton.Size = new Size(80, 30);
            rotateButton.Location = new Point(nextButton.Right + 20, this.ClientSize.Height - 50);
            rotateButton.Click += RotateButton_Click;
            this.Controls.Add(rotateButton);

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000; // Интервал в миллисекундах (3 секунды)
            timer.Tick += Timer_Tick;
            timer.Start();

            ShowImage(); // Отображаем первое изображение при запуске приложения
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            currentImageIndex = (currentImageIndex - 1 + imagePaths.Length) % imagePaths.Length;
            ShowImage();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
            ShowImage();
        }

        private void RotateButton_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                pictureBox.Invalidate();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            NextButton_Click(sender, e); // Вызываем переключение на следующее изображение
        }

        private void ShowImage()
        {
            pictureBox.ImageLocation = imagePaths[currentImageIndex];
        }
private void button1_Click(object sender, EventArgs e)
        {
            // Перемещаемся к предыдущему изображению
            currentImageIndex = (currentImageIndex - 1 + imagePaths.Length) % imagePaths.Length;
            ShowImage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Перемещаемся к следующему изображению
            currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
            ShowImage();
        }
    }
}
