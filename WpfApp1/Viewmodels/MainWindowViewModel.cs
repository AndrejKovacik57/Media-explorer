namespace WpfApp1.Viewmodels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel = new VideoTilesViewModel();
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            OnPropertyChanged(nameof(CurrentViewModel)); 
        }
    }

}