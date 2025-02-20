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
    private int Width { get;  set; }
    private int Height { get;  set; }
    
    public List<string> VideoPaths { get; private set; }
    
    public (string VideoPath, string[] VideoPaths) VideoData { get; set; }

    public VideoPreview(string videoPath, int width, int height, List<string> videoPaths)
    {
        VideoPath = videoPath;
        FileName = GetNameFromPath();
        Width = width;
        Height = height;
        VideoPaths = videoPaths;

    }

    private string GetNameFromPath()
    {
        
        return Path.GetFileNameWithoutExtension(Path.GetFileName(VideoPath));
    }
    
}