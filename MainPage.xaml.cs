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
        private Point currentPoint;
        private Point oldPoint;
        private Line line;
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
            currentPoint = e.GetPosition(this.canvas1);
            line = new Line() { X1 = currentPoint.X, Y1 = currentPoint.Y, X2 = oldPoint.X, Y2 = oldPoint.Y };
            line.Stroke = Globals.scb;
            line.StrokeThickness = 8;
            line.StrokeStartLineCap = PenLineCap.Round;

            this.canvas1.Children.Add(line);
            oldPoint = currentPoint;
            fill = false;
        }

        void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentPoint = e.GetPosition(canvas1);
            oldPoint = currentPoint;         
        }
        void canvas1_Tap(object sender, GestureEventArgs e)
        {
            if (!fill) return;

            currentPoint = e.GetPosition(canvas1);

            bm = new WriteableBitmap(canvas1, null);
            
            //Uri uri = new Uri("images/temp1.png", UriKind.Relative); //dunno why this is here
            //ImageSource img = bm;
            canvas1.t
            flood(currentPoint);
            bm.Invalidate();

            Image image = new Image();
            image.SetValue(Image.SourceProperty, bm);

            
            this.canvas1.Children.Add(image);
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

        private bool isValid(int i, int j)
        { return i>0 && i<bm.PixelWidth && j>0 && j<bm.PixelWidth; } //WHY is it not <= 0 ???

        private void flood(Point p)
        {
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(p);
            Color toReplace = bm.GetPixel((int)p.X, (int)p.Y);
            bm.SetPixel((int)p.X, (int)p.Y, Globals.scb.Color);

            while (q.Count > 0)
            {
                p = q.Dequeue();

                for (int i = 0; i < 8; i++)
                {
                    int x = (int)p.X + Globals.di[i];
                    int y = (int)p.Y + Globals.dj[i];
                    if (isValid(x, y) && toReplace == bm.GetPixel(x, y))
                    {
                        q.Enqueue(new Point(x, y));
                        bm.SetPixel(x, y, Globals.scb.Color);
                    }
                }
            }
        }
    }
}
