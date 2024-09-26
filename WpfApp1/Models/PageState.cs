namespace WpfApp1.Models;

public class PageState
{
    public Type PageType { get; set; } // To recreate the correct page
    public Dictionary<string, object> StateData { get; set; } = new Dictionary<string, object>();
}