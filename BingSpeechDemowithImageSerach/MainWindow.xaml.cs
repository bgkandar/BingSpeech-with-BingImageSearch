using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BingSpeechDemowithImageSerach
{
    using System.Media;
    using System.Threading;
    using System.Windows.Threading;
    
    using Microsoft.CognitiveServices.SpeechRecognition;
    using System.Net.Http;
    using Newtonsoft.Json;
    using System.Web;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MicrophoneRecognitionClient micClient;
        public MainWindow()
        {
            InitializeComponent();
            this.micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                SpeechRecognitionMode.ShortPhrase,
                "en-US",
                "Your_key_Bing_Speech_API");
            this.micClient.OnMicrophoneStatus += MicClient_OnMicrophoneStatus;
            this.micClient.OnResponseReceived += MicClient_OnResponseReceived;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.MySpeechResponse.Text = string.Empty;
            this.MySpeechResponseConfidence.Text = string.Empty;
            this.searchImage.Source = null;
            this.micClient.StartMicAndRecognition();
        }

        private void MicClient_OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(
                    () =>
                    {
                        if (e.Recording)
                        {
                            this.status.Text = "Listening";
                            this.RecordingBar.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.status.Text = "Not Listening";
                            this.RecordingBar.Visibility = Visibility.Collapsed;
                        }
                    }));
        }

        private async void MicClient_OnResponseReceived(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.Results.Length > 0)
            {
                await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal, new Action(() =>
                    {
                        this.MySpeechResponse.Text = $"'{e.PhraseResponse.Results[0].DisplayText}',";
                        this.MySpeechResponseConfidence.Text = $"confidence: { e.PhraseResponse.Results[0].Confidence}";
                    }));
                this.SearchImage(e.PhraseResponse.Results[0].DisplayText);
            }
        }
        private async void SearchImage(string phraseToSearch)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "Your_Key_Bing_Image_Search_API");

            // Request parameters
            queryString["q"] = phraseToSearch;
            queryString["count"] = "1";
            queryString["offset"] = "0";
            queryString["mkt"] = "en-us";
            queryString["safeSearch"] = "Moderate";
            var uri = "https://api.cognitive.microsoft.com/bing/v5.0/images/search?" + queryString;

            var response = await client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();
            // MessageBox.Show(json.ToString());
            BingImageSearchResponse bingImageSearchResponse = JsonConvert.DeserializeObject<BingImageSearchResponse>(json);
            var uriSource = new Uri(bingImageSearchResponse.value[0].contentUrl, UriKind.Absolute);

            await Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal, new Action(() =>
                {
                    this.searchImage.Source = new BitmapImage(uriSource);

                }));
        }

    }
}
