using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp1.Commands;
using WpfApp1.Models;
using WpfApp1.MVVM;

namespace WpfApp1.Viewmodels;

internal class VideoTileViewModel : ViewModelBase
{
    private readonly VideoTile _videoTile;

    public VideoTileViewModel(VideoTile videoTile)
    {
        _videoTile = videoTile;
    }

    public string Title => _videoTile.Title;
    public int EpisodeNumber => _videoTile.EpisodeNumber;
    public ObservableCollection<string> Tags => new ObservableCollection<string>(_videoTile.Tags);

    public ICommand ShowEpisodes => new SowEpisodesCommand();
    public BitmapImage Image
    {
        get
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = _videoTile.ImageUri;  
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }
    }
}