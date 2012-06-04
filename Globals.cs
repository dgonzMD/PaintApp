using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PaintApp
{
    public static class Globals
    {
        public static SolidColorBrush scb= new SolidColorBrush(Colors.Black);
        public static int brushSize= 8;
        public static PenLineCap plc= PenLineCap.Round;
    }
}
