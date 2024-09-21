using  System.IO;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp1.Models;
public class VideoPreview
{
    public string VideoPath { get; set; }
    public string FileName { get; private set; }
    public Image Preview { get; private set; }
    private int Width { get;  set; }
    private int Height { get;  set; }
    
    

    public VideoPreview(string videoPath, int width, int height)
    {
        VideoPath = videoPath;
        FileName = GetNameFromPath();
        Width = width;
        Height = height;
        Preview = CreateVideoPreview();

    }

    private string GetNameFromPath()
    {
        return Path.GetFileName(VideoPath);
    }
    
    private Image CreateVideoPreview()
    {
        Image newImage = new Image();
        MediaPlayer mediaPlayer = new MediaPlayer
        {
            IsMuted = true
        };

        // Load the video file
        mediaPlayer.Open(new Uri(VideoPath));

        // Register a MediaOpened event to get the video information once it is loaded
        mediaPlayer.MediaOpened += (sender, args) =>
        {
            // Seek to the position (e.g., 1 second) in the video to get a preview frame
            mediaPlayer.Position = TimeSpan.FromSeconds(5);
            
            // Use a DispatcherTimer to ensure the frame is rendered after seeking
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += (s, ev) =>
            {
                timer.Stop();
                // Create a render target to capture the current frame
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    Width, Height, 96, 96, PixelFormats.Pbgra32);

                // Create a visual brush to draw the video frame onto the render target
                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext context = visual.RenderOpen())
                {
                    context.DrawVideo(mediaPlayer, new Rect(0, 0, Width, Height));
                }
                rtb.Render(visual);

                newImage.Source = rtb;
                newImage.Stretch = Stretch.Uniform;
                newImage.Height = Height;
                newImage.Width = Width;
                // Set the rendered bitmap as the source for the Image control
                // VideoPreview.Source = rtb;

                // Close the media player (cleanup)
                mediaPlayer.Close();
            };
            timer.Start();
        };
        return newImage;
    }
    
    
}