using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace WpfApp1;
using Models;



public partial class TilesPage : CustomBasePage
{
    private VideoTile selectedVideoTile;
    private VideoTile[] AllVideoTiles;
    private List<VideoTile>? FilteredVideoTiles;
    
    public TilesPage()
    {
        InitializeComponent();
        Console.WriteLine($"1 {Environment.GetEnvironmentVariable("path1")}");
        Console.WriteLine($"2 {Environment.GetEnvironmentVariable("path1")}");
        LoadImagesFromSubfolders([Environment.GetEnvironmentVariable("path1"), Environment.GetEnvironmentVariable("path2")]);

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
            Console.WriteLine($"Processing root folder: {rootFolderPath}");
            // Get all subfolders in the main directory
            string[] subFolders = Directory.GetDirectories(rootFolderPath);

            foreach (var folder in subFolders)
            {
                Console.WriteLine($"Processing folder: {folder}");
                // Get the first image file in the folder
                string imageFile = Directory
                    .GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                            file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                            file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException();  // Selects the first matching image file or null if none found
                
                if (imageFile == null)
                {
                    Console.WriteLine($"Skipping folder {folder}: No valid image file found.");
                    continue; // Skip folder if no valid image file is found
                }
                // Get the first video file in the folder
                string[] videoFiles = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".wmv", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (videoFiles.Length == 0)
                {
                    Console.WriteLine($"Skipping folder {folder}: No valid video files found.");
                    continue; // Skip folder if no valid video files are found
                }
                var output = GetNameAndEpisodes($"{folder}\\tile.txt", videoFiles.Length);
                if (string.IsNullOrEmpty(output.name) || output.episodes == 0)
                {
                    Console.WriteLine($"Skipping folder {folder}: Missing or invalid episode details.");
                    continue; // Skip folder if episode details are missing
                }
                string episodeName = output.name;
                int episodeNum = output.episodes;
                List<string> tags = GetTagsFromPath($"{folder}\\tags.txt");
                if (!tags.Any())
                {
                    Console.WriteLine($"Skipping folder {folder}: Missing or invalid tag details.");
                    continue; // Skip folder if episode details are missing
                }
                
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

    private void FilterEpisodes(TextBox searchBar)
    {
        string searchText = searchBar?.Text;

        if (string.IsNullOrEmpty(searchText))
        {
            // If search text is empty, display all tiles
            DataContext = AllVideoTiles;
            return;
        }

        FilteredVideoTiles = new List<VideoTile>();

        string tagString;
        string pattern = @"[+-](""[^""]+""|\S+)|\b\w+\b";
        // Improved pattern to correctly match quoted strings and other tokens.

        var matches = Regex.Matches(searchText, pattern);

        // Convert the matches to a string array
        string[] commands = matches.Cast<Match>().Select(m => m.Value).ToArray();
        // Convert the matches to a string array

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
                            .Where(videoTileIter => videoTileIter.Title.ToLower().Contains(command.ToLower(), StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        FilteredVideoTiles.AddRange(filteredTiles);
                    }
                    else
                    {
                        var filteredTiles = FilteredVideoTiles
                            .Where(videoTileIter => videoTileIter.Title.ToLower().Contains(command.ToLower(), StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        FilteredVideoTiles = filteredTiles;
                    }
                }
            }
        }
        // Update the data context with the filtered tiles
        DataContext = FilteredVideoTiles;
    }

    private void OnSearchTextChanged(object sender, KeyEventArgs keyEventArgs)
    {
        TextBox searchBar = sender as TextBox;
        FilterEpisodes(searchBar);

    }

    private void TilesContentControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Cast the sender to a ContentControl
        ContentControl contentControl = sender as ContentControl;

        if (contentControl != null)
        {
            // Find all buttons inside the ContentControl
            foreach (Button button in FindVisualChildren<Button>(contentControl))
            {
                // Check if the button has a Tag and if the Tag is "TagElement"
                if (button.Tag != null && button.Tag.ToString() == "TagElement")
                {
                    // Attach the click event to the button
                    button.Click += Button_Click;
                }
            }
        }
    }

// Helper method to recursively search for all visual children of a specific type (Button in this case)
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }




    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Button clickedButton = sender as Button;
        if (clickedButton != null && clickedButton.Content != null)
        {
            string button_text = clickedButton.Content.ToString().ToLower();
            if (!SearchBar.Text.Contains(button_text))
            {
                if (string.IsNullOrEmpty(SearchBar.Text))
                {

                    if (button_text.Contains(' '))
                    {
                        SearchBar.Text = $"+\"{button_text}\"";
                    }
                    else
                    {
                        SearchBar.Text = $"+{button_text}";
                    }
                }
                else
                {
                    if (button_text.Contains(' '))
                    {
                        SearchBar.Text += $" +\"{button_text}\"";
                    }
                    else
                    {
                        SearchBar.Text += $" +{button_text}";
                    }
                }
            }
           
        }
        
        FilterEpisodes(SearchBar);
    }

    public override Dictionary<string, object> GetPageState()
    {
        throw new NotImplementedException();
    }

    public override void RestorePageState(Dictionary<string, object> state)
    {
        throw new NotImplementedException();
    }
}