using System;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.Util;
using DirectShowLib;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.IdentityModel.Tokens;
using System.Timers;
using Emgu.CV.UI;

/*
 * ВАЖНО!!! библиотека tensorflow, необходимая для работы модели, удалена из проекта для загрузки на GitHub. 
 * Установить её компоненты / библиотеку полностью придётся вручную через NuGet Manager.
 * и CsharpTensorflow что ли, ч-т такое
 */

namespace ComputerVisionMeter
{
    public partial class MainWindow : Window
    {
        private VideoCapture capture = null;
        private DsDevice[] availableDevices = null;
        private BitmapImage bmp = null;

        private int selectedCameraID = 0;
        private string filePath = null;
        System.Timers.Timer dispatcherTimer = new System.Timers.Timer(2500);

        //private DependencyPropertyDescriptor changedLabelEvent = DependencyPropertyDescriptor.FromProperty(System.Windows.Controls.Image.SourceProperty, typeof(System.Windows.Controls.Image));

        //private readonly PredictionEngine<MeterClassifier.ModelInput, MeterClassifier.ModelOutput> _engine;
        //var context = new MLContext();
        //_engine = context
        //    .Model
        //    .CreatePredictionEngine<MeterClassifier.ModelInput, MeterClassifier.ModelOutput>(
        //    context.Model.Load(Path.GetFullPath("MeterClassifier.mlnet"), out _));

        public MainWindow() => InitializeComponent();

        private void SourceChangedHandler(object sender, EventArgs e)
        {
            RecognizeLabel.Content = null;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            availableDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            foreach (var device in availableDevices) { CamerasList.Items.Add(device.Name); }
        }

        private void StartImageEvent()
        {
            if (ImageView.Source != null)
            {
                dispatcherTimer.Elapsed += OnTimedEvent;
                dispatcherTimer.AutoReset = true;
                dispatcherTimer.Enabled = true;
            }
            //else OwnMessageBox.Show("Headline", "Is that image null???", MessageBoxButton.OK);
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat frame = new Mat();
                capture.Retrieve(frame);

                var image = frame.ToImage<Bgr, byte>().Flip(Emgu.CV.CvEnum.FlipType.Horizontal).Bitmap;
                //Dispatcher.Invoke(() => ImageView.Source = ImageSourceFromBitmap(image));
                Dispatcher.Invoke(() =>
                    this.ImageView.SetCurrentValue(System.Windows.Controls.Image.SourceProperty, ImageSourceFromBitmap(image))
                );
            }
            catch (Exception ex) { OwnMessageBox.Show("Headline", ex.Message, MessageBoxButton.OK); }
        }

        //необходимо для корректного парсинга из Bitmap в ImageSource 
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        //необходимо для корректного парсинга из ImageSource в byte[]
        public static byte[] ImageSourceToByteArray(ImageSource imageSource)
        {


            if (imageSource is BitmapSource bitmapSource)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    encoder.Save(memoryStream);

                    return memoryStream.ToArray();
                }
            }
            throw new ArgumentException("Ошибка парсинга ImageSource Control в byte[]");
        }

        private void CamerasList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCameraID = CamerasList.SelectedIndex;
            UseCamera.IsEnabled = true;
        }

        private async void PickImageFromPath_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null)
            {
                dispatcherTimer.Enabled = false;

                await Task.Delay(200);
                capture.Stop();
                capture.Dispose();
                capture = null;

                ImageView.Source = null;

                selectedCameraID = 0;

                UseCamera.IsEnabled = true;
                PauseCamera.IsEnabled = false;
                StopCamera.IsEnabled = false;

                RecognizeLabel.Content = "";
                UseCamera.Content = "Начать";

                await Task.Yield();
            }

            var dialogWindow = new OpenFileDialog
            {
                Title = "Выбор изображения",
                Multiselect = false,
                Filter = "Изображения|*.png;*.png*;*.bmp;*.jpg|Все файлы (*.*)|*.*",
                CheckFileExists = true,
            };

            if (dialogWindow.ShowDialog(this) != true) return;

            filePath = dialogWindow.FileName;
            //ImageView.Source = new BitmapImage(new Uri(filePath));
            var image = new BitmapImage(new Uri(filePath));

            this.ImageView.SetCurrentValue(System.Windows.Controls.Image.SourceProperty, image);

            dispatcherTimer.Enabled = true;
        }

        //private async void RecognizeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (ImageView.Source == null) throw new Exception("Выберите изображение прежде, чем его классифицировать!");


        //        //byte[] byteImage = System.Text.Encoding.UTF8.GetBytes(filePath);

        //        //var context = new MLContext();

        //        //var model = context.Model.Load(Path.GetFullPath("MeterClassifier.mlnet"), out _);
        //        //var engine = context.Model.CreatePredictionEngine<MeterClassifier.ModelInput, MeterClassifier.ModelOutput>(model);

        //        //var result = MeterClassifier.Predict(new MeterClassifier.ModelInput
        //        //{
        //        //    ImageSource = byteImage
        //        //});

        //        //var imageBytes = File.ReadAllBytes(filePath);

        //        //capture.Pause();


        //        //RecognizeImage();

        //        //Подчистка надписи при изменении изображения, хз нужно ли
        //        //changedLabelEvent.AddValueChanged(this.ImageView, SourceChangedHandler);

        //        //capture.Start();

        //        await Task.Delay(200);
        //        StartImageEvent();
        //    }
        //    catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        //}

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            string label = string.Empty;
            byte[] imageBytes = null;

            var thread = Task.Run(() =>
            {
                Dispatcher.Invoke(() => imageBytes = ImageSourceToByteArray(ImageView.Source));

                MeterClassifier.ModelInput sampleData = new MeterClassifier.ModelInput()
                {
                    ImageSource = imageBytes,
                };

                var sortedScoresWithLabel = MeterClassifier.Predict(sampleData);

                label = sortedScoresWithLabel.PredictedLabel == "Marked" ? "Готов" : sortedScoresWithLabel.PredictedLabel == "Unmarked" ? "Не готов" : null;

                Dispatcher.Invoke(() => RecognizeLabel.Content = $"{label} с вероятностью {sortedScoresWithLabel.Score.Max():p0}");
            });

            await Task.Yield();
        }

        private async void UseCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CamerasList.SelectedItem == null) throw new Exception("Выберите камеру!");
                else if (availableDevices.Length == 0) throw new Exception("Нет доступных камер!");
                else if (capture != null) { capture.Start(); UseCamera.IsEnabled = false; }
                else
                {
                    capture = new VideoCapture(selectedCameraID);
                    capture.ImageGrabbed += Capture_ImageGrabbed;
                    capture.Start();

                    UseCamera.IsEnabled = false;
                    PauseCamera.IsEnabled = true;
                    StopCamera.IsEnabled = true;
                    dispatcherTimer.Enabled = true;

                    await Task.Delay(700);
                    StartImageEvent();
                    await Task.Yield();
                }
            }
            catch (Exception ex) { OwnMessageBox.Show("Headline", ex.Message, MessageBoxButton.OK); }
        }

        private async void PauseCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (capture != null)
                {
                    await Task.Delay(200);
                    capture.Pause();

                    UseCamera.IsEnabled = true;
                    UseCamera.Content = "Продолжить";

                    await Task.Yield();
                }
            }
            catch (Exception error) { OwnMessageBox.Show("Headline", error.Message, MessageBoxButton.OK); }
        }

        private async void StopCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (capture != null)
                {
                    dispatcherTimer.Enabled = false;

                    capture.Stop();
                    capture.Dispose();
                    capture = null;

                    selectedCameraID = 0;

                    UseCamera.IsEnabled = true;
                    PauseCamera.IsEnabled = false;
                    StopCamera.IsEnabled = false;

                    RecognizeLabel.Content = "";
                    UseCamera.Content = "Начать";

                    await Task.Delay(200);
                    ImageView.Source = null;
                    await Task.Yield();
                }
            }
            catch (Exception error) { OwnMessageBox.Show("Headline", error.Message, MessageBoxButton.OK); }
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (capture !=  null)
            {
                capture.Stop();
                capture.Dispose();
                capture = null;
            }

            ImageView.Source = null;
            this.Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}
