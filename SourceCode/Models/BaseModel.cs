using CommunityToolkit.Mvvm.ComponentModel;

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

    public partial class Redemption : BaseModel
    {
        [ObservableProperty]
        private string personId;

        [ObservableProperty]
        private int score;

        [ObservableProperty]
        private DateTime redemptionDate;
    }

    public partial class RedemptionRecord : Redemption
    {
        [ObservableProperty]
        private string personName;
    }

    public class PersonScore
    {
        public string Name { get; set; } = "";
        
        public int TotalScore { get; set; }

        public int RedeemedScore { get; set; }

        public int AvailableScore { get; set; }

    }
}
