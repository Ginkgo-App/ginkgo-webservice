using APICore;
using APICore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using WebMvcPluginUser.DBContext;
using WebMvcPluginUser.Entities;
using WebMvcPluginUser.Helpers;
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

        public ErrorCode Authenticate(string email, string password, out User user)
        {
            ErrorCode statusCode = ErrorCode.Default;
            user = null;

            do
            {
                bool isGetUserSucess = TryGetUsers(email, out user);

                // Cannot get user
                if (!isGetUserSucess)
                {
                    _logger.Error("Server internal error!");
                    statusCode = ErrorCode.CannotConnectToDatabase;
                    break;
                }

                // return null if user not found
                if (user == null || !user.Password.Equals(password))
                {
                    _logger.Error($"Email: '{email}' or password is incorrect");
                    statusCode = ErrorCode.Fail;
                    break;
                }

                GenerateToken(user);
                statusCode = ErrorCode.Success;
            } while (false);

            return statusCode;
        }

        public ErrorCode Authenticate(string email, ref AuthProvider authProvider, out User user)
        {
            ErrorCode statusCode = ErrorCode.Default;

            do
            {
                TryGetUsers(authProvider.Email ?? email, out user);

                // return null if user not found
                if (user == null)
                {
                    statusCode = Register(authProvider.Name, authProvider.Email ?? email, null, null, out user);
                    if (statusCode != ErrorCode.Success)
                    {
                        break;
                    }
                }

                // Save AuthProvider

                if (!TryAddAuthProvider(authProvider, user))
                {
                    statusCode = ErrorCode.Fail;
                    break;
                }

                GenerateToken(user);
                statusCode = ErrorCode.Success;
            } while (false);

            return statusCode;
        }

        public ErrorCode Register(string name, string email, string phoneNumber, string password, out User user)
        {
            ErrorCode statusCode = ErrorCode.Default;
            user = null;

            do
            {
                if (TryGetUsers(email, out user))
                {
                    statusCode = ErrorCode.UserAlreadyExits;
                    break;
                }

                user = new User
                {
                    Name = name,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Password = password
                };

                if (!TryAddUser(user))
                {
                    break;
                };

                if (Authenticate(email, password, out user) != ErrorCode.Success)
                {
                    break;
                };


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

        public bool TryGetUsers(string email, out User user)
        {
            user = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                user =
                    (from u
                     in _context.Users
                     where (u.Email == email)
                     select u)
                    .FirstOrDefaultAsync()
                    .Result; // Lấy  Product có  ID  chỉ  ra

                if (user != null)
                {
                    isSuccess = true;
                }
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

        public bool TryAddAuthProvider(AuthProvider authProvider, User user)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();

                var dbAuthProvider = _context.AuthProviders.FirstOrDefault(a => a.Id == authProvider.Id);
                if (dbAuthProvider != null)
                {
                    dbAuthProvider.Name = authProvider.Name;
                    dbAuthProvider.Avatar = authProvider.Avatar;
                    dbAuthProvider.Email = authProvider.Email;
                    dbAuthProvider.Provider = authProvider.Provider;
                    dbAuthProvider.User = user;
                    _context.AuthProviders.Update(dbAuthProvider);
                }
                else
                {
                    authProvider.User = user;
                    _context.AuthProviders.Add(authProvider);
                }


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

        public bool TryGetFacbookInfo(string accessToken, out AuthProvider authProvider)
        {
            bool isSuccess = false;

            do
            {
                authProvider = null;
                string _postToPageURL = "https://graph.facebook.com/v2.12/me?fields=name,first_name,last_name,email&access_token=" + accessToken;
                IEnumerable<KeyValuePair<string, string>> postData = new Dictionary<string, string>();

                using (var http = new HttpClient())
                {
                    var httpResponse = http.PostAsync(
                        _postToPageURL,
                        new FormUrlEncodedContent(postData)).Result;
                    dynamic httpContent = JsonConvert.DeserializeObject(httpResponse.Content.ReadAsStringAsync().Result);

                    if ((int)httpResponse.StatusCode != 200 || httpContent == null)
                    {
                        break;
                    }


                    authProvider = new AuthProvider();
                    authProvider.Id = httpContent.id;
                    authProvider.Name = httpContent.name;
                    authProvider.Email = httpContent.email;
                    authProvider.Provider = ProviderType.facebook.ToString();

                    isSuccess = true;
                }
            } while (false);

            return isSuccess;
        }


        private void GenerateToken(User user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            Console.WriteLine("Key: " + _appSettings.Secret);
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
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
