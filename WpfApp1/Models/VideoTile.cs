namespace WpfApp1.Models;
using System.Windows.Media.Imaging;

public class VideoTile(BitmapImage image, List<string> tags, string title, int episodeNumber, string[] videoPaths)
{
    public string Title { get; private set; } = title;
    public BitmapImage Image { get; private set; } = image;
    public string[] VideoPaths { get; private set; } = videoPaths;
    public List<string> Tags { get; private set; } = tags;
    public int EpisodeNumber { get; private set; } = episodeNumber;
}