using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;
using WpfApp1.Pages;

namespace WpfApp1;

public partial class EpisodesPage : Page, INotifyPropertyChanged
{

    public event PropertyChangedEventHandler PropertyChanged;

    private VideoTile _currentItem;
    public VideoTile CurrentItem { 
        get{ return _currentItem; }
        set
        {
            if (_currentItem != value)
            {
                _currentItem = value;
                OnPropertyChanged();
            }
        }
        
    }
    private List<VideoPreview>  _videoPreviews;
    public List<VideoPreview> VideoPreviews { 
        get{ return _videoPreviews; }
        set
        {
            if (_videoPreviews != value)
            {
                _videoPreviews = value;
                OnPropertyChanged();
            }
        }
        
    }
    
    public EpisodesPage(VideoTile currentItem)
    {
        
        InitializeComponent();
        CurrentItem = currentItem;
        VideoPreviews = new List<VideoPreview>();
        LoadPreviews();
        DataContext = this;

    }
    private void PlayVideo_Click(object sender, RoutedEventArgs e)
    {
        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow ?? throw new InvalidOperationException("MainWindow is null.");
        // Get the VideoPath from the Button's Tag property
        Button button = sender as Button;
        if (button != null)
        {
            VideoPreview videoData = (VideoPreview)button.Tag;
            
            if (!string.IsNullOrEmpty(videoData.VideoPath) && videoData.VideoPaths.Length > 0)
            {
                // Navigate to EpisodesPage and pass the currentItem (VideoTile)
                mainWindow.TilesFrame.Navigate(new VideoPlayer(videoData.VideoPath, videoData.VideoPaths));
            } 
            else
            {
                 MessageBox.Show("No video files");
            }
        }
    }

    private void  LoadPreviews()
    {
        foreach (string path in CurrentItem.VideoPaths)
        {
            VideoPreview preview = new VideoPreview(path, 200, 100, CurrentItem.VideoPaths);
            VideoPreviews.Add(preview);
        }
    }
   
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}