namespace WpfApp1.Models;
using System.Windows.Media.Imaging;

public class VideoTile(Uri imageUri, List<string> tags, string title, int episodeNumber, List<string> videoPaths)
{
    public string Title { get; private set; } = title;
    public Uri ImageUri { get; private set; } = imageUri;
    public List<string> VideoPaths { get; private set; } = videoPaths;
    public List<string> Tags { get; private set; } = tags;
    public int EpisodeNumber { get; private set; } = episodeNumber;
}