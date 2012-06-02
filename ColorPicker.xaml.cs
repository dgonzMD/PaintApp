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
    }
}