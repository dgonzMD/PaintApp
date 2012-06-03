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
    public partial class ColorPicker : PhoneApplicationPage
    {
        public ColorPicker()
        {
            InitializeComponent();
            slider1.Value = Globals.brushSize;
            brushText.Text=string.Format("Brush Size: {0}", slider1.Value);
            if (Globals.plc == PenLineCap.Round)
                roundButton.BorderBrush = new SolidColorBrush(Colors.Yellow);
            else if(Globals.plc == PenLineCap.Square)
                squareButton.BorderBrush = new SolidColorBrush(Colors.Yellow);
            else
                triangleButton.BorderBrush = new SolidColorBrush(Colors.Yellow);
        }

        private void colorHexagonPicker_ColorChanged(object sender, Color color)
        {
            Globals.scb = colorHexagonPicker.SolidColorBrush;
        }

        private void AppBarConfirm(object o, EventArgs e)
        {
            NavigationService.GoBack();

            //.Navigate() calls the constructor again, I don't want that. I just want to call the 
            //event handler for the page load.
            //NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Globals.brushSize = (int)slider1.Value;
            brushText.Text = string.Format("Brush Size: {0}", Globals.brushSize);
        }

        private void roundButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.plc = PenLineCap.Round;
            roundButton.BorderBrush = new SolidColorBrush(Colors.Yellow);
            squareButton.BorderBrush = new SolidColorBrush(Colors.White);
            triangleButton.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void SquareButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.plc = PenLineCap.Square;
            roundButton.BorderBrush = new SolidColorBrush(Colors.White);
            squareButton.BorderBrush = new SolidColorBrush(Colors.Yellow);
            triangleButton.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void triangleButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.plc = PenLineCap.Triangle;
            roundButton.BorderBrush = new SolidColorBrush(Colors.White);
            squareButton.BorderBrush = new SolidColorBrush(Colors.White);
            triangleButton.BorderBrush = new SolidColorBrush(Colors.Yellow);
        }
    }
}