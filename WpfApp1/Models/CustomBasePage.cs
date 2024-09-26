using System.Windows.Controls;

namespace WpfApp1.Models;

public abstract class CustomBasePage: Page
{
// ArrayLists to hold forward and backward navigation
    private List<PageState> _backwardPages = new List<PageState>();
    private List<PageState> _forwardPages = new List<PageState>();
    
    
    
    // Function to save the current page state to backward navigation
    public void SaveCurrentPageStateToBackward()
    {
        PageState pageState = new PageState
        {
            PageType = GetType(),
            StateData = GetPageState()
        };

        _backwardPages.Add(pageState);
        _forwardPages.Clear();
    }

    // Function to save the current page state to forward navigation
    public void SaveCurrentPageStateToForward()
    {
        var pageState = new PageState
        {
            PageType = GetType(),
            StateData = GetPageState()
        };

        _forwardPages.Add(pageState);
    }

    // Function to go back to the previous page
    public PageState GoBack()
    {
        if (_backwardPages.Count > 0)
        {
            var previousPageState = (PageState)_backwardPages[_backwardPages.Count - 1];
            _backwardPages.RemoveAt(_backwardPages.Count - 1);

            SaveCurrentPageStateToForward();
            return previousPageState;
        }
        return null;
    }

    // Function to go forward to the next page
    public PageState GoForward()
    {
        if (_forwardPages.Count > 0)
        {
            var nextPageState = (PageState)_forwardPages[_forwardPages.Count - 1];
            _forwardPages.RemoveAt(_forwardPages.Count - 1);

            SaveCurrentPageStateToBackward();
            return nextPageState;
        }
        return null;
    }

    // Abstract methods for saving/restoring state to be implemented by each page
    public abstract Dictionary<string, object> GetPageState();
    public abstract void RestorePageState(Dictionary<string, object> state);
    
}