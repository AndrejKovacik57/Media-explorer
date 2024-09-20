namespace WpfApp1.Models;
using System.Windows.Media.Imaging;

public class VideoTile
{
    public string Title { get; private set; }
    public BitmapImage Image { get; private set; }
    public string[] VideoPaths { get; private set; }
    public List<string> Tags { get; private set; }
    public int EpisodeNumber { get; private set; }
    

    public VideoTile(BitmapImage image, List<string> tags, string title, int episodeNumber, string[]videoPaths)
    {
        Image = image;
        Tags = tags;
        Title = title;
        EpisodeNumber = episodeNumber;
        VideoPaths = videoPaths;
    }
    
}