using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Tokenizer.src.Models;

namespace Tokenizer.src
{
    public class DBClient
    {
        private static readonly string ConnectionString = "Server=.;Database=Tokenizer;Trusted_Connection=True;";
        private readonly SqlConnection connection;


        public DBClient(SqlConnection connection)
        {
            connection.ConnectionString = ConnectionString;
            this.connection = connection;
            connection.Open();
        }

        public void Initialize()
        {

            var createTable1 = this.connection.CreateCommand();
            createTable1.CommandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Positions') " +
                                      "BEGIN " +
                                          "CREATE TABLE Positions(Word varchar(255) not null, Position int not null, DocumentId varchar(255) not null, primary key (DocumentId, Position))" +
                                      "END " +
                                      "ELSE BEGIN " +
                                          "TRUNCATE TABLE dbo.Positions END";

            var createTable2 = this.connection.CreateCommand();
            createTable2.CommandText = "IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'Tokens') " +
                                      "BEGIN " +
                                          "CREATE TABLE Positions(Word varchar(255) not null, DocumentId varchar(255) not null, TF real, TFiDF real,  primary key (Word, DocumentId))" +
                                      "END " +
                                      "ELSE BEGIN " +
                                          "TRUNCATE TABLE dbo.Tokens END";

            createTable1.ExecuteNonQuery();
            createTable2.ExecuteNonQuery();
        }

        public void BulkInsertTokens(List<TokenModel> tokens)
        {
            var tokenTable = GetTableStructure(new DataTable(), "Tokens");
            var positionTable = GetTableStructure(new DataTable(), "Positions");

            foreach (var token in tokens)
            {
                var row = tokenTable.NewRow();
                row["Word"] = token.Word;
                row["DocumentId"] = token.DocumentId;
                row["TF"] = token.TF;
                row["TFiDF"] = 0;
                tokenTable.Rows.Add(row);
            }

            Insert(tokenTable, "Tokens");

            foreach (var token in tokens)
            {
                foreach(var position in token.Positions)
                {
                    var row = positionTable.NewRow();
                    row["Word"] = token.Word;
                    row["Position"] = position;
                    row["DocumentId"] = token.DocumentId;
                    positionTable.Rows.Add(row);
                }
            }

            Insert(positionTable, "Positions");
        }

        private DataTable GetTableStructure(DataTable table, string tableName)
        {
            using (var adapter = new SqlDataAdapter($"SELECT TOP 0 * FROM {tableName}", this.connection))
            {
                adapter.Fill(table);
            };

            return table;
        }

        private void Insert(DataTable table, string tableName)
        {
            using (var bulk = new SqlBulkCopy(this.connection))
            {
                bulk.DestinationTableName = tableName;
                bulk.WriteToServer(table);
            }
        }
    }
}
