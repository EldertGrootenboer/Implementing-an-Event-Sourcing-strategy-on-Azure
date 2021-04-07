using System;
using Microsoft.Azure.Cosmos;

namespace ChangeFeedProcessor
{
    public class HubOverviewRepository : BaseRepository<HubOverview>
    {
        public HubOverviewRepository(Database database)
        {
            Container = database.CreateContainerIfNotExistsAsync(
                Environment.GetEnvironmentVariable("HubContainer"),
                Environment.GetEnvironmentVariable("HubPartitionKey")).Result;
        }
    }
}