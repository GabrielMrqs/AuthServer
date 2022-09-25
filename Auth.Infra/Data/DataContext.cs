using Microsoft.EntityFrameworkCore;

namespace Auth.Infra.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
    }
}