
namespace Tokenizer.src
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Tokenizer.src.Models;

    public class DBClient
    {
        private static readonly string Endpoint = "https://localhost:8081";
        //This is the default key for all local Cosmos DB emulators so there is no security risk.
        private static readonly string Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private DocumentClient client;
        private static string DatabaseId;


        public DBClient()
        {
            client = new DocumentClient(new Uri(Endpoint), Key);
        }

        public async Task InitializeAsync(string dbID, params string[] Collections)
        {
            DatabaseId = dbID;
            await CreateDatabaseAsync();
            await CreateTFCollectinAsync("TF");
            await Task.WhenAll(Collections.Select(i => CreateCollectionAsync(i)));
        }

        public async Task<Document> InsertAsync(Token token, string collection)
        {
            TimeSpan sleepTime = TimeSpan.Zero;

            while(true)
            {
                try
                {
                    return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collection), token);

                }
                catch (DocumentClientException de)
                {
                    if ((int)de.StatusCode != 429)
                    {
                        throw;
                    }
                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        throw;
                    }

                    DocumentClientException de = (DocumentClientException)ae.InnerException;
                    if ((int)de.StatusCode != 429)
                    {
                        throw;
                    }
                    sleepTime = de.RetryAfter;
                }

                await Task.Delay(sleepTime);
            }
        }

        private async Task CreateDatabaseAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionAsync(string collection)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collection));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = collection });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateTFCollectinAsync(string collection)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collection));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {

                    DocumentCollection myCollection = new DocumentCollection
                    {
                        Id = collection
                    };
                    myCollection.PartitionKey.Paths.Add("/DocumentId");
                    myCollection.UniqueKeyPolicy = new UniqueKeyPolicy
                    {
                        UniqueKeys =
                        new Collection<UniqueKey>
                        {
                    new UniqueKey { Paths = new Collection<string> { "/Word" , "/DocumentId"}}
                  }
                    };

                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        myCollection);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
