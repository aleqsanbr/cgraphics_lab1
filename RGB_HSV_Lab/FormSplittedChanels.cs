using FastBitmap;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RGB_HSV_Lab
{
    public partial class FormSplittedChanels : Form
    {
        private PictureBox redHistogramBox, greenHistogramBox, blueHistogramBox;

        public FormSplittedChanels()
        {
            InitializeComponent();
            
            CreateHistogramBoxes();

            Bitmap bitmap = new Bitmap(red_pictureBox.Image);
            
            int[] redHistogram = new int[256];
            int[] greenHistogram = new int[256]; 
            int[] blueHistogram = new int[256];
            
            bitmap.ForEach(color => {
                redHistogram[color.R]++;
                greenHistogram[color.G]++;
                blueHistogram[color.B]++;
            });
            
            red_pictureBox.Image = bitmap.Select(color => Color.FromArgb(color.R, 0, 0));
            green_pictureBox.Image = bitmap.Select(color => Color.FromArgb(0, color.G, 0));
            blue_pictureBox.Image = bitmap.Select(color => Color.FromArgb(0, 0, color.B));
            
            redHistogramBox.Image = DrawColorHistogram(redHistogram, "Red Channel", Color.Red);
            greenHistogramBox.Image = DrawColorHistogram(greenHistogram, "Green Channel", Color.Green);
            blueHistogramBox.Image = DrawColorHistogram(blueHistogram, "Blue Channel", Color.Blue);
        }
        
        private void CreateHistogramBoxes()
        {
            redHistogramBox = new PictureBox();
            redHistogramBox.Size = new Size(500, 200);
            redHistogramBox.Location = new Point(12, 520);
            redHistogramBox.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(redHistogramBox);
            
            greenHistogramBox = new PictureBox();
            greenHistogramBox.Size = new Size(500, 200);  
            greenHistogramBox.Location = new Point(530, 520);
            greenHistogramBox.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(greenHistogramBox);
            
            blueHistogramBox = new PictureBox();
            blueHistogramBox.Size = new Size(500, 200);  
            blueHistogramBox.Location = new Point(1050, 520);
            blueHistogramBox.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(blueHistogramBox);

            this.Height = 650;
            this.Width = 1600;
        }
        
        private Bitmap DrawColorHistogram(int[] histogram, string title, Color channelColor)
        {
            int width = 500;
            int height = 200;
            Bitmap bmp = new Bitmap(width, height);
            
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                
                int maxValue = histogram.Max();
                if (maxValue == 0) maxValue = 1;

                using (Brush brush = new SolidBrush(Color.FromArgb(150, channelColor))) // Полупрозрачный
                {
                    for (int i = 0; i < 256; i++)
                    {
                        int barHeight = (int)((double)histogram[i] / maxValue * (height - 50));
                        int x = i * (width - 40) / 256 + 20;
                        int barWidth = Math.Max(1, (width - 40) / 256);
                        
                        g.FillRectangle(brush, x, height - barHeight - 30, barWidth, barHeight);
                    }
                }
                
                g.DrawString(title, new Font("Arial", 14, FontStyle.Bold), new SolidBrush(channelColor), 20, 10);

                g.DrawString("0", new Font("Arial", 10), Brushes.Black, 20, height - 25);
                g.DrawString("128", new Font("Arial", 10), Brushes.Black, width/2 - 10, height - 25);
                g.DrawString("255", new Font("Arial", 10), Brushes.Black, width - 40, height - 25);
            }
            
            return bmp;
        }
    }
}