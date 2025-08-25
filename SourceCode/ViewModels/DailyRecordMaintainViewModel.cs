using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidGameBoard.Common;
using KidGameBoard.Models;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace KidGameBoard.ViewModels
{
    public partial class DailyRecordMaintainViewModel : ObservableObject
    {
        private readonly string _connStr = AppConfigHelper.GetConnectionString("KidGameDatabase");

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<Person> people = new();

        [ObservableProperty]
        private Person? selectedPerson1;

        [ObservableProperty]
        private Person? selectedPerson2;

        [ObservableProperty]
        private ObservableCollection<WorkItem> workItems = new();

        [ObservableProperty]
        private ObservableCollection<string> checkedWorkItemIds1 = new();

        [ObservableProperty]
        private ObservableCollection<string> checkedWorkItemIds2 = new();

        [ObservableProperty]
        private bool isQueryButtonEnable = false;

        public DailyRecordMaintainViewModel()
        {
            LoadPeople();
            LoadWorkItems();
            IsQueryButtonEnable = true;
        }

        private void LoadPeople()
        {
            People.Clear();
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("SELECT id, name, description FROM person", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int descIndex = reader.GetOrdinal("description");
                People.Add(new Person
                {
                    Id = reader.GetString(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.IsDBNull(descIndex) ? "" : reader.GetString(descIndex)
                });
            }
            // 預設選擇前兩位
            if (People.Count > 0) SelectedPerson1 = People[0];
            if (People.Count > 1) SelectedPerson2 = People[1];
        }

        private void LoadWorkItems()
        {
            WorkItems.Clear();
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("SELECT id, name, description, score, isEnabled, seq FROM workitem WHERE isEnabled = 1 Order By seq", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int descIndex = reader.GetOrdinal("description");
                WorkItem newItem = new();
                newItem.Id = reader.GetString(reader.GetOrdinal("id"));
                newItem.Name = reader.GetString(reader.GetOrdinal("name"));
                newItem.Description = reader.IsDBNull(descIndex) ? "" : reader.GetString(descIndex);
                newItem.Score = reader.GetInt32(reader.GetOrdinal("score"));
                newItem.Displayname = $"{newItem.Name}({newItem.Score})";
                newItem.IsEnabled = reader.GetBoolean(reader.GetOrdinal("isEnabled"));
                newItem.Seq = reader.GetInt32(reader.GetOrdinal("seq"));
                WorkItems.Add(newItem);
            }
        }

        [RelayCommand]
        private void ToggleWorkItem1(WorkItem item)
        {
            if (item == null) return;
            if (CheckedWorkItemIds1.Contains(item.Id))
                CheckedWorkItemIds1.Remove(item.Id);
            else
                CheckedWorkItemIds1.Add(item.Id);
            OnPropertyChanged(nameof(TotalScore1));
            SaveDailyRecord(SelectedPerson1, CheckedWorkItemIds1);
        }

        [RelayCommand]
        private void ToggleWorkItem2(WorkItem item)
        {
            if (item == null) return;
            if (CheckedWorkItemIds2.Contains(item.Id))
                CheckedWorkItemIds2.Remove(item.Id);
            else
                CheckedWorkItemIds2.Add(item.Id);
            OnPropertyChanged(nameof(TotalScore2));
            SaveDailyRecord(SelectedPerson2, CheckedWorkItemIds2);
        }

        public int TotalScore1 => WorkItems.Where(w => CheckedWorkItemIds1.Contains(w.Id)).Sum(w => w.Score);
        public int TotalScore2 => WorkItems.Where(w => CheckedWorkItemIds2.Contains(w.Id)).Sum(w => w.Score);

        private void SaveDailyRecord(Person? person, ObservableCollection<string> checkedWorkItemIds)
        {
            if (person == null) return;
            var workItemIdsStr = string.Join(",", checkedWorkItemIds);
            var dateStr = SelectedDate.Date.ToString("yyyy-MM-dd");

            using var conn = new MySqlConnection(_connStr);
            conn.Open();

            // 先檢查是否已有該人該日的紀錄
            var checkCmd = new MySqlCommand("SELECT id FROM dailyrecord WHERE date=@date AND personId=@personId", conn);
            checkCmd.Parameters.AddWithValue("@date", dateStr);
            checkCmd.Parameters.AddWithValue("@personId", person.Id);
            var existingId = checkCmd.ExecuteScalar() as string;

            if (existingId != null)
            {
                // 更新
                var updateCmd = new MySqlCommand("UPDATE dailyrecord SET workItemIds=@workItemIds WHERE id=@id", conn);
                updateCmd.Parameters.AddWithValue("@id", existingId);
                updateCmd.Parameters.AddWithValue("@workItemIds", workItemIdsStr);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                // 新增
                var newId = Guid.NewGuid().ToString();
                var insertCmd = new MySqlCommand("INSERT INTO dailyrecord (id, date, personId, workItemIds) VALUES (@id, @date, @personId, @workItemIds)", conn);
                insertCmd.Parameters.AddWithValue("@id", newId);
                insertCmd.Parameters.AddWithValue("@date", dateStr);
                insertCmd.Parameters.AddWithValue("@personId", person.Id);
                insertCmd.Parameters.AddWithValue("@workItemIds", workItemIdsStr);
                insertCmd.ExecuteNonQuery();
            }
        }

        private ObservableCollection<string> LoadDailyRecord(Person? person)
        {
            var checkedWorkItemIds = new ObservableCollection<string>();
            if (person == null) return checkedWorkItemIds;
            var dateStr = SelectedDate.Date.ToString("yyyy-MM-dd");
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("SELECT workItemIds FROM dailyrecord WHERE date=@date AND personId=@personId", conn);
            cmd.Parameters.AddWithValue("@date", dateStr);
            cmd.Parameters.AddWithValue("@personId", person.Id);
            var result = cmd.ExecuteScalar() as string;
            if (!string.IsNullOrEmpty(result))
            {
                foreach (var id in result.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    checkedWorkItemIds.Add(id);
            }
            return checkedWorkItemIds;
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            CheckedWorkItemIds1 = LoadDailyRecord(SelectedPerson1);
            CheckedWorkItemIds2 = LoadDailyRecord(SelectedPerson2);
            OnPropertyChanged(nameof(TotalScore1));
            OnPropertyChanged(nameof(TotalScore2));
        }

        partial void OnSelectedPerson1Changed(Person? value)
        {
            CheckedWorkItemIds1 = LoadDailyRecord(SelectedPerson1);
            OnPropertyChanged(nameof(TotalScore1));
        }

        partial void OnSelectedPerson2Changed(Person? value)
        {
            CheckedWorkItemIds2 = LoadDailyRecord(SelectedPerson2);
            OnPropertyChanged(nameof(TotalScore2));
        }
    }
}
