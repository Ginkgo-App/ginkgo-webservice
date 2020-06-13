using APICore.DBContext;
using Microsoft.Extensions.Options;
using NLog;

namespace APICore.Services
{
    public class PostService
    {
        private PostgreSQLContext _context;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        
        public PostService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        
        
    }
}