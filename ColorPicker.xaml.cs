using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace PaintApp
{
    public partial class ColorPicker : PhoneApplicationPage
    {
        public ColorPicker()
        {
            InitializeComponent();
            //slider1.Value = Globals.brushSize; set in xaml instead
            brushText.Text=string.Format("Brush Size: {0}", slider1.Value);
            colorRect.Fill = Globals.scb;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            picker.Color = Globals.scb.Color;
            slider1.Value = Globals.brushSize;
        }

        private void picker_ColorChanged(object sender, Color color)
        {
            Globals.scb = new SolidColorBrush(color);
            colorRect.Fill = Globals.scb;
        }

        private void AppBarConfirm(object o, EventArgs e)
        {
            NavigationService.GoBack();
            //.Navigate() calls the constructor again, I don't want that. I just want to call the event handler for the page load.
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider1 == null) return;

            if (slider1.Value == 0.0) slider1.Value = 1.0;

            Globals.brushSize = (int)slider1.Value;
            brushText.Text = string.Format("Brush Size: {0}", Globals.brushSize);
        }
    }
}