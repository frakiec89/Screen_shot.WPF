using Screen_shot.WPF.Controllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
        string Url = string.Empty;
        string Key = string.Empty;
        bool IsChekedMain;
        bool IsChekedNotMaim;

        DpiScale dpi;
       
        
        Thread thread;

        DispatcherTimer timer = new DispatcherTimer();  

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;
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
            btnStop.IsEnabled = false;
            dpi = System.Windows.Media.VisualTreeHelper.GetDpi(new System.Windows.Controls.Control());
            

        }

        public void StartTimer ()
        {
            timer.Tick += Timer_Tick;
            timer.Start();
        }


        /// <summary>
        /// обновление 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private   void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                thread = new Thread(RunContent);
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
            if (string.IsNullOrEmpty(Key))
            {
                MessageBox.Show("Укажите ключ"); return;
            }
            try
            {
                Bitmap bitmap = SCREANMONOTOR();
                ImageController.SendPicture(bitmap, Url , Key);
                 Thread.Sleep(1000);
                Dispatcher.Invoke( () =>  myImage.Source = ImageController.SetImage(Url,
                   Key)  );
                
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

           

            if (IsChekedMain == true)
                return ImageController.GetScreenShot(w, h, new System.Drawing.Point(0, 0) , dpi);
           
            if (IsChekedNotMaim == true)
            {
                int wFull = (int)System.Windows.SystemParameters.VirtualScreenWidth;
                var w2 = wFull - w;

                int hFull = (int)System.Windows.SystemParameters.VirtualScreenHeight;
                int h2 = 0;

                if (hFull == h) // одинаковые моники 
                    h2 = h;

                if (hFull > h)
                    h2 = hFull;

                return ImageController.GetScreenShot(w2, h2, new System.Drawing.Point((int)w, (int)0)  ,  dpi );

            }
            return null;
        }

        private string GetUri()
        {
            string ur = string.Empty;
            Dispatcher.Invoke(() => ur = $"{cbProtacol.SelectedItem.ToString()}://{tbURi.Text}");
            return ur; 
        }

        private  void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Key = tbKey.Text;
                Url = GetUri();
                IsChekedMain = rbMain.IsChecked.Value;
                IsChekedNotMaim = rbNotMain.IsChecked.Value;

                if(string.IsNullOrEmpty( Key))
                {
                    MessageBox.Show("Укажите ключ");
                    return;
                }

                if(  Controllers.ImageController.IsChekedKey(Url, Key) == true )
                {
                    if(MessageBox.Show
                        ("Такой ключ  уже есть на сервере - продолжить  стрим ? ",
                          "Предупреждение"  , MessageBoxButton.YesNo
                        )== MessageBoxResult.No)
                    {
                        return; 
                    }
                }

                Step = Convert.ToInt32(tbStep.Text);
                if (Step < 100)
                    Step = 100;

                timer.Interval = new TimeSpan(0, 0, 0, 0, Step);

                StartTimer();
                GetMaxSixeImage();
                steckStep.IsEnabled = false;
                btStaru.IsEnabled = false;

                btnStop.IsEnabled = true;
                
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
            try
            {
                timer.Stop();
                var t = this.Title;
                this.Title = "Ждите";
                for (int i = 5; i > 0; i--)
                {
                    Title = $"Стрим остановиться через {i}";
                    Thread.Sleep(300);
                    Controllers.ImageController.StopStram(Url, Key);
                }
                Title = t;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            steckStep.IsEnabled = true; 
            btStaru.IsEnabled = true;
        }
    }
}