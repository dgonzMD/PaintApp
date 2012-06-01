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
using System.IO.IsolatedStorage;
using System.Windows.Resources;
using Microsoft.Phone.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Coding4Fun.Phone.Controls;

namespace PaintApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private iPoint cur_p = new iPoint(0,0);
        private iPoint prev_p = new iPoint(0,0);
        private bool fillModeOn = false;
        private bool activelyDrawing = false;
        private WriteableBitmap bm;
        private Canvas undoCanvas = null;
        private IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication(); //Used in save

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
            if (fillModeOn) return;

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
            if (!activelyDrawing) //just started to draw, save state first (in order to undo)
            {
                undoCanvas = new Canvas();

                bm = new WriteableBitmap(canvas1, null);
                Image image = new Image();
                ImageSource img = bm;
                image.SetValue(Image.SourceProperty, bm);
                undoCanvas.Children.Add(image);

                activelyDrawing = true;
            }

            cur_p.x = (short)e.GetPosition(canvas1).X;
            cur_p.y = (short)e.GetPosition(canvas1).Y;

            prev_p = cur_p;         
        }

        void canvas1_Tap(object sender, GestureEventArgs e)
        {
            if (!fillModeOn) return;
            long before = DateTime.Now.Ticks;

            bm = new WriteableBitmap(canvas1, null);

            undoCanvas = new Canvas();
            while (canvas1.Children.Count > 0)
            {
                UIElement child = canvas1.Children[0];
                canvas1.Children.RemoveAt(0);
                undoCanvas.Children.Add(child);
            }

            cur_p.x  = (short)e.GetPosition(canvas1).X;
            cur_p.y = (short)e.GetPosition(canvas1).Y;

            Image image = new Image();
            ImageSource img = bm;
            image.SetValue(Image.SourceProperty, bm);

            flood(cur_p, Globals.scb.Color, bm.GetPixel(cur_p.x, cur_p.y));
            bm.Invalidate();
            this.canvas1.Children.Add(image);

            //Used to measure performance
            long after = DateTime.Now.Ticks;
            TimeSpan elapsedTime = new TimeSpan(after - before);
            ToastPrompt toast = new ToastPrompt();
            toast.MillisecondsUntilHidden = 1000;
            toast.Title = "Time: ";
            toast.Message = string.Format(" {0} milliseconds", elapsedTime.TotalMilliseconds);
            toast.FontSize = 30;
            toast.TextOrientation = System.Windows.Controls.Orientation.Horizontal;
            toast.ImageSource = new BitmapImage(new Uri("ApplicationIcon.png", UriKind.Relative));
            toast.Show();
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            undoCanvas = new Canvas();
            while (canvas1.Children.Count > 0)
            {
                UIElement ele = canvas1.Children[0];
                canvas1.Children.RemoveAt(0);
                undoCanvas.Children.Add(ele);
            }
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
            fillModeOn ^= true;
            if (fillModeOn)
                button3.Content = "Fill";
            else
                button3.Content = "Pen";
        }

        private void flood(iPoint p, Color fillColor, Color interiorColor)
        {
            if (fillColor == interiorColor) return;

            Queue<iPoint> q = new Queue<iPoint>();
            q.Enqueue(p);
            bm.SetPixel(p.x, p.y, fillColor);

            short[] di = { -1, 0, 0, 1 };
            short[] dj = { 0, 1, -1, 0 };

            while (q.Count > 0)
            {
                p = q.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    short x = (short)(p.x + di[i]);
                    short y = (short)(p.y + dj[i]);

                    if (x>=0 && x<bm.PixelWidth && y>=0 && y<bm.PixelHeight && interiorColor == bm.GetPixel(x, y))
                    {
                        q.Enqueue(new iPoint(x, y));
                        bm.SetPixel(x, y, fillColor);
                    }
                }
            }

            return;
        }

        private void saveFile() 
        {
            String tempJPEG = "img.jpg";

            //Save the writeable bitmap into a temporary file called img.jpg
            IsolatedStorageFileStream fileStream = storage.CreateFile(tempJPEG);

            bm = new WriteableBitmap(canvas1, null);
            bm.SaveJpeg(fileStream, bm.PixelWidth, bm.PixelHeight, 0, 85);
            fileStream.Close();

            fileStream = storage.OpenFile("img.jpg", FileMode.Open, FileAccess.Read);

            MediaLibrary mediaLibrary = new MediaLibrary();
            Picture pic = mediaLibrary.SavePicture("paint_img.jpg", fileStream);
            fileStream.Close();            
        }

        //Save Button
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            saveFile();
            //MessageBox.Show("File Saved."); //removed, the ToastPrompts look better

            ToastPrompt toast = new ToastPrompt();

            toast.MillisecondsUntilHidden = 1000;
            toast.Title = "Success!";
            toast.Message = "File Saved";
            toast.FontSize = 30;
            toast.TextOrientation = System.Windows.Controls.Orientation.Horizontal;
            toast.ImageSource = new BitmapImage(new Uri("ApplicationIcon.png", UriKind.Relative));
            toast.Show();
        }

        private void button5_Click(object sender, RoutedEventArgs e) //Undo button
        {
            if (undoCanvas == null) return; //Can't undo

            //System.Diagnostics.Debug.WriteLine("undo-ing!!!!!!!"); //save to remove

            canvas1.Children.Clear();
            while (undoCanvas.Children.Count > 0)
            {
                UIElement child = undoCanvas.Children[0];
                undoCanvas.Children.RemoveAt(0);
                canvas1.Children.Add(child);
            }

            undoCanvas = null;
        }

        private void canvas1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            activelyDrawing = false;
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