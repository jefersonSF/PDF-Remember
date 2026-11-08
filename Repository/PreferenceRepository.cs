using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class PreferenceRepository
    {
        private const string _tableName = "Preferences";
        private const int _limitRecents = 10;
        public PreferenceRepository()
        {
            InicializarBanco();
        }
        private void InicializarBanco()
        {
            try
            {
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {_tableName} (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL UNIQUE,
                    Value TEXT NOT NULL
                );";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task Save(Preference preference)
        {
            try
            {
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                INSERT INTO {_tableName} (Name, Value)
                VALUES ($name, $value)
                ON CONFLICT(Name) DO UPDATE SET 
                    Value = $value
                ";

                cmd.Parameters.AddWithValue("$name", preference.Name.ToString());
                cmd.Parameters.AddWithValue("$value", preference.Value);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<List<Preference>> List()
        {
            try
            {
                List<Preference> listResult = new List<Preference>();
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT Id, Name, Value FROM {_tableName}";

                using var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read()) {
                    listResult.Add(new Preference()
                    {
                        Id = reader.GetInt32(0),
                        Name = Enum.Parse<Enums.Preferences>(reader.GetString(1)),
                        Value = reader.GetString(2)
                    });
                }

                return listResult;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
