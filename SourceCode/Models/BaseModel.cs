using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidGameBoard.Models
{
    public partial class BaseModel : ObservableObject
    {

        [ObservableProperty]
        private string id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;
    }

    public partial class Person : BaseModel
    {
        // BaseModel 已有 Name 屬性
    }

    public partial class WorkItem : BaseModel
    {
        [ObservableProperty]
        private int score;

        [ObservableProperty]
        private bool isEnabled = true;

        [ObservableProperty]
        private string displayname = string.Empty;

        [ObservableProperty]
        private int seq;
    }

    public partial class DailyRecord : ObservableObject
    {
        [ObservableProperty]
        private DateTime date = DateTime.Today;

        [ObservableProperty]
        private string personId = string.Empty;

        [ObservableProperty]
        private List<string> workItemIds = new(); // 當日有加分的項目ID清單
    }
}
