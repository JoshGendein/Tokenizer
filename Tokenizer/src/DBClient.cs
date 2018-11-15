using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Tokenizer.src.Models;
using System;

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
                                          "CREATE TABLE Tokens(Word varchar(255) not null, DocumentId varchar(255) not null, TF decimal(12,10), TFiDF decimal(12,10),  primary key (Word, DocumentId))" +
                                      "END " +
                                      "ELSE BEGIN " +
                                          "TRUNCATE TABLE dbo.Tokens END";

            var createTable3 = this.connection.CreateCommand();
            createTable3.CommandText = "IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'DF') " +
                                      "BEGIN " +
                                          "CREATE TABLE DF(Word varchar(255) not null, DocumentFrequency int,  primary key (Word))" +
                                      "END " +
                                      "ELSE BEGIN " +
                                          "TRUNCATE TABLE dbo.DF END";

            createTable1.ExecuteNonQuery();
            createTable2.ExecuteNonQuery();
            createTable3.ExecuteNonQuery();
        }


        public void BulkInsertTokens(List<TokenModel> tokens)
        {
            var tokenTable = GetTableStructure("Tokens");
            var positionTable = GetTableStructure("Positions");

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

        private DataTable GetTableStructure(string tableName)
        {
            var table = new DataTable();

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

        public void FillDF()
        {
            var fillDF = this.connection.CreateCommand();
            fillDF.CommandText = "INSERT INTO dbo.DF(Word, DocumentFrequency) SELECT Word, COUNT(Word) as DocumentFrequency from dbo.Tokens GROUP BY Word";
            fillDF.ExecuteNonQuery();
        }

        public void CalculateTFiDF(int TotalDocumentCount)
        {
            //var tfidf = this.connection.CreateCommand();
            //tfidf.CommandText = $"UPDATE Tokens SET Tokens.TFiDF = (Tokens.TF * LOG({TotalDocumentCount}/DF.DocumentFrequency)) FROM Tokens INNER JOIN DF ON Tokens.Word = DF.Word";
            //tfidf.ExecuteNonQuery();

            //1. Get all unique words. This is from the DF table.
            var DF = new DataTable();
            using (var adapter = new SqlDataAdapter($"SELECT * FROM dbo.DF", this.connection))
            {
                adapter.Fill(DF);
            };
            //2. For every uniqu word calculate it's TFiDF
            var tfidf = this.connection.CreateCommand();
            Console.WriteLine("Before loop: ");
            for(int i =0; i < DF.Rows.Count; i++)
            {
                var row = DF.Rows[i];
                var currentWord = row["Word"];
                var currentDF = row["DocumentFrequency"];
                tfidf.CommandText = 
                    $"UPDATE dbo.Tokens SET Tokens.TFiDF = (Tokens.TF * LOG({TotalDocumentCount}/{currentDF})) FROM Tokens WHERE Tokens.Word = '{currentWord}'";
                tfidf.ExecuteNonQuery();
                if(i % 1000 == 0)
                    Console.WriteLine($"{i} words completed.");
            }

        }
    }
}
