using Screen_shot.WPF.Controllers;
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
using System.Threading;
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
        private    void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                Thread thread = new Thread(RunContent);
                thread.Start();
            }
            catch (Exception ex)
            {
                var r = MessageBox.Show(ex.Message, "Остановить  стрим ? ", MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes)
                    timer.Stop();
            }
            
        }

        private void RunContent()
        {

            string key = string.Empty;
            Dispatcher.Invoke(() => key = tbKey.Text);

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Укажите ключ"); return;
            }
               

            try
            {
                Bitmap bitmap = SCREANMONOTOR();
                ImageController.SendPicture(bitmap, GetUri() , key);
                Dispatcher.Invoke( () =>  myImage.Source = ImageController.SetImage(GetUri(),
                   key) 
                    );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private   Bitmap SCREANMONOTOR()
        {
            int  w =  (int )System.Windows.SystemParameters.FullPrimaryScreenWidth;
            int  h = (int) System.Windows.SystemParameters.FullPrimaryScreenHeight;

            var bull = false;
            Dispatcher.Invoke(() => bull = rbMain.IsChecked.Value);

            if (bull == true)
                return ImageController.GetScreenShot(w, h, new System.Drawing.Point(0, 0));

            
            Dispatcher.Invoke(() => bull = rbNotMain.IsChecked.Value);
            if (bull == true)
            {
                int wFull = (int)System.Windows.SystemParameters.VirtualScreenWidth;
                var w2 = wFull - w;

                int hFull = (int)System.Windows.SystemParameters.VirtualScreenHeight;
                int h2 = 0;

                if (hFull == h) // одинаковые моники 
                    h2 = h;

                if (hFull > h)
                    h2 = hFull;

                return ImageController.GetScreenShot(w2, h2, new System.Drawing.Point((int)w, (int)0));

            }
            return null;
        }

        private string GetUri()
        {
            string ur = string.Empty;
            Dispatcher.Invoke(() => ur = $"{cbProtacol.SelectedItem.ToString()}://{tbURi.Text}");
            return ur; 
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
                var r = MessageBox.Show(ex.Message, "Остановить  стрим ? ", MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes)
                    timer.Stop();
            }
        }

        private void GetMaxSixeImage()
        {
            brImage.MaxHeight = this.ActualHeight - 50;
            brImage.MaxWidth = this.ActualHeight - 50;
        }

      

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            steckStep.IsEnabled = true; 
            btStaru.IsEnabled = true;
        }
    }

}
