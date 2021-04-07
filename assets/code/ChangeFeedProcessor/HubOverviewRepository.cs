using System;
using Microsoft.Azure.Cosmos;

namespace ChangeFeedProcessor
{
    public class HubOverviewRepository : BaseRepository<HubOverview>
    {
        public HubOverviewRepository(Database database)
        {
            Container = database.CreateContainerIfNotExistsAsync("hub-monitoring", "/partitionKey").Result;
        }
    }
}