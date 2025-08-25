using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidGameBoard.Common;
using KidGameBoard.Models;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows;

namespace KidGameBoard.ViewModels
{
    public partial class BaseInfoMaintainViewModel : ObservableObject
    {
        private readonly string _connStr = AppConfigHelper.GetConnectionString("KidGameDatabase");

        [ObservableProperty]
        private ObservableCollection<Person> people = new();

        [ObservableProperty]
        private Person? selectedPerson;

        [ObservableProperty]
        private Person editPerson = new();

        [ObservableProperty]
        private bool isQueryButtonEnable = false;

        [ObservableProperty]
        private ObservableCollection<WorkItem> workItems = new();

        [ObservableProperty]
        private WorkItem? selectedWorkItem;

        [ObservableProperty]
        private WorkItem editWorkItem = new();


        public BaseInfoMaintainViewModel()
        {

            LoadPeople();
            LoadWorkItems(); // 載入工作項目
            IsQueryButtonEnable = true;
        }


        #region 人員維護

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
        }

        [RelayCommand]
        private void AddPerson()
        {
            if (string.IsNullOrWhiteSpace(EditPerson.Name)) return;
            var newPerson = new Person
            {
                Id = Guid.NewGuid().ToString(),
                Name = EditPerson.Name,
                Description = EditPerson.Description
            };
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO person (id, name, description) VALUES (@id, @name, @desc)", conn);
            cmd.Parameters.AddWithValue("@id", newPerson.Id);
            cmd.Parameters.AddWithValue("@name", newPerson.Name);
            cmd.Parameters.AddWithValue("@desc", newPerson.Description);
            cmd.ExecuteNonQuery();
            LoadPeople();
            EditPerson = new Person();
        }

        [RelayCommand]
        private void SavePerson()
        {
            if (SelectedPerson == null) return;
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("UPDATE person SET name=@name, description=@desc WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", SelectedPerson.Id);
            cmd.Parameters.AddWithValue("@name", EditPerson.Name);
            cmd.Parameters.AddWithValue("@desc", EditPerson.Description);
            cmd.ExecuteNonQuery();
            LoadPeople();
        }

        [RelayCommand]
        private void DeletePerson()
        {
            if (SelectedPerson == null) return;
            if (MessageBox.Show("確定要刪除這個人員嗎？\n刪除後將無法恢復。", "確認刪除", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;

            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM person WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", SelectedPerson.Id);
            cmd.ExecuteNonQuery();
            LoadPeople();
            SelectedPerson = null;
            EditPerson = new Person();
        }

        partial void OnSelectedPersonChanged(Person? value)
        {
            if (value != null)
                EditPerson = new Person
                {
                    Id = value.Id,
                    Name = value.Name,
                    Description = value.Description
                };
            else
                EditPerson = new Person();
        }

        #endregion

        #region 工作項目維護
        [RelayCommand]
        private void LoadWorkItems()
        {
            WorkItems.Clear();
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("SELECT id, name, description, score, isEnabled, seq FROM workitem Order By seq", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int descIndex = reader.GetOrdinal("description");
                WorkItem newItem = new();
                newItem.Id = reader.GetString(reader.GetOrdinal("id"));
                newItem.Name = reader.GetString(reader.GetOrdinal("name"));
                newItem.Description = reader.IsDBNull(descIndex) ? "" : reader.GetString(descIndex);
                newItem.Score = reader.GetInt32(reader.GetOrdinal("score"));
                newItem.Displayname = $"{newItem.Name} - {newItem.Description}({newItem.Score})";
                newItem.IsEnabled = reader.GetBoolean(reader.GetOrdinal("isEnabled"));
                newItem.Seq = reader.GetInt32(reader.GetOrdinal("seq"));
                WorkItems.Add(newItem);
            }
        }

        [RelayCommand]
        private void AddWorkItem()
        {
            if (string.IsNullOrWhiteSpace(EditWorkItem.Name)) return;
            var newItem = new WorkItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = EditWorkItem.Name,
                Description = EditWorkItem.Description,
                Score = EditWorkItem.Score,
                IsEnabled = EditWorkItem.IsEnabled,
                Seq = EditWorkItem.Seq,
            };
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO workitem (id, name, description, score, isEnabled, seq) VALUES (@id, @name, @desc, @score, @isEnabled, @seq)", conn);
            cmd.Parameters.AddWithValue("@id", newItem.Id);
            cmd.Parameters.AddWithValue("@name", newItem.Name);
            cmd.Parameters.AddWithValue("@desc", newItem.Description);
            cmd.Parameters.AddWithValue("@score", newItem.Score);
            cmd.Parameters.AddWithValue("@isEnabled", newItem.IsEnabled);
            cmd.Parameters.AddWithValue("@seq", newItem.Seq);
            cmd.ExecuteNonQuery();
            LoadWorkItems();
            EditWorkItem = new WorkItem();
        }

        [RelayCommand]
        private void SaveWorkItem()
        {
            if (SelectedWorkItem == null) return;
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("UPDATE workitem SET name=@name, description=@desc, score=@score, isEnabled=@isEnabled, seq=@seq WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", SelectedWorkItem.Id);
            cmd.Parameters.AddWithValue("@name", EditWorkItem.Name);
            cmd.Parameters.AddWithValue("@desc", EditWorkItem.Description);
            cmd.Parameters.AddWithValue("@score", EditWorkItem.Score);
            cmd.Parameters.AddWithValue("@isEnabled", EditWorkItem.IsEnabled);
            cmd.Parameters.AddWithValue("@seq", EditWorkItem.Seq);
            cmd.ExecuteNonQuery();
            LoadWorkItems();
        }

        [RelayCommand]
        private void DeleteWorkItem()
        {
            if (SelectedWorkItem == null) return;
            if (MessageBox.Show("確定要刪除這個工作項目嗎？\n刪除後將無法恢復。", "確認刪除", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;

            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM workitem WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", SelectedWorkItem.Id);
            cmd.ExecuteNonQuery();
            LoadWorkItems();
            SelectedWorkItem = null;
            EditWorkItem = new WorkItem();
        }

        partial void OnSelectedWorkItemChanged(WorkItem? value)
        {
            if (value != null)
                EditWorkItem = new WorkItem
                {
                    Id = value.Id,
                    Name = value.Name,
                    Description = value.Description,
                    Score = value.Score,
                    IsEnabled = value.IsEnabled,
                    Seq = value.Seq,
                };
            else
                EditWorkItem = new WorkItem();
        }

        [RelayCommand]
        private void MoveWorkItemUp()
        {
            if (SelectedWorkItem == null) return;

            var selectedIndex = WorkItems.IndexOf(SelectedWorkItem);
            if (selectedIndex > 0)
            {
                var upperItem = WorkItems[selectedIndex - 1];
                SwapWorkItemSeq(SelectedWorkItem, upperItem);
            }
        }

        [RelayCommand]
        private void MoveWorkItemDown()
        {
            if (SelectedWorkItem == null) return;

            var selectedIndex = WorkItems.IndexOf(SelectedWorkItem);
            if (selectedIndex < WorkItems.Count - 1)
            {
                var lowerItem = WorkItems[selectedIndex + 1];
                SwapWorkItemSeq(SelectedWorkItem, lowerItem);
            }
        }

        private void SwapWorkItemSeq(WorkItem item1, WorkItem item2)
        {
            (item1.Seq, item2.Seq) = (item2.Seq, item1.Seq);

            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                var cmd1 = new MySqlCommand("UPDATE workitem SET seq=@seq WHERE id=@id", conn, transaction);
                cmd1.Parameters.AddWithValue("@id", item1.Id);
                cmd1.Parameters.AddWithValue("@seq", item1.Seq);
                cmd1.ExecuteNonQuery();

                var cmd2 = new MySqlCommand("UPDATE workitem SET seq=@seq WHERE id=@id", conn, transaction);
                cmd2.Parameters.AddWithValue("@id", item2.Id);
                cmd2.Parameters.AddWithValue("@seq", item2.Seq);
                cmd2.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                // Optionally handle the exception
            }
            var selectedId = SelectedWorkItem?.Id;
            LoadWorkItems();

            if (selectedId != null)
            {
                foreach (var item in WorkItems)
                {
                    if (item.Id == selectedId)
                    {
                        SelectedWorkItem = item;
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
