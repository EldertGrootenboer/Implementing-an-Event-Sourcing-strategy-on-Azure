using Microsoft.Azure.Cosmos;

namespace Demo.Common
{
    public class WarehouseRepository : BaseRepository<WarehouseProjection>
    {
        public WarehouseRepository(Database database)
        {
            Container = database.CreateContainerIfNotExistsAsync("hub-monitoring", "/partitionKey").Result;
        }
    }
}