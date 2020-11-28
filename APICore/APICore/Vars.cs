using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace APICore
{
    public static class Vars
    {
        //public const string CONNECTION_STRING = "postgres://szdhists:Z6W3M_aMmyGIr0Ai7gBsdPGw5FzfjlEi@satao.db.elephantsql.com:5432/szdhists";
        //public const string CONNECTION_STRING = @"Server=satao.db.elephantsql.com;Port=5432;Database=szdhists;User Id=szdhists;Password=Z6W3M_aMmyGIr0Ai7gBsdPGw5FzfjlEi";
        public static string ConnectionString { get; set; }
        public static string PasswordSalt { get; set; }
        public static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("Nlog.config").GetCurrentClassLogger();
        public const int DefaultPageSize = 10;
        public const int DefaultPage = 1;
    }
}
