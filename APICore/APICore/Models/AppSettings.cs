using System;
using System.Collections.Generic;
using System.Text;

namespace APICore
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string ConnectionString { get; set; }
        public string DevConnectionString { get; set; }
        public string PasswordSalt { get; set; }
    }
}
