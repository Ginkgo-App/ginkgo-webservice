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
                if (user == null || (user.Password != null && !user.Password.Equals(password)))
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
            user = null;

            do
            {
                TryGetAuthProvider(authProvider.Id, out AuthProvider dbAuth);

                // Chua dang ky
                if (dbAuth == null)
                {
                    var inputEmail = authProvider.Email ?? email;

                    if (!inputEmail.IsExistAndNotEmpty())
                    {
                        statusCode = ErrorCode.AuthProviderMissingEmail;
                        break;
                    }
                    else
                    {
                        // Kiem tra email da su dung hay chua
                        if (!TryGetUsers(inputEmail, out user))
                        {
                            statusCode = Register(authProvider.Name, authProvider.Email ?? email, null, null, out user);
                            if (statusCode != ErrorCode.Success)
                            {
                                break;
                            }
                        }

                        if (!TryAddAuthProvider(authProvider, user))
                        {
                            TryRemoveUser(user.Id);
                            break;
                        }
                    }
                }
                // Da dang ky
                else
                {
                    // Dang loi cho nay, cai dbAuth ko lay ve cai user
                    TryGetUsers(dbAuth.UserId, out user);
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
                    Password = password,
                    Role = RoleType.User
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

        public bool TryGetUsers(int page, int pageSize, out List<User> users)
        {
            users = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                var usersDb = (from u
                         in _context.Users
                               select u)
                        .ToListAsync()
                        .Result;

                var total = usersDb.Select(p => p.Id).Count();
                var skip = pageSize * (page - 1);

                var canPage = skip < total;

                if (canPage)
                {
                    users = usersDb.Select(u => u)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToList();

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

        public bool TryUpdateUser(User user)
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

        public bool TryRemoveUser(int userId)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
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
                    dbAuthProvider.UserId = user.Id;
                    _context.AuthProviders.Update(dbAuthProvider);
                }
                else
                {
                    authProvider.UserId = user.Id;
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

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        break;
                    }

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

        public bool TryGetAuthProvider(string id, out AuthProvider authProvider)
        {
            authProvider = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                authProvider = _context.AuthProviders.Where(a => a.Id == id).Single();

                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryGetTours(int userId, out List<TourInfo> tourInfos)
        {
            tourInfos = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                tourInfos = _context.TourInfos.Where(a => a.CreateById == userId).ToList<TourInfo>();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryGetTourInfoById(int tourId, out TourInfo tourInfos)
        {
            tourInfos = null;
            bool isSuccess = false;

            try
            {
                ConnectDB();
                tourInfos = _context.TourInfos.Where(a => a.Id == tourId).FirstOrDefault();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

            return isSuccess;
        }

        public bool TryUpdateTourInfo(TourInfo tourInfo)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();
                _context.TourInfos.Update(tourInfo);
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

        public bool TryRemoveTourInfo(int tourInfoId)
        {
            bool isSuccess = false;
            try
            {
                ConnectDB();
                var tourInfo = _context.TourInfos.FirstOrDefault(u => u.Id == tourInfoId);
                if (tourInfo != null)
                {
                    _context.TourInfos.Remove(tourInfo);
                    _context.SaveChanges();
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

        public bool TryGetFriends(int userId, out List<User> friends)
        {
            friends = new List<User>();
            bool isSuccess = false;

            try
            {
                ConnectDB();
                var friendIds = _context.Friends.Where(a => a.UserId == userId || a.UserOtherId == userId).ToArray();
                foreach (var friendId in friendIds)
                {
                    TryGetUsers(friendId.UserId, out User user);
                    friends.Add(user);
                }

                friends.Distinct().ToList();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DisconnectDB();
            }

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
                    new Claim(ClaimTypes.Role, user.Role),
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
