using APICore.DBContext;
using Microsoft.EntityFrameworkCore;

namespace APICore.Services
{
    public class DbService
    {
        public static void ConnectDb(out PostgreSQLContext context)
        {
            // if (context != null) return;
            // context = PostgreSQLContext.Instance;
            context = new PostgreSQLContext(new DbContextOptions<PostgreSQLContext>());
        }

        public static void DisconnectDb(out PostgreSQLContext context)
        {
            // if (context != null)
            // {
            //     context.Dispose();
            // }
            context = null;
        }
    }
}