using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace PaintApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private iPoint cur_p = new iPoint(0,0);
        private iPoint prev_p = new iPoint(0,0);
        private bool fill = false;
        private WriteableBitmap bm;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.canvas1.MouseMove += new MouseEventHandler(canvas1_MouseMove);
            this.canvas1.MouseLeftButtonDown += new MouseButtonEventHandler(canvas1_MouseLeftButtonDown);
            Globals.scb = new SolidColorBrush(Colors.Black);
        }

        void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (fill) return;

            cur_p = new iPoint(e.GetPosition(this.canvas1));
            Line line = new Line() { X1 = cur_p.x, Y1 = cur_p.y, X2 = prev_p.x, Y2 = prev_p.y };
            line.Stroke = Globals.scb;
            line.StrokeThickness = 8;
            line.StrokeStartLineCap = PenLineCap.Round;

            this.canvas1.Children.Add(line);
            prev_p = cur_p;
        }

        void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            cur_p.x = (short)e.GetPosition(canvas1).X;
            cur_p.y = (short)e.GetPosition(canvas1).Y;

            prev_p = cur_p;         
        }
        void canvas1_Tap(object sender, GestureEventArgs e)
        {
            if (!fill) return;
            long before = DateTime.Now.Ticks;           

            cur_p.x  = (short)e.GetPosition(canvas1).X;
            cur_p.y = (short)e.GetPosition(canvas1).Y;

            Image image = new Image();
            bm = new WriteableBitmap(canvas1, null);

            ImageSource img = bm;
            image.SetValue(Image.SourceProperty, bm);

            flood(cur_p, Globals.scb.Color, bm.GetPixel(cur_p.x, cur_p.y));
            bm.Invalidate();
            this.canvas1.Children.Add(image);

            long after = DateTime.Now.Ticks;

            TimeSpan elapsedTime = new TimeSpan(after - before);
            MessageBox.Show(string.Format("Task took {0} milliseconds",
                elapsedTime.TotalMilliseconds));

        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            this.canvas1.Children.Clear();
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ColorPicker.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            button2.Background = Globals.scb;
        }

        private void fillButton_Click(object sender, RoutedEventArgs e)
        {
            fill ^= true;
            if (fill)
                button3.Content = "Fill";
            else
                button3.Content = "Pen";
        }

        private bool isValid(short i, short j)
        { return i>=0 && i<bm.PixelWidth && j>=0 && j<bm.PixelHeight; } 

        private void flood(iPoint init, Color fillColor, Color interiorColor)
        {
            if (fillColor == interiorColor) return;

            iPoint[] q = new iPoint[(int)(canvas1.Width * canvas1.Height)];
            int len = 0;

            q[len] = init;
            len++;
            bm.SetPixel(init.x, init.y, fillColor);

            short[] di = { -1, -1, -1, 0, 1, 1, 1, 0 };
            short[] dj = { -1, 0, 1, 1, 1, 0, -1, -1 };

            while (len > 0)
            {
                iPoint p = q[len-1];
                len--;

                for (int i = 0; i < 8; i++)
                {
                    short x = (short)(p.x + di[i]);
                    short y = (short)(p.y + dj[i]);
                    if (isValid(x, y) && interiorColor == bm.GetPixel(x, y))
                    {
                        q[len] = new iPoint(x, y);
                        len++;
                        bm.SetPixel(x, y, fillColor);
                    }
                }
            }
        }
    }
}

class iPoint
{
    public short x, y; // can possibly use shorts
    public iPoint(short X, short Y)
    { x = X; y = Y; }

    public iPoint(Point p)
    { x = (short)p.X; y = (short)p.Y; }
}