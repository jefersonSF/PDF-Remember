using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class RecentRepository
    {
        private const string _tableName = "Recents";
        private const int _limitRecents = 10;
        public RecentRepository()
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
                    FileFullPath TEXT NOT NULL UNIQUE,
                    DthRegister TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%d %H:%M:%f', 'NOW', 'localtime'))
                );";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task Save(Recents recent)
        {
            try
            {
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                INSERT INTO {_tableName} (FileFullPath)
                VALUES ($fileFullPath)
                ON CONFLICT(FileFullPath) DO UPDATE SET 
                    DthRegister = STRFTIME('%Y-%m-%d %H:%M:%f', 'NOW', 'localtime')
                ";

                cmd.Parameters.AddWithValue("$fileFullPath", recent.FileFullPath);

                await cmd.ExecuteNonQueryAsync();
                await ControlLimit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<List<Recents>> List()
        {
            try
            {
                List<Recents> listResult = new List<Recents>();
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT Id, FileFullPath, DthRegister FROM {_tableName} ORDER BY DthRegister DESC";

                using var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read()) {
                    listResult.Add(new Recents(reader.GetString(1))
                    {
                        Id = reader.GetInt32(0),
                        DthRegister = DateTime.Parse(reader.GetString(2))
                    });
                }

                return listResult;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private async Task ControlLimit() {
            int qtd = await GetQtdRecents();

            if (qtd > _limitRecents) {
                var listRecents = (await List()).OrderByDescending(x=>x.DthRegister).ToArray();
                for (int i = qtd; i > _limitRecents; i--) {
                    await Delete(listRecents[i - 1].Id);
                }

            }
        }

        public async Task Delete(int id) {
            try
            {
                ICollection<Recents> listResult = new List<Recents>();
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"DELETE FROM {_tableName} WHERE Id = $id;";
                cmd.Parameters.AddWithValue("$id", id);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }

        private async Task<int> GetQtdRecents() {
            try
            {
                ICollection<Recents> listResult = new List<Recents>();
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM {_tableName}";

                int qtdRecents = Convert.ToInt32(await cmd.ExecuteScalarAsync())!;

                return qtdRecents;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
