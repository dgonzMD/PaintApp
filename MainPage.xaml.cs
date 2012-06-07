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
using Microsoft.Phone;

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
        private CameraCaptureTask ctask;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.canvas1.MouseMove += new MouseEventHandler(canvas1_MouseMove);
            this.canvas1.MouseLeftButtonDown += new MouseButtonEventHandler(canvas1_MouseLeftButtonDown);

            pct = new PhotoChooserTask();
            pct.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);

            ctask = new CameraCaptureTask();
            ctask.Completed += new EventHandler<PhotoResult>(ctask_Completed);
        }

        void ctask_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {
                bm = new WriteableBitmap(canvas1, null);

                int width = bm.PixelWidth;
                int height = bm.PixelHeight;

                //Take JPEG stream and decode into a WriteableBitmap object
                bm = PictureDecoder.DecodeJpeg(e.ChosenPhoto, height, width);
                bm = bm.Rotate(1);
                //Collapse visibility on the progress bar once writeable bitmap is visible.
                //progressBar1.Visibility = Visibility.Collapsed;


                //Populate image control with WriteableBitmap object.
                bm.Invalidate();
                updateCanvasFromWBM(bm);

                //Once writeable bitmap has been rendered, the crop button
                //is enabled.
                //btnCrop.IsEnabled = true;

                //textStatus.Text = "Tap the crop button to proceed";
            }
            else
            {
                makeToast("Success: ", "Loaded Picture");
            }
        }

        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                updateUndoList();

                bm = new WriteableBitmap(canvas1, null);

                double width = bm.PixelWidth;
                double height = bm.PixelHeight;

                bm.SetSource(e.ChosenPhoto);
                
                double scaleX = bm.PixelWidth / width;
                double scaleY = bm.PixelHeight / height;

                //For debugging
                //System.Diagnostics.Debug.WriteLine(width + " - " + height);
                //System.Diagnostics.Debug.WriteLine(bm.PixelWidth + " :: " + bm.PixelHeight);
                //System.Diagnostics.Debug.WriteLine(scaleX + " = " + scaleY);

                bm = bm.Resize((int)(bm.PixelWidth / scaleX), 
                               (int)(bm.PixelHeight / scaleY), 
                               WriteableBitmapExtensions.Interpolation.Bilinear);

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

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            border2.Background = Globals.scb;
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (toolState>0) return;

            cur_p = new iPoint(e.GetPosition(this.canvas1));

            /* Frank: My own custom drawline with variable thickness (still needs some work)
            bm = new WriteableBitmap(canvas1, null);

            int x1 = cur_p.x;
            int y1 = cur_p.y;
            int x2 = prev_p.x;
            int y2 = prev_p.y;

            double pp_x = y1 - y2;
            double pp_y = x2 - x1;
            double mag = Math.Sqrt(pp_x * pp_x + pp_y * pp_y);
            
            //normalize
            pp_x /= mag;
            pp_y /= mag;

            for (int i = 0; i < Globals.brushSize; i++)
                bm.DrawLine((int)(x1 + pp_x * i), (int)(y1 + pp_y * i), (int)(x2 + pp_x * i), (int)(y2 + pp_y * i), Globals.scb.Color);

            bm.Invalidate();            
            updateCanvasFromWBM(bm);
            */

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
                border2.Background = Globals.scb;
                makeToast("Color Sampled", "");
                fillClick(null, null); //to move to pencil mode
                //NavigationService.Navigate(new Uri("/ColorPicker.xaml", UriKind.Relative));
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
                
                flood3(cur_p.y*bm.PixelWidth + cur_p.x, 
                       Globals.getColorAsInt(), 
                       bm.Pixels[cur_p.x + cur_p.y*bm.PixelWidth]);

                bm.Invalidate();
                updateCanvasFromWBM(bm);
                
                //Used to measure performance
                long after = DateTime.Now.Ticks;
                TimeSpan elapsedTime = new TimeSpan(after - before);
                
                makeToast("Bucket Applied -", string.Format(" {0}ms", elapsedTime.TotalMilliseconds), 500);
            }
        }

        private void colorClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ColorPicker.xaml", UriKind.Relative));
        }

        //Frank: such a misleading name..
        //(this is the eventhandler for the 'tools' appbutton
        private void fillClick(object sender, EventArgs e)
        {
            toolState = (toolState + 1) % 3;
            ApplicationBarIconButton b = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
            switch(toolState)
            {
                case 0:
                    b.IconUri = new Uri("/Images/edit.png", UriKind.Relative);
                    b.Text = "Pen";
                    makeToast("Pen Mode", "", 500, "/Images/edit.png");
                    break;
                case 1:
                    b.IconUri = new Uri("/Images/beer.png", UriKind.Relative);
                    b.Text = "Fill";
                    makeToast("Fill Mode", "", 500, "/Images/beer.png");
                    break;
                case 2:
                    b.IconUri = new Uri("/Images/questionmark.png", UriKind.Relative);
                    b.Text = "Sample";
                    makeToast("Color Sampling Mode", "", 500, "Images/questionmark.png");
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

        private void pictureClick(object sender, EventArgs e)
        {
            ctask.Show();
        }

        //From http://en.wikipedia.org/wiki/Hue
        private double sqrt3 = Math.Sqrt(3.0);
        private double getHue(Color a)
        {
            return Math.Atan2(sqrt3 * (a.G - a.B), 2 * a.R - a.G - a.B);
        }

        //From http://en.wikipedia.org/wiki/Luminance_(relative)
        //Y = 0.2126 R + 0.7152 G + 0.0722 B
        private double getLuminance(Color a)
        {
            return 0.2126 * a.R + 0.7152 * a.G + 0.0722 * a.B;
        }

        private double colorDist(double x1, double y1, double x2, double y2)
        {
            return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        }

        private void flood3(int p, int replacementColor, int targetColor) //Scanline algorithm
        {
            if (replacementColor == targetColor) return;

            Queue<int> q = new Queue<int>();
            q.Enqueue(p);

            int[] img = bm.Pixels;
            int pixelCt = bm.PixelWidth * bm.PixelHeight;

            while (q.Count > 0)
            {
                int c = q.Dequeue();
                if (img[c] != targetColor) continue;

                int stop = c - c % bm.PixelWidth; //leftmost point of that particular row
                for (int a = c; a >= stop && img[a] == targetColor; --a)
                {
                    img[a] = replacementColor;

                    if (a > bm.PixelWidth && img[a-bm.PixelWidth] == targetColor) //up
                        q.Enqueue(a-bm.PixelWidth);

                    if (a + bm.PixelWidth < pixelCt && img[a+bm.PixelWidth] == targetColor) //down
                        q.Enqueue(a+bm.PixelWidth);
                }

                stop += bm.PixelWidth;
                for (int a = c+1; a < stop && img[a] == targetColor; ++a)
                {
                    img[a] = replacementColor;

                    if (a > bm.PixelWidth && img[a - bm.PixelWidth] == targetColor) //up
                        q.Enqueue(a - bm.PixelWidth);

                    if (a + bm.PixelWidth < pixelCt && img[a + bm.PixelWidth] == targetColor) //down
                        q.Enqueue(a + bm.PixelWidth);
                }
            }

            return;
        }

        //Helper function used to display a toast
        private void makeToast(string title, string text, int duration = 1000, string uri = "Images/check.png")
        {
            ToastPrompt toast = new ToastPrompt();
            toast.MillisecondsUntilHidden = duration;
            toast.Title = title;
            toast.Message = text;
            toast.FontSize = 28;
            toast.Height = 80;
            toast.TextOrientation = System.Windows.Controls.Orientation.Horizontal;
            toast.ImageSource = new BitmapImage(new Uri(uri, UriKind.Relative));
            toast.Show();
        }

        private void helpClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/instructions.xaml", UriKind.Relative));
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