using Microsoft.Azure.Cosmos;

namespace Demo.Common
{
    public class CustomerRepository : BaseRepository<CustomerProjection>
    {
        public CustomerRepository(Database database)
        {
            Container = database.CreateContainerIfNotExistsAsync("customers-overview","/customerId").Result.Container;
        }
    }
}