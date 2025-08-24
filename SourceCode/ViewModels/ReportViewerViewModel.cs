using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidGameBoard.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KidGameBoard.Common;

namespace KidGameBoard.ViewModels
{
    public partial class ReportViewerViewModel : ObservableObject
    {
        private readonly string _connStr = AppConfigHelper.GetConnectionString("KidGameDatabase");

        [ObservableProperty]
        private DateTime startDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime endDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<PersonScore> personScores = new();

        [ObservableProperty]
        private bool isQueryButtonEnable = false;

        public ReportViewerViewModel()
        {

            IsQueryButtonEnable = true;
        }

        [RelayCommand]
        private void Query()
        {
            PersonScores.Clear();

            using var conn = new MySqlConnection(_connStr);
            conn.Open();

            // 取得所有人員
            var people = new List<Person>();
            var cmdPeople = new MySqlCommand("SELECT id, name FROM person", conn);
            using (var reader = cmdPeople.ExecuteReader())
            {
                while (reader.Read())
                {
                    people.Add(new Person
                    {
                        Id = reader.GetString("id"),
                        Name = reader.GetString("name")
                    });
                }
            }

            // 取得所有工作項目分數
            var workItemScores = new Dictionary<string, int>();
            var cmdWorkItems = new MySqlCommand("SELECT id, score FROM workitem", conn);
            using (var reader = cmdWorkItems.ExecuteReader())
            {
                while (reader.Read())
                {
                    workItemScores[reader.GetString("id")] = reader.GetInt32("score");
                }
            }

            // 查詢區間內所有 dailyrecord
            var cmd = new MySqlCommand(
                "SELECT personId, workItemIds FROM dailyrecord WHERE date >= @start AND date <= @end", conn);
            cmd.Parameters.AddWithValue("@start", StartDate.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@end", EndDate.Date.ToString("yyyy-MM-dd"));

            var personScoreDict = new Dictionary<string, int>();
            using (var reader = cmd.ExecuteReader())
            {
                int workItemIdsIndex = reader.GetOrdinal("workItemIds");
                int personIdIndex = reader.GetOrdinal("personId");
                while (reader.Read())
                {
                    var personId = reader.GetString(personIdIndex);
                    var workItemIds = reader.IsDBNull(workItemIdsIndex) ? "" : reader.GetString(workItemIdsIndex);
                    int sum = 0;
                    foreach (var id in workItemIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (workItemScores.TryGetValue(id, out int score))
                            sum += score;
                    }
                    if (personScoreDict.ContainsKey(personId))
                        personScoreDict[personId] += sum;
                    else
                        personScoreDict[personId] = sum;
                }
            }

            // 組合結果
            foreach (var person in people)
            {
                personScoreDict.TryGetValue(person.Id, out int score);
                PersonScores.Add(new PersonScore { Name = person.Name, TotalScore = score });
            }
        }
    }

    public class PersonScore
    {
        public string Name { get; set; } = "";
        public int TotalScore { get; set; }
    }
}