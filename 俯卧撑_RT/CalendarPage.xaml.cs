using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UmengSocialSDK;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using 玩机统计_WP_RT;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace 俯卧撑_RT
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CalendarPage : Page
    {
        int[] time = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
        private string Umeng_App_Key = "5549d52067e58ed292003360";
        public CalendarPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;//注册重写后退按钮事件
            initUI();
            drawStatistics();
        }

        //离开页面时，取消事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;//注册重写后退按钮事件
        }


        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)//重写后退按钮，如果要对所有页面使用，可以放在App.Xaml.cs的APP初始化函数中重写。
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
            App.isVisibility = false;
            ProgressGrid.Visibility = Visibility.Collapsed;
        }


        private void initUI()
        {
            //if (App.isVisibility)
            //{
            //    ProgressGrid.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    ProgressGrid.Visibility = Visibility.Collapsed;
            //}
            highGrid.Height = (Window.Current.Bounds.Width - 50) / 2;
            lowdateGrid.Height = (Window.Current.Bounds.Width - 50) / 2;
            statisticsGrid.Height = highGrid.Height;
            //statisticsGrid.Width = Window.Current.Bounds.Width - 90;
            sum_aveGrid.Height = highGrid.Height;
        }

        private async void drawStatistics()
        {
            statisticsViewCanvas.Height = ((Window.Current.Bounds.Width - 50) / 2 - 40) / 6 * 5;
            statisticsViewCanvas.Width = Window.Current.Bounds.Width - 90;

            double CanvasWidth = statisticsViewCanvas.Width;
            double CanvasHeight = statisticsViewCanvas.Height;

            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            //读
            string text = "";
            IStorageFile storageFileRE = await applicationFolder.GetFileAsync("num");
            IRandomAccessStream accessStream = await storageFileRE.OpenReadAsync();
            using (StreamReader streamReader = new StreamReader(accessStream.AsStreamForRead((int)accessStream.Size)))
            {
                text = streamReader.ReadToEnd();
            }
            Debug.WriteLine(text);

            int temp = 0;
            try
            {
                for (int i = 0; i < 7; i++)
                {
                    time[i] = Int32.Parse(text.Substring(temp, text.IndexOf("$", temp) - temp));
                    temp = text.IndexOf("$", temp) + 1;
                    Debug.WriteLine(time[i]);
                }
            }
            catch (Exception) { }

            temp = 0;
            double sum = 0;
            int num = 1;
            try
            {
                while (true)
                {
                    sum += Int32.Parse(text.Substring(temp, text.IndexOf("$", temp) - temp));
                    temp = text.IndexOf("$", temp) + 1;
                    num++;
                    Debug.WriteLine(sum + "   " + num);

                }
            }
            catch (Exception) { }

            sumTextBlock.Text = sum.ToString();
            aveTextBlock.Text = Math.Round((sum / num), 1).ToString();

            int max = IntArray_Max_Min.FindMax(time);
            int min = IntArray_Max_Min.FindMin(time);

            highTextBlock.Text = max.ToString();
            lowTextBlock.Text = min.ToString();


            if (max == -1) max = 0;
            if (min == -1) min = 0;

            MaxLTextBlock.Text = max.ToString();
            MinLTextBlock.Text = "0";

            //canvasLine.Y1 = 0;
            //canvasLine.Y2 = CanvasHeight + 5;

            Polyline ChartPolyLine = new Polyline();
            ChartPolyLine.Stroke = this.Foreground;
            ChartPolyLine.StrokeThickness = 1;

            PointCollection ChartPolyLinePointCollection = new PointCollection();
            Point p = new Point();
            for (int i = 6; i >= 0; i--)
            {
                if (time[i] == -1) time[i] = 0;
                p.X = CanvasWidth / 6 * (6 - i);
                p.Y = CanvasHeight - CanvasHeight * ((time[i] - 0) / (double)(max - 0));
                ChartPolyLinePointCollection.Add(p);
            }
            ChartPolyLine.Points = ChartPolyLinePointCollection;
            statisticsViewCanvas.Children.Clear();
            statisticsViewCanvas.Children.Add(ChartPolyLine);

        }

        private async void shareAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //ProgressGrid.Visibility = Visibility.Visible;
            App.isVisibility = true;

            RenderTargetBitmap mBitmap = new RenderTargetBitmap();
            await mBitmap.RenderAsync(CalendarStackPanel);
            IBuffer buffer = await mBitmap.GetPixelsAsync();

            IStorageFolder mapplicationFolder = ApplicationData.Current.TemporaryFolder;
            IStorageFile saveFile = await mapplicationFolder.CreateFileAsync("Share.png", CreationCollisionOption.OpenIfExists);

            using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)mBitmap.PixelWidth,
                    (uint)mBitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    buffer.ToArray());
                await encoder.FlushAsync();
            }

            byte[] imageData = null;
            using (var stream = await saveFile.OpenStreamForReadAsync())
            {
                imageData = new byte[stream.Length];
                await stream.ReadAsync(imageData, 0, imageData.Length);
            }

            UmengLink link = new UmengLink();

            //显示平台列表，由用户选择平台进行分享
            List<UmengClient> clients = new List<UmengClient>()
            {
                new SinaWeiboClient(Umeng_App_Key),
                new RenrenClient(Umeng_App_Key),
                //new QzoneClient(Umeng_App_Key),
                new TencentWeiboClient(Umeng_App_Key),
                new DoubanClient(Umeng_App_Key),
            };
            UmengClient umengClient = new MultiClient(clients);

            var res = await umengClient.SharePictureAsync(new UmengPicture(imageData, "快来运动吧") { Title = "图片分享" });
            Debug.WriteLine(res.Status.ToString());
            if (res.Status.ToString() == "Success")
            {
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                XmlNodeList elements = toastXml.GetElementsByTagName("text");
                elements[0].AppendChild(toastXml.CreateTextNode("分享成功"));
                ToastNotification toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);

                //从通知中心删除
                await Task.Delay(1000);
                ToastNotificationManager.History.Clear();
            }
            else
            {
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                XmlNodeList elements = toastXml.GetElementsByTagName("text");
                elements[0].AppendChild(toastXml.CreateTextNode("分享失败"));
                ToastNotification toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);

                //从通知中心删除
                await Task.Delay(1000);
                ToastNotificationManager.History.Clear();
            }



        }
    }
}
