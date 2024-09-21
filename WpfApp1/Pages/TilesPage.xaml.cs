using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1;
using System.Windows.Media.Imaging;
using Models;
using System.IO;


public partial class TilesPage : Page
{
    private VideoTile selectedVideoTile;
    private VideoTile[] AllVideoTiles;
    private List<VideoTile>? FilteredVideoTiles;
    
    public TilesPage()
    {
        InitializeComponent();
        LoadImagesFromSubfolders([Environment.GetEnvironmentVariable("path1"), Environment.GetEnvironmentVariable("path2") ]);

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
                
                VideoTile videoTile = new VideoTile(bitmap, tags, episodeName, episodeNum, videoFiles);
                videoTiles.Add(videoTile);
            }

            
        }
        // Bind the images to the ItemsControl
        AllVideoTiles = videoTiles.ToArray();
        DataContext = videoTiles;
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
    
        // Get the button that was clicked
        var button = sender as Button;

        // Retrieve the bound data object (which is passed through CommandParameter)
        var currentItem = button?.CommandParameter as VideoTile;

        if (currentItem != null)
        {
            // Navigate to EpisodesPage and pass the currentItem (VideoTile)
            mainWindow.TilesFrame.Navigate(new EpisodesPage(currentItem));
        }
        else
        {
            MessageBox.Show("No item selected.");
        }
    }

    private void OnSearchTextChanged(object sender, KeyEventArgs keyEventArgs)
    {
        TextBox searchBar = sender as TextBox;
        string searchText = searchBar?.Text;

        if (string.IsNullOrEmpty(searchText))
        {
            // If search text is empty, display all tiles
            DataContext = AllVideoTiles;
            return;
        }

        FilteredVideoTiles = new List<VideoTile>();

        string tagString;
        string[] commands = searchText.Split(' ');

        foreach (var command in commands)
        {
            if (!string.IsNullOrEmpty(command))
            {
                Console.WriteLine($"command = {command}");
                if (command.StartsWith("+"))
                {
                    Console.WriteLine("plus");
                    tagString = command.Substring(1).ToLower();
                    tagString = tagString.Replace("\"", "").Replace("'", "");
                    
                    Console.WriteLine($"tagString {tagString}");
                    if (!string.IsNullOrEmpty(tagString))
                    {
                        if (!FilteredVideoTiles.Any())
                        {
                            var filteredTiles = AllVideoTiles
                                .Where(videoTileIter =>
                                    videoTileIter.Tags.Any(tag => tag.ToLower().Contains(tagString)))
                                .ToList();
                            FilteredVideoTiles.AddRange(filteredTiles);
                        }
                        else
                        {
                            var filteredTiles = FilteredVideoTiles
                                .Where(videoTileIter =>
                                    videoTileIter.Tags.Any(tag => tag.ToLower().Contains(tagString)))
                                .ToList();
                            FilteredVideoTiles = filteredTiles;
                        }
                    }
                }
                else if (command.StartsWith("-"))
                {
                    tagString = command.Substring(1).ToLower();
                    tagString = tagString.Replace("\"", "").Replace("'", "");
                    Console.WriteLine("minus");
                    Console.WriteLine($"tagString {tagString}");
                    if (!string.IsNullOrEmpty(tagString))
                    {
                        if (!FilteredVideoTiles.Any())
                        {
                            var filteredTiles = AllVideoTiles
                                .Where(videoTileIter =>
                                    videoTileIter.Tags.All(tag => !tag.ToLower().Contains(tagString)))
                                .ToList();
                            FilteredVideoTiles.AddRange(filteredTiles);
                        }
                        else
                        {
                            var filteredTiles = FilteredVideoTiles
                                .Where(videoTileIter =>
                                    videoTileIter.Tags.All(tag => !tag.ToLower().Contains(tagString)))
                                .ToList();
                            FilteredVideoTiles = filteredTiles;
                        }
                    }
                }
                else
                {
                    if (!FilteredVideoTiles.Any())
                    {
                        var filteredTiles = AllVideoTiles
                            .Where(videoTileIter => videoTileIter.Title.ToLower().Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        FilteredVideoTiles.AddRange(filteredTiles);
                    }
                    else
                    {
                        searchText = searchText.ToLower();
                        var filteredTiles = FilteredVideoTiles
                            .Where(videoTileIter => videoTileIter.Title.ToLower().Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        FilteredVideoTiles = filteredTiles;
                    }
                }
            }
        }
        // Update the data context with the filtered tiles
        DataContext = FilteredVideoTiles;
    }

}