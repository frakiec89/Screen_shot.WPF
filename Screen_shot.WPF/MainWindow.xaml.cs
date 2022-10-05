using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Screen_shot.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string[] protacols = new string[] { 
            "HTTP" , "HTTPS"
        }; 


        private string _url = "192.168.10.160:81"; //По Умолчанию
        public int Step = 100;

        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            timer.Tick += Timer_Tick;
            this.SizeChanged += MainWindow_SizeChanged;
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GetMaxSixeImage();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            cbProtacol.ItemsSource = protacols;
            cbProtacol.SelectedIndex = 0;
            tbURi.Text = _url;

            rbMain.IsChecked = true;

            tbStep.Text = "100"; 
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, Step);
        }


        /// <summary>
        /// обновление 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Bitmap bitmap = SCREANMONOTOR();
            SendPicture(bitmap, GetUri());
            SetImage(GetUri());
        }

        private  Bitmap SCREANMONOTOR()
        {

            int  w =  (int )System.Windows.SystemParameters.FullPrimaryScreenWidth;
            int  h = (int) System.Windows.SystemParameters.FullPrimaryScreenHeight;
            if (rbMain.IsChecked == true)
                return GetScreenShot(w, h, new System.Drawing.Point(0, 0));


            if(rbNotMain.IsChecked == true)
            {
                int wFull = (int)System.Windows.SystemParameters.VirtualScreenWidth;
                var w2 = wFull - w;

                int hFull = (int)System.Windows.SystemParameters.VirtualScreenHeight;
                int h2 = 0;

                if (hFull == h) // одинаковые моники 
                    h2 = h;

                if (hFull > h)
                    h2 = hFull;

                return GetScreenShot(w2, h2, new System.Drawing.Point((int)w, (int)0));
            }
            return null;
        }

        private Bitmap GetScreenShot(int widht, int height, System.Drawing.Point pLT)
        {
            Bitmap printscreen = new Bitmap(widht, height);
            using   Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(pLT, new System.Drawing.Point(0, 0), printscreen.Size);
            return printscreen;
        }



        private string GetUri()
        {
            return $"{cbProtacol.SelectedItem.ToString()}://{tbURi.Text}";
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Step = Convert.ToInt32(tbStep.Text);
                timer.Interval = new TimeSpan(0, 0, 0, 0, Step);
                timer.Start();
                GetMaxSixeImage();
                steckStep.IsEnabled = false;
                btStaru.IsEnabled = false;
            }
            catch (Exception ex )
            {
                MessageBox.Show(ex.Message);
                timer.Stop();
            }
        }

        private void GetMaxSixeImage()
        {
            brImage.MaxHeight = this.ActualHeight - 50;
            brImage.MaxWidth = this.ActualHeight - 50;
        }

        private static void SendPicture(Bitmap bitmap , string uru)
        {
            try
            {
                byte[] bData = ImageToByte(bitmap);
                WebRequest request = WebRequest.Create(uru + @"/api/ScreenRetrieval/PictureReception");
                request.Method = "POST"; 
                ImagePost imagePost = new ImagePost()
                { bytes = bData };
                string jsonString = JsonSerializer.Serialize(imagePost);
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;
             
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                WebResponse response = request.GetResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            steckStep.IsEnabled = true; 
            btStaru.IsEnabled = true;
        }

        private void SetImage(string uri)
        {
            try
            {
                WebRequest request = WebRequest.Create(uri + @"/api/ScreenRetrieval/GetPicture");
                request.Method = "GET";
                request.ContentType = "application/json";
                WebResponse response = request.GetResponse();

                using StreamReader stream = new StreamReader(response.GetResponseStream());
                var content = stream.ReadToEnd();
                ImagePost bitmapImage = JsonSerializer.Deserialize<ImagePost>(content);
                myImage.Source = ToImage(bitmapImage.bytes);
                response.Close();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Остановить стрим ? "
                    , MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    timer.Stop();

            }
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[] ));
        }


        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
       




    }


    /// <summary>
    /// стуктура отправки  получения запроса  для  апи
    /// </summary>
    public class ImagePost
    {
        public byte[] bytes { get; set; }
    }

}
