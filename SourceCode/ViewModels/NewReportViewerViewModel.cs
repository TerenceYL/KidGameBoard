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
    public partial class NewReportViewerViewModel : ObservableObject
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

        public NewReportViewerViewModel()
        {
            IsQueryButtonEnable = true;
        }

        [RelayCommand]
        private void Query()
        {
            PersonScores.Clear();

            using var conn = new MySqlConnection(_connStr);
            conn.Open();

            // Get all people
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

            // Get all workitem scores
            var workItemScores = new Dictionary<string, int>();
            var cmdWorkItems = new MySqlCommand("SELECT id, score FROM workitem", conn);
            using (var reader = cmdWorkItems.ExecuteReader())
            {
                while (reader.Read())
                {
                    workItemScores[reader.GetString("id")] = reader.GetInt32("score");
                }
            }

            // Calculate total earned scores
            var personEarnedScoreDict = new Dictionary<string, int>();
            var cmd = new MySqlCommand(
                "SELECT personId, workItemIds FROM dailyrecord WHERE date >= @start AND date <= @end", conn);
            cmd.Parameters.AddWithValue("@start", StartDate.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@end", EndDate.Date.ToString("yyyy-MM-dd"));

            using (var reader = cmd.ExecuteReader())
            {
                int workItemIdsIndex = reader.GetOrdinal("workItemIds");
                int personIdIndex = reader.GetOrdinal("personId");
                while (reader.Read())
                {
                    var personId = reader.GetString(personIdIndex);
                    var workItemIds = reader.IsDBNull(workItemIdsIndex) ? "" : reader.GetString(workItemIdsIndex);
                    int sum = 0;
                    if (!string.IsNullOrEmpty(workItemIds))
                    {
                        var ids = workItemIds.Trim('[', ']').Split(',');
                        foreach (var id in ids)
                        {
                            var cleanId = id.Trim('"', ' ');
                            if (workItemScores.TryGetValue(cleanId, out int score))
                                sum += score;
                        }
                    }
                    if (personEarnedScoreDict.ContainsKey(personId))
                        personEarnedScoreDict[personId] += sum;
                    else
                        personEarnedScoreDict[personId] = sum;
                }
            }

            // Calculate total redeemed scores
            var personRedeemedScoreDict = new Dictionary<string, int>();
            var cmdRedeemed = new MySqlCommand(
                "SELECT personId, SUM(score) as redeemedScore FROM redemption WHERE redemptionDate >= @start AND redemptionDate <= @end GROUP BY personId", conn);
            cmdRedeemed.Parameters.AddWithValue("@start", StartDate);
            cmdRedeemed.Parameters.AddWithValue("@end", EndDate);

            using (var reader = cmdRedeemed.ExecuteReader())
            {
                while (reader.Read())
                {
                    var personId = reader.GetString("personId");
                    var redeemedScore = reader.GetInt32("redeemedScore");
                    personRedeemedScoreDict[personId] = redeemedScore;
                }
            }

            // Combine results
            foreach (var person in people)
            {
                personEarnedScoreDict.TryGetValue(person.Id, out int earnedScore);
                personRedeemedScoreDict.TryGetValue(person.Id, out int redeemedScore);
                PersonScores.Add(new PersonScore
                {
                    Name = person.Name,
                    TotalScore = earnedScore,
                    RedeemedScore = redeemedScore
                });
            }
        }
    }
}
