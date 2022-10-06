using Screen_shot.WPF.Controllers;
using Screen_shot.WPF.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Screen_shot.WPF
{
    /// <summary>
    /// Логика взаимодействия для WatchStramWindows.xaml
    /// </summary>
    public partial class WatchStramWindows : Window
    {

        string[] protacols = new string[] {
            "HTTP" , "HTTPS"
        };


        private string _url = "192.168.10.160:81"; //По Умолчанию
        public int Step = 100;

        DispatcherTimer timer = new DispatcherTimer();

        public WatchStramWindows()
        {
            InitializeComponent();
            this.Loaded += WatchStramWindows_Loaded;
            this.SizeChanged += WatchStramWindows_SizeChanged;
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void WatchStramWindows_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          
            imageScrean.Width = this.ActualWidth- 20;
            imageScrean.Height = this.ActualHeight - 80;
        }

        private void Timer_Tick(object? sender, EventArgs e)
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

        private void RunContent(object? obj)
        {

            var key = string.Empty;
            Dispatcher.Invoke(() => key = tbkey.Text);

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Укажите ключ трансляции"); return;
            }
                


            Dispatcher.Invoke(() => imageScrean.Source = ImageController.SetImage(GetUri() , key));
        }

        private string GetUri()
        {
            string ur = string.Empty;
            Dispatcher.Invoke(() => ur = $"{cbProtacol.SelectedItem.ToString()}://{tbURi.Text}");
            return ur;
        }

        private void WatchStramWindows_Loaded(object sender, RoutedEventArgs e)
        {
            cbProtacol.ItemsSource = protacols;
            cbProtacol.SelectedIndex = 0;
            tbURi.Text = _url;

            tbStep.Text = "100";
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, Step);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Step = Convert.ToInt32(tbStep.Text);
                timer.Interval = new TimeSpan(0, 0, 0, 0, Step);
                timer.Start();
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
                return;
            }
           
        }
       
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }
    }
}
