using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace 俯卧撑_RT
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool istimerun;
        TimeSpan clicktime;
        int quicknum = 0;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            clicktime = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void run()
        {
            TimeTextBlock.Text = "01:00";
            MainClickTextBlock.Text = "0";
            TimeSpan time = TimeSpan.Parse("00:01:00");
#if DEBUG
            time = TimeSpan.Parse("00:00:10");
#endif

            istimerun = true;
            while (istimerun)
            {
                await Task.Delay(1000);
                time -= TimeSpan.Parse("00:00:01");
                TimeTextBlock.Text = time.Minutes + ":" + time.Seconds;
                if (time == TimeSpan.Parse("00:00:00"))
                {
                    istimerun = false;
                    save();
                }
            }
        }

        private async void save()
        {
            SavingStackPanel.Visibility = Visibility.Visible;

            stopAppBarButton.IsEnabled = false;
            CalendarAppBarButton.IsEnabled = false;

            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            //读
            string text = "";
            IStorageFile storageFileRE = await applicationFolder.GetFileAsync("num");
            IRandomAccessStream accessStream = await storageFileRE.OpenReadAsync();
            using (StreamReader streamReader = new StreamReader(accessStream.AsStreamForRead((int)accessStream.Size)))
            {
                text = streamReader.ReadToEnd();
            }
            Debug.WriteLine("文本1：" + text);
            //写
            IStorageFile storageFileWR = await applicationFolder.CreateFileAsync("num", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(storageFileWR, MainClickTextBlock.Text + "$" + text);

            await Task.Delay(2000);
            SavingStackPanel.Visibility = Visibility.Collapsed;
            startAppBarButton.IsEnabled = true;
            CalendarAppBarButton.IsEnabled = true;
        }

        private async void MainClickGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (DateTime.Now.TimeOfDay - clicktime >= TimeSpan.Parse("00:00:00.5") && istimerun)
            {
                MainClickTextBlock.Text = (Int32.Parse(MainClickTextBlock.Text) + 1).ToString();
                clicktime = DateTime.Now.TimeOfDay;
                quicknum = 0;
            }
            else if (istimerun)
            {
                quicknum++;
            }
            if (quicknum >= 2)
            {
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                XmlNodeList elements = toastXml.GetElementsByTagName("text");
                elements[0].AppendChild(toastXml.CreateTextNode("不要作弊哦~"));
                ToastNotification toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);

                //从通知中心删除
                await Task.Delay(2000);
                ToastNotificationManager.History.Clear();
                quicknum = 0;
            }
        }

        private void startAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            run();
            startAppBarButton.IsEnabled = false;
            stopAppBarButton.IsEnabled = true;
            CalendarAppBarButton.IsEnabled = false;
        }

        private void stopAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            startAppBarButton.IsEnabled = true;
            stopAppBarButton.IsEnabled = false;
            CalendarAppBarButton.IsEnabled = true;
            istimerun = false;
        }

        private void CalendarAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            startAppBarButton.IsEnabled = true;
            stopAppBarButton.IsEnabled = false;
            istimerun = false;

            Frame.Navigate(typeof(CalendarPage));
        }
    }
}
