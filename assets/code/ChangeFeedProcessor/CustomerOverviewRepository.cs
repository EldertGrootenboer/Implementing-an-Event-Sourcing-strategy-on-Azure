using System;
using Microsoft.Azure.Cosmos;

namespace ChangeFeedProcessor
{
    public class CustomerOverviewRepository : BaseRepository<CustomerOverview>
    {
        public CustomerOverviewRepository(Database database)
        {
            Container = database.CreateContainerIfNotExistsAsync("customers-overview","/customerId").Result.Container;
        }
    }
}