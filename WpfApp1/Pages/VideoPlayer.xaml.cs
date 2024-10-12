using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfApp1.Models;
using Sharpcaster.Models;
using System.Net;
using System.Net.Sockets;
using Sharpcaster;
using Sharpcaster.Interfaces;
using Sharpcaster.Models.Media;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace WpfApp1.Pages

{
    public partial class VideoPlayer : CustomBasePage
    {
        public string[] VideoPaths { get; private set; }
        public string SelectedVideoPath { get; private set; }
        private bool isPlaying;
        private bool isFullScreen = false;
        private bool isDragging = false; // To prevent conflicts when the user drags the slider
        private int currentIndex;
        private DispatcherTimer timer; // For updating the timeline slider
        private readonly string ipAddress;
        private const string Port = "5000";
        private ChromecastClient? chromecastClient;
        private MediaServer? localMediaServer;


        // private List<ChromecastDevice> chromecastDevices;

        public VideoPlayer(string videoPath, string[] videoPaths)
        {
            InitializeComponent();
            VideoPaths = videoPaths;
            SelectedVideoPath = videoPath;
            ipAddress = GetLocalIpAddress();
            currentIndex = Array.IndexOf(VideoPaths, SelectedVideoPath);
            MediaPlayer.Source = new Uri(SelectedVideoPath, UriKind.Absolute);
            MediaPlayer.Play(); // Add this line to start playing automatically.
            MediaPlayer.Volume = 0.1f;
            PlayPauseButton.Content = "Pause";
            isPlaying = true;
            
            // Create and start the timer to update the slider
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500); // Update every half-second
            timer.Tick += Timer_Tick!;
            timer.Start();
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded; // Subscribe to the MediaEnded event
            FadeOutAtStart(new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)));
            // Display available Chromecast devices for selection

        }


        async private void FadeOutAtStart(DoubleAnimation fadeOut)
        {
            await Task.Delay(4000);
            ControlPanel.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                // Update the slider only if it's not being dragged
                TimelineSlider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TimelineSlider.Value = MediaPlayer.Position.TotalSeconds;
                CurrentTimeLabel.Content = MediaPlayer.Position.ToString(@"hh\:mm\:ss");
                TotalTimeLabel.Content = MediaPlayer.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
            }
        }
        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (ReplayCheckBox.IsChecked == true)
            {
                MediaPlayer.Position = TimeSpan.Zero; // Reset to the start
                MediaPlayer.Play(); // Replay the video
            }
            else
            {
                PlayPauseButton.Content = "Play";
                isPlaying = false;
            }
        }
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                MediaPlayer.Pause();
                PlayPauseButton.Content = "Play";
            }
            else
            {
                MediaPlayer.Play();
                PlayPauseButton.Content = "Pause";
            }
            isPlaying = !isPlaying;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < VideoPaths.Length - 1)
            {
                currentIndex++;
                PlayVideoAtIndex(currentIndex);
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                PlayVideoAtIndex(currentIndex);
            }
        }

        private void PlayVideoAtIndex(int index)
        {
            SelectedVideoPath = VideoPaths[index];
            MediaPlayer.Source = new Uri(SelectedVideoPath, UriKind.Absolute);
            MediaPlayer.Play();
            PlayPauseButton.Content = "Pause";
            isPlaying = true;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaPlayer != null)
            {
                MediaPlayer.Volume = VolumeSlider.Value;
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isFullScreen)
            {
                EnterFullScreen();
            }
            else
            {
                ExitFullScreen();
            }
        }

        private void EnterFullScreen()
        {
            Window fullScreenWindow = new Window
            {
                Content = this.Content,
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Topmost = true
            };

            fullScreenWindow.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    ExitFullScreen(fullScreenWindow);
                }
            };

            fullScreenWindow.Show();
            isFullScreen = true;
        }

        private void ExitFullScreen(Window fullScreenWindow = null)
        {
            if (fullScreenWindow != null)
            {
                fullScreenWindow.Close();
            }

            isFullScreen = false;
        }

        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Handle seeking the video when the slider value changes
            if (isDragging && MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                MediaPlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
                CurrentTimeLabel.Content = MediaPlayer.Position.ToString(@"hh\:mm\:ss");
            }
        }

        private void TimelineSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Handle slider click to allow direct seeking
            var slider = sender as Slider;
            var mousePosition = e.GetPosition(slider);
            var percentage = mousePosition.X / slider.ActualWidth;
            var newTime = percentage * slider.Maximum;

            // Update the MediaElement position
            MediaPlayer.Position = TimeSpan.FromSeconds(newTime);
            TimelineSlider.Value = newTime; // Update the slider
        }

        private void TimelineSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // User finished dragging, resume normal timeline updates
            MediaPlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
        }
        private void ControlPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            // Show the control panel when the mouse enters
            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));

            ControlPanel.BeginAnimation(StackPanel.OpacityProperty, fadeIn);
        }

        private void ControlPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            // Hide the control panel when the mouse leaves
            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));

            ControlPanel.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }

        async public void ChromecastButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            IChromecastLocator locator = new MdnsChromecastLocator();
            var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500));

            try
            {
                // Find available Chromecast devices
                var chromecasts = await locator.FindReceiversAsync(source.Token);

                var chromecastReceivers = chromecasts.ToList();
                if (chromecastReceivers.Any())
                {
                    // Populate ComboBox with Chromecast devices
                    ChromecastDevicesComboBox.ItemsSource = chromecastReceivers;
                    ChromecastDevicesComboBox.SelectedIndex = 0;  // Optionally set default selection
                    ChromecastDevicesComboBox.Visibility = Visibility.Visible;
                    ChromecastToDevice.IsEnabled = true;
                    ChromecastToDevice.Visibility = Visibility.Visible;

                }
                else
                {
                    MessageBox.Show("No Chromecast devices found.");
                    ChromecastDevicesComboBox.ItemsSource = null;
                    ChromecastDevicesComboBox.Visibility = Visibility.Collapsed;
                    ChromecastToDevice.IsEnabled = false;
                    ChromecastToDevice.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while finding Chromecast devices: {ex.Message}");
            }
        }
  
        async void ChromecastToDeviceButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (ChromecastDevicesComboBox.SelectedItem is ChromecastReceiver selectedChromecast)
            {
                Console.WriteLine($"Connecting to {selectedChromecast.Name}...");
                Console.WriteLine($"localIpAddress {ipAddress}");
                
                localMediaServer = new MediaServer();
                localMediaServer.StartServer(SelectedVideoPath, Port);
                
                chromecastClient = new ChromecastClient();
                await chromecastClient.ConnectChromecast(selectedChromecast);
                var launchResult = await chromecastClient.LaunchApplicationAsync("B3419EF5");
                
                if (launchResult == null)
                {
                    MessageBox.Show("Failed to launch Chromecast application.");
                    return;
                }
                
                var media = new Media
                {
                    ContentUrl = $"http://{ipAddress}:{Port}/"
                };
                _ = await chromecastClient.MediaChannel.LoadAsync(media);
                // Show the stop cast button
                StopCastButton.Visibility = Visibility.Visible;
                
                // Subscribe to the Disconnected event
                chromecastClient.Disconnected += ChromecastClient_Disconnected;
            }
            else
            {
                MessageBox.Show("Please select a Chromecast device.");
            }
            
        }
        private async void StopCastButton_Click(object sender, RoutedEventArgs e)
        {
            if (chromecastClient?.MediaChannel != null)
            {
                // Stop the currently playing media
                await chromecastClient.MediaChannel.StopAsync();
            }

            if (localMediaServer != null)
            {
                localMediaServer.StopServer();
                localMediaServer = null;
            }
            // Hide the stop cast button
            StopCastButton.Visibility = Visibility.Hidden;

            MessageBox.Show("Casting stopped.");
        }
        private void ChromecastClient_Disconnected(object sender, EventArgs e)
        {
            // This event gets triggered when Chromecast is disconnected

            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Casting was stopped on the TV.");

                // Hide the stop cast button and stop the media server
                StopCastButton.Visibility = Visibility.Collapsed;

                if (localMediaServer != null)
                {
                    localMediaServer.StopServer();
                    localMediaServer = null;
                }

                // Optionally, you can disconnect the ChromecastClient
                chromecastClient = null;
            });
        }

        public static string GetLocalIpAddress()
        {
            string localIp;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint!.Address.ToString();
            }
            Console.WriteLine($"Addresa je  {localIp}");
            return localIp;
        }
        public override Dictionary<string, object> GetPageState()
        {
            throw new NotImplementedException();
        }

        public override void RestorePageState(Dictionary<string, object> state)
        {
            throw new NotImplementedException();
        }
    }
}
