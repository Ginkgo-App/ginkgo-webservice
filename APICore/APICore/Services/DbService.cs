using APICore.DBContext;
using Microsoft.EntityFrameworkCore;

namespace APICore.Services
{
    public class DbService
    {
        public static void ConnectDb(ref PostgreSQLContext context)
        {
            if (context != null) return;
            // context = PostgreSQLContext.Instance;
            context = new PostgreSQLContext(new DbContextOptions<PostgreSQLContext>());
        }

        public static void DisconnectDb(ref PostgreSQLContext context)
        {
            if (context != null)
            {
                context.Dispose();
            }
            context = null;
        }
    }
}