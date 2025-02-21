using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp1.Models;

namespace WpfApp1.Viewmodels;

internal class VideoTilesViewModel : ViewModelBase
{
    private readonly ObservableCollection<VideoTileViewModel> _videoTiles;
    
    public ObservableCollection<VideoTileViewModel> VideoTiles => _videoTiles; 
    public VideoTilesViewModel()
    {
        _videoTiles = new ObservableCollection<VideoTileViewModel>();

 
        var imageUri = new Uri(@"pack://application:,,,/WpfApp1;component/Resources\Images\png-placeholder.png", UriKind.Absolute);

        var videoTile = new VideoTile(
            imageUri,  
            [ "Tag1", "Tag2", "Tag3" ], 
            "Sample Title",
            2,
            [
                @"path",
                @"path"
            ]
        );

        _videoTiles.Add(new VideoTileViewModel(videoTile));
        
        var videoTile2 = new VideoTile(
            imageUri,  
            [ "Tag1", "Tag2", "Tag3" ], 
            "Sample Title2",
            2,
            [
                @"path",
                @"path"
            ]
        );

        _videoTiles.Add(new VideoTileViewModel(videoTile2));
        _videoTiles.Add(new VideoTileViewModel(videoTile2));
        _videoTiles.Add(new VideoTileViewModel(videoTile2));
        _videoTiles.Add(new VideoTileViewModel(videoTile2));
    }
}
