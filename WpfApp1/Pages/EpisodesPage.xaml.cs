using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

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
        // Get the VideoPath from the Button's Tag property
        Button button = sender as Button;
        if (button != null)
        {
            string videoPath = button.Tag as string;
            if (!string.IsNullOrEmpty(videoPath))
            {
                Console.WriteLine("Playing video " + videoPath);
            }
        }
    }

    private void  LoadPreviews()
    {
        foreach (string path in CurrentItem.VideoPaths)
        {
            VideoPreview preview = new VideoPreview(path, 200, 100);
            VideoPreviews.Add(preview);
        }
    }
   
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}