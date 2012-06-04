using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
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
        private int toolState = 0;
        private bool activelyDrawing = false;
        private WriteableBitmap bm;
        private IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication(); //Used in save
        private PhotoChooserTask pct;
        private WriteableBitmap undoBm = null;
        private LinkedList<WriteableBitmap> undoList = new LinkedList<WriteableBitmap>();
        private int maxUndos = 50;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.canvas1.MouseMove += new MouseEventHandler(canvas1_MouseMove);
            this.canvas1.MouseLeftButtonDown += new MouseButtonEventHandler(canvas1_MouseLeftButtonDown);
            Globals.scb = new SolidColorBrush(Colors.Black);
            Globals.brushSize = 8;
            Globals.plc = PenLineCap.Round;

            pct = new PhotoChooserTask();
            pct.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }

        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                updateUndoList();

                bm = new WriteableBitmap(canvas1, null);
                bm.SetSource(e.ChosenPhoto);
                updateCanvasFromWBM(bm);

                makeToast("", "Load Successful");

                //Frank: Look into cleaning up this Imagesource mess later with the following trick
                //Code to display the photo on the page in an image control named myImage.
                //System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                //bmp.SetSource(e.ChosenPhoto);
                //myImage.Source = bmp;
            }
            else
            {
                makeToast("Uh oh!", "Load Failed");
            }
        }

        //Frank: Remove this if you are not using it. (Don't forget to remove it from the xaml too)
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (toolState>0) return;

            cur_p = new iPoint(e.GetPosition(this.canvas1));
            Line line = new Line() { X1 = cur_p.x, Y1 = cur_p.y, X2 = prev_p.x, Y2 = prev_p.y };
            line.Stroke = new SolidColorBrush(Globals.scb.Color);
            line.StrokeThickness = Globals.brushSize;
            line.StrokeStartLineCap = Globals.plc;

            this.canvas1.Children.Add(line);
            prev_p = cur_p;
        }

        private void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (toolState == 0)
            {
                canvas1.CaptureMouse();
                if (!activelyDrawing && toolState == 0) //just started to draw, save state first (in order to undo)
                {
                    updateUndoList();
                    activelyDrawing = true;
                }

                cur_p.x = (short)e.GetPosition(canvas1).X;
                cur_p.y = (short)e.GetPosition(canvas1).Y;

                prev_p = cur_p;
            }
            else if (toolState == 2)
            {
                bm = new WriteableBitmap(canvas1, null);
                Globals.scb.Color = bm.GetPixel((int)e.GetPosition(canvas1).X, (int)e.GetPosition(canvas1).Y);
    //            NavigationService.Navigate(new Uri("/ColorPicker.xaml", UriKind.Relative));
            }
        }

        private void canvas1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas1.ReleaseMouseCapture();
            activelyDrawing = false;
        }

        //updates canvas1 to the whatever WriteableBitmap you give it
        private void updateCanvasFromWBM(WriteableBitmap b)
        {
            Image image = new Image();
            image.Source = b;

            canvas1.Children.Clear();
            canvas1.Children.Add(image);
        }
        private void updateUndoList()
        {
            if (undoList.Count >= maxUndos)
                undoList.RemoveLast();
            undoList.AddFirst(new WriteableBitmap(canvas1,null));
        }
        private void canvas1_Tap(object sender, GestureEventArgs e)
        {
            if (toolState == 1)
            {
                long before = DateTime.Now.Ticks;

                undoBm = new WriteableBitmap(canvas1, null); //UNDOop
                updateUndoList();
            
                cur_p.x  = (short)e.GetPosition(canvas1).X;
                cur_p.y = (short)e.GetPosition(canvas1).Y;

                bm = new WriteableBitmap(canvas1, null);
                
                flood(cur_p, Globals.scb.Color, bm.GetPixel(cur_p.x, cur_p.y));
                bm.Invalidate();

                updateCanvasFromWBM(bm);
                
                //Used to measure performance
                long after = DateTime.Now.Ticks;
                TimeSpan elapsedTime = new TimeSpan(after - before);
                
                makeToast("Time: ", string.Format(" {0} milliseconds", elapsedTime.TotalMilliseconds));
            }
        }

        private void colorClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ColorPicker.xaml", UriKind.Relative));
        }

        private void fillClick(object sender, EventArgs e)
        {
            toolState = (toolState + 1) % 3;
            ApplicationBarIconButton b = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
            switch(toolState)
            {
                case 0:
                    b.IconUri = new Uri("/Images/edit.png", UriKind.Relative);
                    b.Text = "Pen";
                    break;
                case 1:
                    b.IconUri = new Uri("/Images/beer.png", UriKind.Relative);
                    b.Text = "Fill";
                    break;
                case 2:
                    b.IconUri = new Uri("/Images/questionmark.png", UriKind.Relative);
                    b.Text = "Query";
                    break;
            }
            
        }

        //Clear Button
        private void clearClick(object sender, EventArgs e)
        {
            updateUndoList();
            canvas1.Children.Clear();
        }

        //Save Button
        private void saveClick(object sender, EventArgs e)
        {
            //Save the writeable bitmap into a temporary file called img.jpg
            IsolatedStorageFileStream fileStream = storage.CreateFile("img.jpg");

            bm = new WriteableBitmap(canvas1, null);
            bm.SaveJpeg(fileStream, bm.PixelWidth, bm.PixelHeight, 0, 85);
            fileStream.Close();

            fileStream = storage.OpenFile("img.jpg", FileMode.Open, FileAccess.Read);

            MediaLibrary mediaLibrary = new MediaLibrary();
            Picture pic = mediaLibrary.SavePicture("paint_img.jpg", fileStream);
            fileStream.Close();

            makeToast("Success!", "File Saved");
        }

        //Undo button
        private void undoClick(object sender, EventArgs e) 
        {
            if (undoList.Count<1) return;

            canvas1.Children.Clear();
            WriteableBitmap tempbm = undoList.First.Value;
            undoList.RemoveFirst();

            Image image = new Image();
            ImageSource img = tempbm;
            image.SetValue(Image.SourceProperty, tempbm);
            canvas1.Children.Add(image);
        }

        private void loadClick(object sender, EventArgs e)
        {
            pct.Show();
        }

        //From http://en.wikipedia.org/wiki/Hue
        private double sqrt3 = Math.Sqrt(3.0);
        private double getHue(Color a)
        {
            return Math.Atan2(sqrt3 * (a.G - a.B), 2 * a.R - a.G - a.B);
        }

        private void flood(iPoint p, Color fillColor, Color interiorColor)
        {
            if (fillColor == interiorColor) return;

            Queue<iPoint> q = new Queue<iPoint>();
            q.Enqueue(p);
            bm.SetPixel(p.x, p.y, fillColor);

            short[] di = { -1, 0, 0, 1 };
            short[] dj = { 0, 1, -1, 0 };

            //double targetHue = getHue(interiorColor);
            //double tolerance = 5.0;
            while (q.Count > 0)
            {
                p = q.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    short x = (short)(p.x + di[i]);
                    short y = (short)(p.y + dj[i]);

                    if (x >= 0 && x < bm.PixelWidth && y >= 0 && y < bm.PixelHeight)
                    {
                        Color cur = bm.GetPixel(x, y);
                        if (cur == fillColor) continue;
                        if (interiorColor == cur /*|| Math.Abs(targetHue - getHue(cur)) < tolerance*/)
                        {
                            q.Enqueue(new iPoint(x, y));
                            bm.SetPixel(x, y, fillColor);
                        }
                    }
                }
            }

            return;
        }

        //Helper function used to display a toast
        private void makeToast(string title, string text, double duration = 1000)
        {
            ToastPrompt toast = new ToastPrompt();
            toast.MillisecondsUntilHidden = 1000;
            toast.Title = title;
            toast.Message = text;
            toast.FontSize = 30;
            toast.TextOrientation = System.Windows.Controls.Orientation.Horizontal;
            toast.ImageSource = new BitmapImage(new Uri("ApplicationIcon.png", UriKind.Relative));
            toast.Show();
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