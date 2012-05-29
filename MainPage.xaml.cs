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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace PaintApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Point currentPoint;
        private Point oldPoint;
        private Line line;

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
        }

        void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentPoint = e.GetPosition(canvas1);
            oldPoint = currentPoint;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Globals.scb = new SolidColorBrush(Colors.Blue);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Globals.scb = new SolidColorBrush(Colors.Green);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Globals.scb = new SolidColorBrush(Colors.Red);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Globals.scb = new SolidColorBrush(Colors.Yellow);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            Globals.scb = new SolidColorBrush(Colors.Black);
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
    }
}
