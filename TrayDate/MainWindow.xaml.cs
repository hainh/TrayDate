using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace TrayDate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.IContainer components = null;
        private readonly NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components);
            notifyIcon.Click += NotifyIcon_Click;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            notifyIcon.Visible = false;
            CreateTextIcon();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(stateFilePath);
                textBox1.Text = lines[0];
                textBox2.Text = lines[1];
                textBox3.Text = lines[2];
            }
            catch { }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                Dispatcher.Invoke(() =>
                {
                    Show();
                    notifyIcon.Visible = false;
                });
            });
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            Hide();
            CreateTextIcon();
            notifyIcon.Visible = true;
        }

        private readonly string stateFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrayDate.txt");
        private void SaveState()
        {
            try
            {
                System.IO.File.WriteAllLines(stateFilePath, new string[] { textBox1.Text, textBox2.Text, textBox3.Text });
            }
            catch { }
        }

        private Color GetColor()
        {
            Color color = Color.White;
            try
            {
                string[] colorParts = new System.Text.RegularExpressions.Regex("[^a-fA-F0-9]")
                    .Split(textBox3.Text)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
                switch (colorParts.Length)
                {
                    case 1:
                        string c = colorParts[0];
                        switch (c.Length)
                        {
                            case 3:
                                color = Color.FromArgb(Convert.ToInt32($"FF{c[0]}0{c[1]}0{c[2]}0", 16));
                                break;
                            case 4:
                                color = Color.FromArgb(Convert.ToInt32($"{c[0]}{c[0]}{c[1]}{c[1]}{c[2]}{c[2]}{c[3]}{c[3]}", 16));
                                break;
                            case 6:
                                color = Color.FromArgb(Convert.ToInt32("FF" + c, 16));
                                break;
                            case 8:
                                color = Color.FromArgb(Convert.ToInt32(c, 16));
                                break;
                            default:
                                break;
                        }
                        break;
                    case 3:
                        color = Color.FromArgb(int.Parse(colorParts[0]), int.Parse(colorParts[1]), int.Parse(colorParts[2]));
                        break;
                    case 4:
                        color = Color.FromArgb(int.Parse(colorParts[0]), int.Parse(colorParts[1]), int.Parse(colorParts[2]), int.Parse(colorParts[3]));
                        break;
                    default: break;
                }
            }
            catch { }
            return color;
        }

        private void CreateTextIcon()
        {
            Rect screen = WpfScreen.GetScreenFrom(this).DeviceBounds;
            double width = SystemParameters.FullPrimaryScreenWidth;
            double scale = screen.Width / width;
            int iconWidth = (int)(SystemParameters.SmallIconWidth * scale);

            Color color = GetColor();
            string str = string.Join("\n", new System.Text.RegularExpressions.Regex(@"\W").Split(DateTime.Now.ToString(textBox1.Text)));
            float fontHeight = (float)Math.Round(iconWidth * 6 / 7.0);
            Font fontToUse = new Font(System.Drawing.SystemFonts.DefaultFont.Name, fontHeight, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(color);
            Bitmap bitmapText = new Bitmap(iconWidth, iconWidth);
            Graphics g = Graphics.FromImage(bitmapText);

            IntPtr hIcon;
            SizeF size = g.MeasureString(str, fontToUse);
            double maxSize = iconWidth + (scale > 1.3 ? scale * 4 : 4);
            while (size.Width > maxSize - 3 || size.Height > maxSize)
            {
                fontHeight -= 1;
                fontToUse = new Font("Tahoma", fontHeight, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
                size = g.MeasureString(str, fontToUse);
            }

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Brush lineBrush = new SolidBrush(Color.CadetBlue);
            Pen pen = new Pen(lineBrush);
            int x = bitmapText.Width - 1, y = bitmapText.Height - 1;
            int d = (bitmapText.Width + bitmapText.Height) / 8;
            g.DrawRectangle(pen, 0, 0, x, y);
            g.DrawLine(pen, x - d, y, x, y - d);
            g.DrawLine(pen, x - d + 1, y, x, y - d + 1);
            g.DrawString(str, fontToUse, brushToUse, (bitmapText.Width - size.Width) / 2, (bitmapText.Height - size.Height) / 2);

            hIcon = bitmapText.GetHicon();
            notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
            notifyIcon.Text = DateTime.Now.ToString(textBox2.Text);

            SaveState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            CreateTextIcon();
            notifyIcon.Visible = true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Title = "Show Date";
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Title = DateTime.Now.ToString(textBox2.Text);
            SaveState();
        }
    }
}
