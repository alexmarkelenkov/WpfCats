using AngleSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

namespace WpfCats
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dictionary<string, OpenCvSharp.Rect[]> catDictionary = new Dictionary<string, OpenCvSharp.Rect[]>();
        private static IEnumerable<string> uris;
        private static int N = 50;

        public MainWindow()
        {
            InitializeComponent();            
        }


        static async Task ProcessImagesAsync()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = CatServer.Cutespaw;
            var document = await BrowsingContext.New(config).OpenAsync(address);
            var imgSelector = CatServer.Selectors[address];
            uris = document.QuerySelectorAll(imgSelector).Select(item => item.GetAttribute("src")).Take(N).ToList();

        }

        static async Task GetImageAsync(string uri, string filename)
        {
            WebClient webClient = new WebClient();
            try
            {
                await webClient.DownloadFileTaskAsync(uri, filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void DetectCat(FileInfo f)
        {
            OpenCvSharp.CascadeClassifier cc = new OpenCvSharp.CascadeClassifier("haarcascade_frontalcatface.xml");
            var img = new OpenCvSharp.Mat(f.FullName);
            var img2 = new OpenCvSharp.Mat();
            img.ConvertTo(img2, OpenCvSharp.MatType.CV_8U);
            var cats = cc.DetectMultiScale(img2);
            Console.WriteLine(cats.Length);
            if(cats.Length > 0)
            {
                catDictionary.Add(f.FullName, cats);
            }
           
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await ProcessImagesAsync();

            try
            {
                int i = 0;
                List<Task> imageSaveTasks = new List<Task>();
                Task t;

                var sw = new Stopwatch();
                sw.Start();
                foreach (var uri in uris)
                {
                    Console.WriteLine(uri);

                    //WebClient webClient = new WebClient();
                    //webClient.DownloadFile(uri, Directory.GetCurrentDirectory().ToString() + "\\img\\cat" + i.ToString() + ".jpg");                    

                    t = GetImageAsync(uri, Directory.GetCurrentDirectory().ToString() + "\\img\\cat" + i.ToString() + ".jpg");
                    imageSaveTasks.Add(t);

                    i++;
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


            
            foreach (KeyValuePair<string, OpenCvSharp.Rect[]> t in catDictionary)
            {
                
                Image image = new Image();
                BitmapImage bm = new BitmapImage();
                bm.BeginInit();
                bm.UriSource = new Uri(t.Key, UriKind.RelativeOrAbsolute);
                //ImageBrush imgBrush = new ImageBrush();
                //imgBrush.ImageSource = bm;
                bm.EndInit();
                image.Source = bm;
                //canvas.Children.Add(image);
                stackPanel.Children.Add(image);

                //Canvas canvas = new Canvas();
                foreach (var c in t.Value)
                {
                    var r = new Rectangle
                    {
                        Stroke = Brushes.LightBlue,
                        StrokeThickness = 2.0,
                        Width = c.Width,
                        Height = c.Height
                    };

                    stackPanel.Children.Add(r);
                    
                    //canvas.Children.Add(r);
                    //Canvas.SetLeft(r, c.Left);
                    //Canvas.SetTop(r, c.Top);
                }                

                //stackPanel.Children.Add(canvas);
            }

            

        }
    }
}
