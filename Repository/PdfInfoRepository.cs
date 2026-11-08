using Microsoft.Data.Sqlite;
using Models;

namespace Repository
{
    public class PdfInfoRepository
    {
        private const string _tableName = "PdfInfo";

        public PdfInfoRepository()
        {            
            InicializarBanco();
        }

        private void InicializarBanco()
        {
            try
            {
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"
            CREATE TABLE IF NOT EXISTS {_tableName} (
                Id INTEGER PRIMARY KEY,
                FileHash TEXT NOT NULL UNIQUE,
                FileName TEXT NOT NULL,
                FilePath TEXT NOT NULL,
                NumPage INTEGER NOT NULL,
                DthRegister TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%d %H:%M:%f', 'NOW', 'localtime'))
            );";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<PdfInfo?> FindPdfInfoByFullPath(string FileHash)
        {
            //return new PdfInfo { Id = 100, FileName = "teste.pdf", NumPage = 100 };
            using var connection = new SqliteConnection(Connection.ConnectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT Id, FileHash, FileName, FilePath, NumPage, DthRegister FROM {_tableName} WHERE FileHash = $fileHash;";
            cmd.Parameters.AddWithValue("$fileHash", FileHash);

            using var reader = await cmd.ExecuteReaderAsync();
            if (reader.Read()) {
                return new PdfInfo
                {
                    Id = reader.GetInt32(0),
                    FileHash = reader.GetString(1),
                    FileName = reader.GetString(2),
                    FilePath = reader.GetString(3),
                    NumPage = reader.GetInt32(4),
                    DthRegister = DateTime.Parse(reader.GetString(5))
                };
            }
            return null;
        }

        public async Task Save(PdfInfo info)
        {
            try
            {
                using var connection = new SqliteConnection(Connection.ConnectionString);
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                INSERT INTO {_tableName} (FileHash, FileName, FilePath, NumPage)
                VALUES ($fileHash, $fileName, $filePath, $numPage)
                ON CONFLICT(FileHash) DO UPDATE SET 
                    NumPage = $numPage,
                    DthRegister = STRFTIME('%Y-%m-%d %H:%M:%f', 'NOW', 'localtime'),
                    FilePath = $filePath,
                    FileName = $fileName
                RETURNING Id;
                ";

                cmd.Parameters.AddWithValue("$fileHash", info.FileHash);
                cmd.Parameters.AddWithValue("$fileName", info.FileName);
                cmd.Parameters.AddWithValue("$filePath", info.FilePath);
                cmd.Parameters.AddWithValue("$numPage", info.NumPage);
                info.Id = (long)(await cmd.ExecuteScalarAsync())!;                
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task Delete(long id)
        {
            using var connection = new SqliteConnection(Connection.ConnectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"DELETE FROM {_tableName} WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
