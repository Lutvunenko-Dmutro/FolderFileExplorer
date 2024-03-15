using System;
using System.Windows.Forms;

namespace PhotoPrintCalculator
{
    public partial class Form1 : Form
    {
        // Вартість фотографії для кожного розміру
        private double photoPrice = 0.0;

        // Знижка, надається, якщо кількість фотографій перевищує 20
        private double discount = 0.1;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Вартість однієї фотографії
            double totalCost = 0.0;

            // Визначаємо вартість фотографії в залежності від обраного розміру
            if (radioButton1.Checked)
                photoPrice = 1.5;
            else if (radioButton2.Checked)
                photoPrice = 2.0;
            else if (radioButton3.Checked)
                photoPrice = 3.0;

            // Кількість фотографій
            int quantity = (int)numericUpDown1.Value;

            // Розрахунок вартості замовлення
            totalCost = photoPrice * quantity;

            // Перевірка наявності знижки
            if (quantity > 20)
                totalCost *= (1 - discount);

            // Вивід результату
            MessageBox.Show("Вартість замовлення: $" + totalCost.ToString("0.00"), "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }
    }
}
