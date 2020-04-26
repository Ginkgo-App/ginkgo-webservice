namespace APICore
{
    public class Vars
    {
        //public const string CONNECTION_STRING = "postgres://szdhists:Z6W3M_aMmyGIr0Ai7gBsdPGw5FzfjlEi@satao.db.elephantsql.com:5432/szdhists";
        //public const string CONNECTION_STRING = @"Server=satao.db.elephantsql.com;Port=5432;Database=szdhists;User Id=szdhists;Password=Z6W3M_aMmyGIr0Ai7gBsdPGw5FzfjlEi";
        public static string CONNECTION_STRING { get; set; }
        public static readonly NLog.Logger LOGGER = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
    }
}
