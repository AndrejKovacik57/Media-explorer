using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp1.Pages
{
    public partial class VideoPlayer : Page
    {
        public string[] VideoPaths { get; private set; }
        public string SelectedVideoPath { get; private set; }
        private bool isPlaying = true;
        private bool isFullScreen = false;
        private bool isDragging = false; // To prevent conflicts when the user drags the slider
        private int currentIndex;
        private DispatcherTimer timer; // For updating the timeline slider
        

        public VideoPlayer(string videoPath, string[] videoPaths)
        {
            InitializeComponent();
            VideoPaths = videoPaths;
            SelectedVideoPath = videoPath;
            currentIndex = Array.IndexOf(VideoPaths, SelectedVideoPath);
            MediaPlayer.Source = new Uri(SelectedVideoPath, UriKind.Absolute);
            MediaPlayer.Play(); // Add this line to start playing automatically.
            MediaPlayer.Volume = 0.1f;
            PlayPauseButton.Content = "Pause";
            isPlaying = true;
            
            // Create and start the timer to update the slider
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500); // Update every half-second
            timer.Tick += Timer_Tick;
            timer.Start();
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded; // Subscribe to the MediaEnded event
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
    }
}
