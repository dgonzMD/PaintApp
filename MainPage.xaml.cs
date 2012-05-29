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
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Purple);
            line.StrokeThickness = 15;

            Point point1 = new Point();
            point1.X = 10.0;
            point1.Y = 100.0;

            Point point2 = new Point();
            point2.X = 150.0;
            point2.Y = 100.0;

            line.X1 = point1.X;
            line.Y1 = point1.Y;
            line.X2 = point2.X;
            line.Y2 = point2.Y;

            canvas1.Children.Add(line);
        }
    }
}