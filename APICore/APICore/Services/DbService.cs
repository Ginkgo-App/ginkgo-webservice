using APICore.DBContext;

namespace APICore.Services
{
    public class DbService
    {
        public static void ConnectDb(ref PostgreSQLContext context)
        {
            if (context != null) return;
            context = PostgreSQLContext.Instance;
        }

        public static void DisconnectDb(ref PostgreSQLContext context)
        {
            // if (context != null)
            // {
            //     context.Dispose();
            // }
            // context = null;
        }
    }
}