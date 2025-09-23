using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RGB_HSV_Lab
{
    public partial class HSV_form : Form
    {
        Bitmap _originalBitmap;
        Bitmap _currentBitmap;
        HSV_Bitmap _hsv_map;
        string _originalFileName;

        public HSV_form()
        {
            InitializeComponent();
            _originalFileName = "modified_image";

            // Сохраняем оригинальное изображение
            _originalBitmap = new Bitmap(pictureBox.Image);
            _currentBitmap = new Bitmap(_originalBitmap);
            _hsv_map = new HSV_Bitmap(_originalBitmap);

            // Инициализируем начальные значения ползунков
            h_trackBar.Value = 0;
            s_trackBar.Value = 0;
            v_trackBar.Value = 0;
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            // Всегда работаем от оригинального изображения
            _currentBitmap = new Bitmap(_originalBitmap);

            _hsv_map.ChangeHSL(ref _currentBitmap,
                (h_trackBar.Value, s_trackBar.Value, v_trackBar.Value));

            pictureBox.Image = _currentBitmap;
        }

        public class HSV_Bitmap
        {
            private int[,] h_arr;
            private float[,] s_arr, v_arr;

            public HSV_Bitmap(Bitmap rgb_bitmap)
            {
                h_arr = new int[rgb_bitmap.Width, rgb_bitmap.Height];
                s_arr = new float[rgb_bitmap.Width, rgb_bitmap.Height];
                v_arr = new float[rgb_bitmap.Width, rgb_bitmap.Height];

                for (int i = 0; i < rgb_bitmap.Width; i++)
                {
                    for (int j = 0; j < rgb_bitmap.Height; j++)
                    {
                        (h_arr[i, j], s_arr[i, j], v_arr[i, j]) = rgb2hsv(rgb_bitmap.GetPixel(i, j));
                    }
                }
            }

            public void ChangeHSL(ref Bitmap rgb_bitmap, (int, int, int) adjustments)
            {
                for (int i = 0; i < rgb_bitmap.Width; i++)
                {
                    for (int j = 0; j < rgb_bitmap.Height; j++)
                    {
                        // Применяем изменения к оригинальным HSV значениям
                        int new_h = (h_arr[i, j] + adjustments.Item1 + 360) % 360;

                        // Для насыщенности: -100..100 → -1.0..1.0
                        float new_s = clamp(s_arr[i, j] + (adjustments.Item2 / 100f), 0f, 1f);

                        // Для яркости: -100..100 → -1.0..1.0
                        float new_v = clamp(v_arr[i, j] + (adjustments.Item3 / 100f), 0f, 1f);

                        // Конвертируем обратно в RGB
                        Color newColor = HsvToRgb(new_h, new_s, new_v);
                        rgb_bitmap.SetPixel(i, j, newColor);
                    }
                }
            }

            private Color HsvToRgb(int h, float s, float v)
            {
                h = (h % 360 + 360) % 360;
                int hi = (h / 60) % 6;
                float f = h / 60f - hi;
                float p = v * (1 - s);
                float q = v * (1 - f * s);
                float t = v * (1 - (1 - f) * s);

                float r, g, b;
                switch (hi)
                {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    case 5: r = v; g = p; b = q; break;
                    default: r = g = b = 0; break;
                }

                return Color.FromArgb(
                    (byte)(r * 255),
                    (byte)(g * 255),
                    (byte)(b * 255));
            }

            private float clamp(float value, float min, float max)
            {
                if (value < min) return min;
                if (value > max) return max;
                return value;
            }

            private (int, float, float) rgb2hsv(Color rgb)
            {
                float r = rgb.R / 255f;
                float g = rgb.G / 255f;
                float b = rgb.B / 255f;

                float max = Math.Max(r, Math.Max(g, b));
                float min = Math.Min(r, Math.Min(g, b));
                float delta = max - min;

                int h = 0;
                float s = 0f;
                float v = max;

                if (delta > 0)
                {
                    if (max == r)
                        h = (int)(60 * ((g - b) / delta) % 360);
                    else if (max == g)
                        h = (int)(60 * ((b - r) / delta + 2));
                    else if (max == b)
                        h = (int)(60 * ((r - g) / delta + 4));

                    if (h < 0) h += 360;
                }

                if (max > 0)
                    s = delta / max;

                return (h, s, v);
            }
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Сохранить изображение с HSV коррекцией";
                saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp|All Files|*.*";
                saveDialog.DefaultExt = "png";
                saveDialog.AddExtension = true;

                string defaultFileName = $"{_originalFileName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                saveDialog.FileName = defaultFileName;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = saveDialog.FileName;
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                        if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                        {
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        }
                        else if (filePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        {
                            format = System.Drawing.Imaging.ImageFormat.Bmp;
                        }

                        _currentBitmap.Save(filePath, format);

                        MessageBox.Show($"Изображение успешно сохранено!\n\nПуть: {filePath}",
                                      "Сохранение завершено",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void reset_button_Click(object sender, EventArgs e)
        {
            // Сброс ползунков к нулевым значениям
            h_trackBar.Value = 0;
            s_trackBar.Value = 0;
            v_trackBar.Value = 0;

            // Восстановление оригинального изображения
            _currentBitmap = new Bitmap(_originalBitmap);
            pictureBox.Image = _currentBitmap;
        }
    }
}