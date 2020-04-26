using APICore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WebMvcPluginUser.DBContext;
using WebMvcPluginUser.Entities;
using static APICore.Helpers.ErrorList;

namespace WebMvcPluginUser.Services
{
    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.LOGGER;
        private PostgreSQLContext _context;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public ErrorCode Authenticate(string username, string password, out User user)
        {
            ErrorCode statusCode = ErrorCode.Default;
            user = null;

            do
            {
                bool isGetUserSucess = TryGetUsers(username, out user);

                // Cannot get user
                if (!isGetUserSucess)
                {
                    _logger.Error("Server internal error!");
                    statusCode = ErrorCode.CannotConnectToDatabase;
                    break;
                }

                // return null if user not found
                if (user == null)
                {
                    _logger.Error($"User {username} not found");
                    statusCode = ErrorCode.Fail;
                    break;
                }

                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                Console.WriteLine("Key: " + _appSettings.Secret);
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);
                statusCode = ErrorCode.Success;
            } while (false);

            return statusCode;
        }

        public bool TryGetUsers(out List<User> users)
        {
            users = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                users = (from u
                         in _context.Users
                         select u)
                        .ToListAsync()
                        .Result;
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryGetUsers(string username, out User user)
        {
            user = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                user =
                    (from u
                     in _context.Users
                     where (u.Username == username)
                     select u)
                    .FirstOrDefaultAsync()
                    .Result; // Lấy  Product có  ID  chỉ  ra
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryGetUsers(int userId, out User user)
        {
            user = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                user =
                    (from u
                     in _context.Users
                     where (u.Id == userId)
                     select u)
                    .FirstOrDefaultAsync()
                    .Result; // Lấy  Product có  ID  chỉ  ra
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryAddUser(User user)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();
                _context.Users.Update(user);
                _context.SaveChanges();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        #region ConnectDB
        private void ConnectDB()
        {
            if (_context == null)
            {
                DbContextOptions<PostgreSQLContext> options = new DbContextOptions<PostgreSQLContext>();
                _context = new PostgreSQLContext(options);
            }
        }

        private void DisconnectDB()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
        #endregion
    }
}
