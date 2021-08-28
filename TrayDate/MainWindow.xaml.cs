using System;
using System.Drawing;
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

        private readonly string stateFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TrayDate.txt");
        private void SaveState()
        {
            try
            {
                System.IO.File.WriteAllLines(stateFilePath, new string[] { textBox1.Text, textBox2.Text });
            }
            catch { }
        }

        private void CreateTextIcon()
        {
            Rect screen = WpfScreen.GetScreenFrom(this).DeviceBounds;
            double width = SystemParameters.FullPrimaryScreenWidth;
            int iconWidth = (int)(SystemParameters.SmallIconWidth * screen.Width / width);

            string str = DateTime.Now.ToString(textBox1.Text);
            int fontHeight = (int)Math.Round(iconWidth * 6 / 7.0);
            Font fontToUse = new Font("Tahoma", fontHeight, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Bitmap bitmapText = new Bitmap(iconWidth, iconWidth);
            Graphics g = Graphics.FromImage(bitmapText);

            IntPtr hIcon;
            SizeF size = g.MeasureString(str, fontToUse);

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.DrawString(str, fontToUse, brushToUse, (bitmapText.Width - size.Width) / 2, (bitmapText.Height - size.Height) / 2);
            Brush lineBrush = new SolidBrush(Color.CadetBlue);
            Pen pen = new Pen(lineBrush);
            int x = bitmapText.Width - 1, y = bitmapText.Height - 1;
            int d = (bitmapText.Width + bitmapText.Height) / 8;
            g.DrawRectangle(pen, 0, 0, x, y);
            g.DrawLine(pen, x - d, y, x, y - d);
            g.DrawLine(pen, x - d + 1, y, x, y - d + 1);
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
