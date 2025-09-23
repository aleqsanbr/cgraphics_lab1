using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FastBitmap;

namespace RGB_HSV_Lab
{
    public partial class FormDesaturated : Form
    {
        private PictureBox histogramBox1, histogramBox2;

        public FormDesaturated()
        {
            InitializeComponent();
            
            CreateHistogramBoxes();

            Bitmap bitmap = new Bitmap(first_formula_pictureBox.Image);

            int[] histogram1 = new int[256];
            int[] histogram2 = new int[256];

            // NTSC
            first_formula_pictureBox.Image = bitmap.Select(color =>
            {
                var avg = (byte)Math.Round(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                histogram1[avg]++;
                return Color.FromArgb(avg, avg, avg);
            });

            // sRGB/Rec.709
            second_formula_pictureBox.Image = bitmap.Select(color =>
            {
                var avg = (byte)Math.Round(color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722);
                histogram2[avg]++;
                return Color.FromArgb(avg, avg, avg);
            });
            
            difference_pictureBox.Image = bitmap.Select(color =>
            {
                var gray1 = color.R * 0.299 + color.G * 0.587 + color.B * 0.114;
                var gray2 = color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722;
                var diff = (byte)Math.Round(Math.Abs(gray1 - gray2));
                return Color.FromArgb(diff, diff, diff);
            });
            
            histogramBox1.Image = DrawHistogram(histogram1, "NTSC");
            histogramBox2.Image = DrawHistogram(histogram2, "sRGB");
        }

        private void CreateHistogramBoxes()
        {
            histogramBox1 = new PictureBox();
            histogramBox1.Size = new Size(600, 300);
            histogramBox1.Location = new Point(12, 520);
            histogramBox1.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(histogramBox1);
            
            histogramBox2 = new PictureBox();
            histogramBox2.Size = new Size(600, 300);
            histogramBox2.Location = new Point(650, 520);
            histogramBox2.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(histogramBox2);

            this.Height = 800;
            this.Width = 1300;
        }

        private Bitmap DrawHistogram(int[] histogram, string title)
        {
            int width = 600;
            int height = 300;
            Bitmap bmp = new Bitmap(width, height);
            
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                
                int maxValue = histogram.Max();
                if (maxValue == 0) maxValue = 1;
                
                for (int i = 0; i < 256; i++)
                {
                    int barHeight = (int)((double)histogram[i] / maxValue * (height - 50));
                    int x = i * (width - 40) / 256 + 20; // Отступы
                    int barWidth = Math.Max(2, (width - 40) / 256);
                    
                    g.FillRectangle(Brushes.DarkGray, x, height - barHeight - 30, barWidth, barHeight);
                }
                
                g.DrawString(title, new Font("Arial", 16, FontStyle.Bold), Brushes.Black, 20, 10);
                
                g.DrawString("0", new Font("Arial", 10), Brushes.Black, 20, height - 25);
                g.DrawString("128", new Font("Arial", 10), Brushes.Black, width/2 - 10, height - 25);
                g.DrawString("255", new Font("Arial", 10), Brushes.Black, width - 40, height - 25);
            }
            
            return bmp;
        }

        private void second_formula_pictureBox_Click(object sender, EventArgs e)
        {

        }
    }
}