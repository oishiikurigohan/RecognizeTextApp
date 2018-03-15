using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using Xamarin.Forms;

namespace RecognizeTextApp
{
    public partial class MainPage : ContentPage
    {
        private readonly VisionServiceClient visionClient;

        public MainPage()
        {
            InitializeComponent();
            this.visionClient = new VisionServiceClient(Constants.SecretKey1, Constants.EndPoint);
        }

        private async void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            // カメラで写真を撮る
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions());
            if (file == null) return;

            this.Indicator1.IsVisible = true;
            this.Indicator1.IsRunning = true;
            Image1.Source = ImageSource.FromStream(() => file.GetStream());

            // クラウドにアクセスする前にネットワーク接続のチェックをする
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Network error", "Please check your network connection and retry.", "OK");
                return;
            }

            // カメラで撮った画像からテキストを取り出す
            OcrResults ocrResult = await visionClient.RecognizeTextAsync(file.GetStream());

            this.BindingContext = null;
            this.BindingContext = ocrResult;

            // OcrResults型から解析結果のテキストを取り出し画面に貼り付ける
            foreach (var region in ocrResult.Regions) {
                foreach (var line in region.Lines) {
                    var lineStack = new StackLayout { Orientation = StackOrientation.Horizontal };
                    foreach (var word in line.Words) {
                        var textLabel = new Label { Text = word.Text };
                        lineStack.Children.Add(textLabel);
                    }
                    this.DetectedText.Children.Add(lineStack);
                }
            }

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }
    }
}