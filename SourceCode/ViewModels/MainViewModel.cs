using CommunityToolkit.Mvvm.ComponentModel;

namespace KidGameBoard.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isQueryButtonEnable = false;
        public MainViewModel()
        {
            IsQueryButtonEnable = true;
        }
    }
}
