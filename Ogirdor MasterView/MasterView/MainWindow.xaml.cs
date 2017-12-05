using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterView
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        FileInfo currentFile = null;
        Point lastPoint = new Point();
        Thickness lastPosition = new Thickness();
        Size currentSize = new Size();
        Stretch currentStretch = Stretch.Uniform;
        int currentZoom = 1000;
        bool isPixelMode = false;
        bool isBlackMode = true;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                string fname = Application.Current.Properties["ArbitraryArgName"].ToString();
                showImage(fname);
            }
            else
            {
                showImage(@"test.png");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (currentStretch == Stretch.None)
            {
                rePosition(false, true, 0);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (currentStretch == Stretch.None)
            {
                var lastZoom = currentZoom;
                var xZoom = currentZoom + e.Delta;
                if (xZoom < 10000 && Math.Floor(currentSize.Width * xZoom / 1000) > 1 && Math.Floor(currentSize.Height * xZoom / 1000) > 1)
                {
                    currentZoom = xZoom;
                    xZoomValue.Text = "Zoom : " + (currentZoom / 10) + "%";
                    rePosition(false, false, currentZoom - lastZoom);
                }
            }
        }

        private void xFrame_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentStretch == Stretch.None && e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(xFrame);
                var diff = pos - lastPoint;
                xImage.Margin = new Thickness(lastPosition.Left + diff.X, lastPosition.Top + diff.Y, 0, 0);
            }
        }

        private void xFrame_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentStretch == Stretch.None)
            {
                lastPoint = e.GetPosition(xFrame);
                lastPosition = new Thickness(xImage.Margin.Left, xImage.Margin.Top, xImage.Margin.Right, xImage.Margin.Bottom);

                var imgPoint = e.GetPosition(xImage);
                var bmap = (BitmapImage)xImage.Source;
            }
        }

        private void showImage(string file)
        {
            try
            {
                BitmapImage bmap = new BitmapImage();
                bmap.BeginInit();
                bmap.UriSource = new Uri(file, UriKind.Relative);
                bmap.CacheOption = BitmapCacheOption.OnLoad;
                bmap.EndInit();

                currentFile = new FileInfo(file);
                lastPoint = new Point();
                lastPosition = new Thickness();
                currentSize = new Size(bmap.PixelWidth, bmap.PixelHeight);
                currentStretch = Stretch.Uniform;
                currentZoom = 1000;

                xImage.Source = bmap;
                xImage.Stretch = Stretch.Uniform;
                rePosition(true, false, 0);

                this.Title = "Ogirdor MasterView | " + currentFile.Name;
                xZoomValue.Text = "";
                xColorValue.Text = "";
            }
            catch (Exception)
            {
            }
        }

        private void rePosition(bool fromCenterImage, bool fromTopLleft, int zoomFactor)
        {
            if (currentStretch == Stretch.None)
            {
                xImage.Width = currentSize.Width * currentZoom / 1000;
                xImage.Height = currentSize.Height * currentZoom / 1000;
                xImage.HorizontalAlignment = HorizontalAlignment.Left;
                xImage.VerticalAlignment = VerticalAlignment.Top;
                if (fromCenterImage)
                {
                    xImage.Margin = new Thickness((xFrame.ActualWidth - currentSize.Width * currentZoom / 1000) / 2, (xFrame.ActualHeight - currentSize.Height * currentZoom / 1000) / 2, 0, 0);
                }
                else if (!fromTopLleft)
                {
                    xImage.Margin = new Thickness(xImage.Margin.Left - currentSize.Width * zoomFactor / 2000, xImage.Margin.Top - currentSize.Height * zoomFactor / 2000, 0, 0);
                }
            }
            else
            {
                xImage.Width = double.NaN;
                xImage.Height = double.NaN;
                xImage.HorizontalAlignment = HorizontalAlignment.Center;
                xImage.VerticalAlignment = VerticalAlignment.Center;
                xImage.Margin = new Thickness(0);
            }
        }

        private void xCenterPosition_Click(object sender, RoutedEventArgs e)
        {
            if (currentStretch == Stretch.None)
            {
                rePosition(true, false, 0);
            }
        }

        private void xPixelView_Click(object sender, RoutedEventArgs e)
        {
            isPixelMode = !isPixelMode;
            if (isPixelMode)
            {
                RenderOptions.SetBitmapScalingMode(xImage, BitmapScalingMode.NearestNeighbor);
            }
            else
            {
                RenderOptions.SetBitmapScalingMode(xImage, BitmapScalingMode.Unspecified);
            }
        }

        private void xInvertView_Click(object sender, RoutedEventArgs e)
        {
            isBlackMode = !isBlackMode;
            SolidColorBrush brushFrameB = new SolidColorBrush(Color.FromRgb(40, 40, 40));
            SolidColorBrush brushFrameW = new SolidColorBrush(Color.FromRgb(215, 215, 215));
            xFrame.Background = isBlackMode ? brushFrameB : brushFrameW;
            //SolidColorBrush brushOptionsB = new SolidColorBrush(Color.FromRgb(70, 70, 70));
            //SolidColorBrush brushOptionsW = new SolidColorBrush(Color.FromRgb(185, 185, 185));
            //xOptions.Background = isBlackMode ? brushOptionsB : brushOptionsW;
        }

        private void xBackPicture_Click(object sender, RoutedEventArgs e)
        {
            var ext = new List<string>() { ".bmp", ".gif", ".ico", ".jpeg", ".png", ".tiff", ".jpg" };
            var files = currentFile.Directory.GetFiles().Where(x => ext.IndexOf(x.Extension.ToLower()) > -1).ToList();
            if (files.Count > 1)
            {
                var xFile = files.FirstOrDefault(x => x.FullName == currentFile.FullName);
                if (xFile != null)
                {
                    var xIndex = (files.Count + files.IndexOf(xFile) - 1) % files.Count;
                    showImage(files[xIndex].FullName);
                }
            }
        }

        private void xNextPicture_Click(object sender, RoutedEventArgs e)
        {
            var ext = new List<string>() { ".bmp", ".gif", ".ico", ".jpeg", ".png", ".tiff", ".jpg" };
            var files = currentFile.Directory.GetFiles().Where(x => ext.IndexOf(x.Extension.ToLower()) > -1).ToList();
            if (files.Count > 1)
            {
                var xFile = files.FirstOrDefault(x => x.FullName == currentFile.FullName);
                if (xFile != null)
                {
                    var xIndex = (files.Count + files.IndexOf(xFile) + 1) % files.Count;
                    showImage(files[xIndex].FullName);
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                xBackPicture_Click(null, null);
            }
            if (e.Key == Key.Right)
            {
                xNextPicture_Click(null, null);
            }
        }

        /* Image size */

        private void xStretchSize_Click(object sender, RoutedEventArgs e)
        {
            if (currentStretch == Stretch.None)
            {
                xStretchASize.Visibility = Visibility.Visible;
                xStretchBSize.Visibility = Visibility.Collapsed;
                xZoom.IsEnabled = false;
                xCenterPosition.IsEnabled = false;
                currentStretch = Stretch.Uniform;
                rePosition(true, false, 0);
            }
            else
            {
                xStretchASize.Visibility = Visibility.Collapsed;
                xStretchBSize.Visibility = Visibility.Visible;
                xZoom.IsEnabled = true;
                xCenterPosition.IsEnabled = true;
                currentStretch = Stretch.None;
                rePosition(true, false, 0);
            }
        }

        private void xZoom_Click(object sender, RoutedEventArgs e)
        {
            if (xZoomValue.Visibility == Visibility.Visible)
            {
                xZoomValue.Visibility = Visibility.Collapsed;
            }
            else
            {
                xZoomValue.Visibility = Visibility.Visible;
                xZoomValue.Text = "Zoom : " + (currentZoom / 10) + "%";
            }
        }

        /* Windows events */

        bool windowsLocationModified = false;
        bool windowsMaximized = false;
        Point lastWindowPosition = new Point();
        Size lastWindowSize = new Size();

        private void saveWindowsLocation()
        {
            if (!windowsMaximized)
            {
                windowsLocationModified = true;
                lastWindowPosition.X = this.Left;
                lastWindowPosition.Y = this.Top;
                lastWindowSize.Width = this.ActualWidth;
                lastWindowSize.Height = this.ActualHeight;
                windowsMaximized = true;
            }
        }

        private void xHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void xMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void xRestore_Click(object sender, RoutedEventArgs e)
        {
            xRestore.Visibility = Visibility.Collapsed;
            xMaximize.Visibility = Visibility.Visible;
            xFullsize.Visibility = Visibility.Visible;
            saveWindowsLocation();
            windowsMaximized = false;
            this.WindowState = WindowState.Normal;
            if (windowsLocationModified)
            {
                this.Left = lastWindowPosition.X;
                this.Top = lastWindowPosition.Y;
                this.Width = lastWindowSize.Width;
                this.Height = lastWindowSize.Height;
            }
            xWindow.Padding = new Thickness(0);
            xWindow.CornerRadius = new CornerRadius(5, 5, 0, 0);
        }

        private void xMaximize_Click(object sender, RoutedEventArgs e)
        {
            xRestore.Visibility = Visibility.Visible;
            xMaximize.Visibility = Visibility.Collapsed;
            xFullsize.Visibility = Visibility.Visible;
            saveWindowsLocation();
            this.WindowState = WindowState.Normal;
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.FullPrimaryScreenWidth;
            this.Height = SystemParameters.FullPrimaryScreenHeight + SystemParameters.CaptionHeight;
            xWindow.Padding = new Thickness(0);
            xWindow.CornerRadius = new CornerRadius(0);
        }

        private void xFullsize_Click(object sender, RoutedEventArgs e)
        {
            xRestore.Visibility = Visibility.Visible;
            xMaximize.Visibility = Visibility.Visible;
            xFullsize.Visibility = Visibility.Collapsed;
            saveWindowsLocation();
            this.WindowState = WindowState.Maximized;
            xWindow.Padding = new Thickness(6, 6, 6, 0);
            xWindow.CornerRadius = new CornerRadius(0);
        }

        private void xClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
