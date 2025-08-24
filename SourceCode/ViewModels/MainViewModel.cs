using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidGameBoard.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool isQueryButtonEnable = false;
        public MainViewModel()
        {
            IsQueryButtonEnable = true;
        }
    }
}
