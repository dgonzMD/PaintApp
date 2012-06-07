using System.Windows.Media;

namespace PaintApp
{
    public static class Globals
    {
        public static SolidColorBrush scb= new SolidColorBrush(Colors.Black);
        public static int brushSize= 8;
        public static PenLineCap plc= PenLineCap.Round;
        
        public static int getColorAsInt()
        {
            Color c = scb.Color;
            return (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
        }
    }
}
