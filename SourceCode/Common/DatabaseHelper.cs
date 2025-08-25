using MySql.Data.MySqlClient;

namespace KidGameBoard.Common
{
    public static class DatabaseHelper
    {
        public static void EnsureTablesExist()
        {
            var _connStr = AppConfigHelper.GetConnectionString("KidGameDatabase");
            using var conn = new MySqlConnection(_connStr);
            conn.Open();

            // person table
            var personTable = @"
                CREATE TABLE IF NOT EXISTS person (
                    id VARCHAR(36) PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    description VARCHAR(255)
                );";
            new MySqlCommand(personTable, conn).ExecuteNonQuery();

            // workitem table
            var workitemTable = @"
                CREATE TABLE IF NOT EXISTS workitem (
                    id VARCHAR(36) PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    description VARCHAR(255),
                    score INT NOT NULL,
                    isEnabled BOOLEAN NOT NULL,
                    seq INT NOT NULL
                );";
            new MySqlCommand(workitemTable, conn).ExecuteNonQuery();

            // dailyrecord table
            var dailyrecordTable = @"
                CREATE TABLE IF NOT EXISTS dailyrecord (
                    id VARCHAR(36) PRIMARY KEY,
                    date DATE NOT NULL,
                    personId VARCHAR(36) NOT NULL,
                    workItemIds TEXT NOT NULL,
                    FOREIGN KEY (personId) REFERENCES person(id)
                );";
            new MySqlCommand(dailyrecordTable, conn).ExecuteNonQuery();

            // redemption table
            var redemptionTable = @"
                CREATE TABLE IF NOT EXISTS redemption (
                    id VARCHAR(36) PRIMARY KEY,
                    personId VARCHAR(36) NOT NULL,
                    score INT NOT NULL,
                    redemptionDate DATETIME NOT NULL,
                    FOREIGN KEY (personId) REFERENCES person(id)
                );";
            new MySqlCommand(redemptionTable, conn).ExecuteNonQuery();
        }
    }
}
