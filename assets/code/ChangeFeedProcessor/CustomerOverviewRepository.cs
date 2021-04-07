using System;
using Microsoft.Azure.Cosmos;

namespace ChangeFeedProcessor
{
    public class CustomerOverviewRepository : BaseRepository<CustomerOverview>
    {
        public CustomerOverviewRepository(Database database)
        {
            Container = database.CreateContainerIfNotExistsAsync(
                Environment.GetEnvironmentVariable("CustomerContainer"),
                Environment.GetEnvironmentVariable("CustomerPartitionKey")).Result.Container;
        }
    }
}