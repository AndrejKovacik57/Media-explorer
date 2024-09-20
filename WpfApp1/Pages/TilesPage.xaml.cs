using System.Windows;
using System.Windows.Controls;

namespace WpfApp1;
using System.Windows.Media.Imaging;
using WpfApp1.Models;
using System.IO;


public partial class TilesPage : Page
{
    private VideoTile selectedVideoTile;
    public TilesPage()
    {
        InitializeComponent();
        LoadImagesFromSubfolders([@"E:\media\videa\hentaimama\2D", @"E:\media\videa\hentaimama\3D"]);

    }
        private void LoadImagesFromSubfolders(List<string> rootFolderPaths)
    {
        List<VideoTile> videoTiles = [];
        foreach (string rootFolderPath in rootFolderPaths)
        { 
            // Check if the root folder exists
            if (!Directory.Exists(rootFolderPath))
            {
                MessageBox.Show("Directory does not exist.");
                continue;
            }

            // Get all subfolders in the main directory
            string[] subFolders = Directory.GetDirectories(rootFolderPath);

            foreach (var folder in subFolders)
            {
                // Get the first image file in the folder
                string imageFile = Directory
                    .GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                            file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                            file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException();  // Selects the first matching image file or null if none found

                // Get the first video file in the folder
                string[] videoFiles = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".wmv", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                
                var output = GetNameAndEpisodes($"{folder}\\tile.txt", videoFiles.Length);
                string episodeName = output.name;
                int episodeNum = output.episodes;
                List<string> tags = GetTagsFromPath($"{folder}\\tags.txt");
                
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageFile, UriKind.Absolute);  // Load the first image
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                Console.WriteLine($"tags {tags} episodeName {episodeName} episodeNum {episodeNum}");
                VideoTile videoTile = new VideoTile(bitmap, tags, episodeName, episodeNum, videoFiles);
                videoTiles.Add(videoTile);
            }

            
        }
        // Bind the images to the ItemsControl
        ImageItemsControl.ItemsSource = videoTiles;
    }

    private List<string> GetTagsFromPath(string path)
    {
        List<string> tags = new List<string>();
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    tags.Add(line);
                }
            } 
        }

        return tags;
    }
    private (string name, int episodes) GetNameAndEpisodes(string path, int epNum)
    {
        //"E:\media\videa\hentaimama\2D\1LDK_+_JK_Ikinari_Doukyo_Micchaku!_Hatsu_Ecchi!!\tile.txt"
        string name = "Episode name";
        int episodes = epNum;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string nameLine  = reader.ReadLine();
                if (nameLine != null)
                {
                    name = nameLine;
                }
                string line = reader.ReadLine();
                if (line != null)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        int.TryParse(parts[1].Trim(), out episodes);
                    }
                }
                
            } 
        }
        return (name, episodes);
    }

    private void ShowEpisodes_OnClick(object sender, RoutedEventArgs e)
    {
        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow ?? throw new InvalidOperationException("MainWindow is null.");
        
        mainWindow.TilesFrame.Navigate(new EpisodesPage());
        // Get the button that was clicked
        var button = sender as Button;

        // Retrieve the bound data object (which is passed through CommandParameter)
        var currentItem = button?.CommandParameter;

        if (currentItem != null)
        {
            // You can now use currentItem to access the properties of the bound data object
            MessageBox.Show($"Episodes for: {((VideoTile)currentItem).Title}");
        
            // Perform other logic like navigating to a new page or displaying episodes
        }
    }
}