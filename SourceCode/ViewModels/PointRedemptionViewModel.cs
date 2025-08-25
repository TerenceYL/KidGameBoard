using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidGameBoard.Common;
using KidGameBoard.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KidGameBoard.ViewModels
{
    public partial class PointRedemptionViewModel : ObservableObject
    {
        private readonly string _connStr = AppConfigHelper.GetConnectionString("KidGameDatabase");

        [ObservableProperty]
        private ObservableCollection<Person> _people = new();

        [ObservableProperty]
        private Person? _selectedPerson;

        [ObservableProperty]
        private int _totalScore;

        [ObservableProperty]
        private int _redeemedScore;

        [ObservableProperty]
        private int _availableScore;

        [ObservableProperty]
        private int _scoreToRedeem;

        [ObservableProperty]
        private ObservableCollection<RedemptionRecord> _redemptionHistory = new();

        public PointRedemptionViewModel()
        {
            LoadPeople();
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
        }

        partial void OnSelectedPersonChanged(Person? value)
        {
            if (value != null)
            {
                LoadScores();
                LoadRedemptionHistory();
            }
        }

        private void LoadScores()
        {
            if (SelectedPerson == null) return;

            // Calculate total earned score
            TotalScore = 0;
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                    SELECT SUM(wi.score)
                    FROM dailyrecord dr
                    JOIN workitem wi ON JSON_CONTAINS(dr.workItemIds, CONCAT('""', wi.id, '""'))
                    WHERE dr.personId = @personId", conn);
                cmd.Parameters.AddWithValue("@personId", SelectedPerson.Id);
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    TotalScore = Convert.ToInt32(result);
                }
            }

            // Calculate total redeemed score
            RedeemedScore = 0;
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT SUM(score) FROM redemption WHERE personId = @personId", conn);
                cmd.Parameters.AddWithValue("@personId", SelectedPerson.Id);
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    RedeemedScore = Convert.ToInt32(result);
                }
            }

            AvailableScore = TotalScore - RedeemedScore;
        }

        private void LoadRedemptionHistory()
        {
            if (SelectedPerson == null) return;

            RedemptionHistory.Clear();
            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand(@"
                SELECT r.id, r.personId, p.name as personName, r.score, r.redemptionDate
                FROM redemption r
                JOIN person p ON r.personId = p.id
                WHERE r.personId = @personId
                ORDER BY r.redemptionDate DESC", conn);
            cmd.Parameters.AddWithValue("@personId", SelectedPerson.Id);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                RedemptionHistory.Add(new RedemptionRecord
                {
                    Id = reader.GetString("id"),
                    PersonId = reader.GetString("personId"),
                    PersonName = reader.GetString("personName"),
                    Score = reader.GetInt32("score"),
                    RedemptionDate = reader.GetDateTime("redemptionDate")
                });
            }
        }

        [RelayCommand]
        private void Redeem()
        {
            if (SelectedPerson == null || ScoreToRedeem <= 0) return;

            if (ScoreToRedeem > AvailableScore)
            {
                MessageBox.Show("兌換分數不可大於可用分數", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO redemption (id, personId, score, redemptionDate) VALUES (@id, @personId, @score, @redemptionDate)", conn);
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@personId", SelectedPerson.Id);
            cmd.Parameters.AddWithValue("@score", ScoreToRedeem);
            cmd.Parameters.AddWithValue("@redemptionDate", DateTime.Now);
            cmd.ExecuteNonQuery();

            ScoreToRedeem = 0;
            LoadScores();
            LoadRedemptionHistory();
        }
    }
}
