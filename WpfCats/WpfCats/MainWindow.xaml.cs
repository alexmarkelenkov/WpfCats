using AngleSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

namespace WpfCats
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dictionary<string, OpenCvSharp.Rect[]> catDictionary = new Dictionary<string, OpenCvSharp.Rect[]>();
        private static Dictionary<string, int> rusPlateNumbers = new Dictionary<string, int>();
        public List<string> Cats {
            get
            {
                return cats;
            }

            private set { }
        }        
        public List<string> Plates {
            get
            {
                return plates;
            }
            private set
            {
            }
        }
        private static IEnumerable<string> uris;
        private static int N = 10;
        private static double percent = 0.0;
        public List<string> cats = new List<string>();
        public List<string> plates = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        static async Task ProcessImagesAsync()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = CatServer.Cutespaw;
            var document = await BrowsingContext.New(config).OpenAsync(address);
            var imgSelector = CatServer.Selectors[address];
            uris = document.QuerySelectorAll(imgSelector).Select(item => item.GetAttribute("src")).Take(N).ToList();
        }

        static async Task GetImageAsync(string uri, string filename, IProgress<double> progress, CancellationToken token)
        {

            if (token.IsCancellationRequested == false)
            {
                WebClient webClient = new WebClient();
                try
                {
                    token.ThrowIfCancellationRequested();
                    await webClient.DownloadFileTaskAsync(uri, filename);                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                if (token.IsCancellationRequested == false)
                {
                    progress.Report(100 * percent / uris.Count());
                    percent++;  
                }
            }
        }

        private void DetectCat(FileInfo f)
        {
            OpenCvSharp.CascadeClassifier cc = new OpenCvSharp.CascadeClassifier("haarcascade_frontalcatface.xml");
            var img = new OpenCvSharp.Mat(f.FullName);
            var img2 = new OpenCvSharp.Mat();
            img.ConvertTo(img2, OpenCvSharp.MatType.CV_8U);
            var catsCV = cc.DetectMultiScale(img2);
            //Console.WriteLine(cats.Length);
            if(catsCV.Length > 0)
            {
                catDictionary.Add(f.FullName, catsCV);
            }
            cats.Add(f.Name + " : " + catsCV.Length.ToString());              
            
        }

        private void DetectRussianPlateNumber(FileInfo f)
        {
            try
            {
                OpenCvSharp.CascadeClassifier cc = new OpenCvSharp.CascadeClassifier("haarcascade_russian_plate_number.xml");
                var img = new OpenCvSharp.Mat(f.FullName);
                var img2 = new OpenCvSharp.Mat();
                img.ConvertTo(img2, OpenCvSharp.MatType.CV_8U);
                var plateNumbers = cc.DetectMultiScale(img2);
                //Console.WriteLine(plateNumbers.Length);
                if (plateNumbers.Length > 0)
                {
                    rusPlateNumbers.Add(f.FullName, plateNumbers.Length);
                }
                Plates.Add(f.Name + " : " + plateNumbers.Length.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;            

            await ProcessImagesAsync();

            try
            {
                int i = 0;
                List<Task> imageSaveTasks = new List<Task>();
                Task t;

                CancellationTokenSource cts = new CancellationTokenSource();
                button1.Click += delegate { 
                    if (button.IsEnabled == false)
                    {
                        button1.IsEnabled = false;
                        cts.Cancel();
                    }
                };

                var sw = new Stopwatch();
                sw.Start();
                foreach (var uri in uris)
                {
                    Console.WriteLine(uri);

                    //WebClient webClient = new WebClient();
                    //webClient.DownloadFile(uri, Directory.GetCurrentDirectory().ToString() + "\\img\\cat" + i.ToString() + ".jpg");                    

                    if (cts.Token.IsCancellationRequested == false)
                    {
                        t = GetImageAsync(uri,
                                       Directory.GetCurrentDirectory().ToString() + "\\img\\cat" + i.ToString() + ".jpg",
                                       new Progress<double>(percent => progressBar.Value = percent), cts.Token);
                        imageSaveTasks.Add(t);
                        i++; 
                    }
                    else
                    {
                        break;
                    }
                }
                await Task.WhenAll(imageSaveTasks);
                sw.Stop();
                Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\img");
            Parallel.ForEach(dir.GetFiles(), DetectCat);
            
            
            Console.WriteLine("--------------------------------------------------------------------");
            DirectoryInfo dir2 = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\reg");
            Parallel.ForEach(dir2.GetFiles(), DetectRussianPlateNumber);

            this.DataContext = null;
            this.DataContext = this;

            //List<Rectangle> rectangles = new List<Rectangle>();
            //foreach (KeyValuePair<string, OpenCvSharp.Rect[]> t in catDictionary)
            //{
            //    Canvas cn = new Canvas();
            //    Canvas canvas = new Canvas();
            //    Image image = new Image();
            //    BitmapImage bm = new BitmapImage();
            //    bm.BeginInit();
            //    bm.UriSource = new Uri(t.Key, UriKind.RelativeOrAbsolute);
            //    //ImageBrush imgBrush = new ImageBrush();
            //    //imgBrush.ImageSource = bm;
            //    bm.EndInit();
            //    image.Source = bm;
            //    canvas.Children.Add(image);
            //    //canvas.Background = imgBrush;

            //    foreach (var c in t.Value)
            //    {
            //        var r = new Rectangle
            //        {
            //            Stroke = Brushes.LightBlue,
            //            StrokeThickness = 2.0,
            //            Width = c.Width,
            //            Height = c.Height
            //        };
            //        rectangles.Add(r);

            //        canvas.Children.Add(r);
            //        Canvas.SetLeft(r, c.Left);
            //        Canvas.SetTop(r, c.Top);                                       
            //    }

            //    Canvas.SetTop(canvas, image.Height);
            //    cn.Children.Add(canvas);
            //    grid.Children.Add(cn);
            //}            


        }
        
    }
}
