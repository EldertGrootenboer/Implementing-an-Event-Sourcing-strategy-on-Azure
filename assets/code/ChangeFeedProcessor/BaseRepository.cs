using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ChangeFeedProcessor
{
    public class BaseRepository<T> where T : BaseOverview
    {
        protected Container Container;

        public async Task<T> GetDocument(string id)
        {
            try
            {
                var result = await Container.ReadItemAsync<T>(id, new PartitionKey(id));

                return result;
            }
            catch (CosmosException)
            {
                return null;
            }
        }

        public async Task CreateDocument(T overview)
        {
            var options = new ItemRequestOptions
            {
                EnableContentResponseOnWrite = false
            };

            var partitionKey = new PartitionKey(overview.id);

            await Container.CreateItemAsync(overview, partitionKey, options);
        }

        public async Task UpdateDocument(T overview)
        {
            var options = new ItemRequestOptions
            {
                EnableContentResponseOnWrite = false
            };

            var partitionKey = new PartitionKey(overview.id);

            await Container.ReplaceItemAsync(overview, overview.id, partitionKey, options);
        }
    }
}