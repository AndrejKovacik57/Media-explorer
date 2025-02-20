namespace WpfApp1.Models;

public class PageState
{
    public Type PageType { get; set; } 
    public Dictionary<string, object> StateData { get; set; } = new Dictionary<string, object>();
}