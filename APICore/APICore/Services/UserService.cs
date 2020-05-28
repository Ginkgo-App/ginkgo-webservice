using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using static APICore.Helpers.ErrorList;

namespace APICore.Services
{
    public interface IUserService
    {
        ErrorCode Authenticate(string email, string password, out User user);
        ErrorCode Authenticate(string email, ref AuthProvider authProvider, out User user);
        ErrorCode Register(string name, string email, string phoneNumber, string password, out User user);
        bool TryGetUsers(int page, int pageSize, out List<User> users, out Pagination pagination);
        bool TryGetUsers(string email, out User user);
        bool TryGetUsers(int userId, out User user);
        bool TryAddUser(User user);
        bool TryUpdateUser(User user);
        bool TryRemoveUser(int userId);
        bool TryAddAuthProvider(AuthProvider authProvider, User user);
        bool TryGetFacebookInfo(string accessToken, out AuthProvider authProvider);

        bool TryGetTours(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination);

        bool TryGetTourInfoById(int tourId, out TourInfo tourInfos);
        bool TryUpdateTourInfo(TourInfo tourInfo);
        bool TryRemoveTourInfo(int tourInfoId);

        bool TryGetFriends(int userId, string type, int page, int pageSize, out List<User> friends,
            out Pagination pagination);
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private readonly Logger _logger = Vars.Logger;
        private PostgreSQLContext _context;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public ErrorCode Authenticate(string email, string password, out User user)
        {
            ErrorCode statusCode;
            user = null;

            do
            {
                var isGetUserSuccess = TryGetUsers(email, out user);

                // Cannot get user
                if (!isGetUserSuccess)
                {
                    _logger.Error("Server internal error!");
                    statusCode = ErrorCode.CannotConnectToDatabase;
                    break;
                }

                // Hash password
                // var hashPassword = CoreHelper.HashPassword(password);

                // return null if user not found
                if (user == null || (user.Password != null && !user.Password.Equals(CoreHelper.HashPassword(password))))
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
            var statusCode = ErrorCode.Default;
            user = null;

            do
            {
                // No register
                if (!TryGetAuthProvider(authProvider.Id, out var dbAuth) || dbAuth == null)
                {
                    var inputEmail = authProvider.Email ?? email;

                    if (string.IsNullOrWhiteSpace(inputEmail))
                    {
                        statusCode = ErrorCode.AuthProviderMissingEmail;
                        break;
                    }
                    else
                    {
                        // Check whether email is exist or not
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
            var statusCode = ErrorCode.Default;
            user = null;

            do
            {
                if (TryGetUsers(email, out user))
                {
                    statusCode = ErrorCode.UserAlreadyExits;
                    break;
                }

                user = new User
                (
                    name: name,
                    email: email,
                    phoneNumber: phoneNumber,
                    password: password,
                    role: RoleType.User
                );

                if (!TryAddUser(user))
                {
                    break;
                }

                if (Authenticate(email, password, out user) != ErrorCode.Success)
                {
                    break;
                }

                statusCode = ErrorCode.Success;
            } while (false);

            return statusCode;
        }

        public bool TryGetUsers(int page, int pageSize, out List<User> users, out Pagination pagination)
        {
            users = null;
            pagination = null;

            try
            {
                DbService.ConnectDb(out _context);
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
                }
                else
                {
                    users = new List<User>();
                }

                pagination = new Pagination(total, page, pageSize);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryGetUsers(string email, out User user)
        {
            user = null;
            var isSuccess = false;

            try
            {
                DbService.ConnectDb(out _context);
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
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }

        public bool TryGetUsers(int userId, out User user)
        {
            user = null;

            try
            {
                DbService.ConnectDb(out _context);
                user =
                    (from u
                            in _context.Users
                        where (u.Id == userId)
                        select u)
                    .FirstOrDefaultAsync()
                    .Result; // Lấy  Product có  ID  chỉ  ra
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryAddUser(User user)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryUpdateUser(User user)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryRemoveUser(int userId)
        {
            var isSuccess = false;
            try
            {
                DbService.ConnectDb(out _context);
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    isSuccess = true;
                }
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }

        public bool TryAddAuthProvider(AuthProvider authProvider, User user)
        {
            try
            {
                DbService.ConnectDb(out _context);

                var dbAuthProvider = _context.AuthProviders.FirstOrDefault(a => a.Id == authProvider.Id);
                if (dbAuthProvider != null)
                {
                    _context.AuthProviders.Add(authProvider);
                    _context.AuthProviders.Update(dbAuthProvider);
                }
                else
                {
                    authProvider.UserId = user.Id;
                    _context.AuthProviders.Add(authProvider);
                }

                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryGetFacebookInfo(string accessToken, out AuthProvider authProvider)
        {
            var isSuccess = false;

            do
            {
                authProvider = null;
                var postToPageUrl =
                    "https://graph.facebook.com/v2.12/me?fields=name,first_name,last_name,email&access_token=" +
                    accessToken;
                IEnumerable<KeyValuePair<string, string>> postData = new Dictionary<string, string>();

                using var http = new HttpClient();
                var httpResponse = http.PostAsync(
                    postToPageUrl,
                    new FormUrlEncodedContent(postData)).Result;

                if (!httpResponse.IsSuccessStatusCode)
                {
                    break;
                }

                var httpContent =
                    JsonConvert.DeserializeObject(httpResponse.Content.ReadAsStringAsync().Result);
                var jsonContext = httpContent != null ? JObject.FromObject(httpContent) : null;

                if ((int) httpResponse.StatusCode != 200 || jsonContext == null)
                {
                    break;
                }

                var type = ProviderType.facebook.ToString();
                var id = jsonContext.GetValue("id")?.ToString() ?? "none";
                var name = jsonContext.GetValue("name")?.ToString();
                var email = jsonContext.GetValue("email")?.ToString();
                
                authProvider = new AuthProvider(id, name, email, null, type);
                isSuccess = true;
            } while (false);

            return isSuccess;
        }

        private bool TryGetAuthProvider(string id, out AuthProvider authProvider)
        {
            authProvider = null;

            try
            {
                DbService.ConnectDb(out _context);
                authProvider = _context.AuthProviders.FirstOrDefault(a => a.Id == id);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryGetTours(int userId, int page, int pageSize, out List<SimpleTour> tours,
            out Pagination pagination)
        {
            tours = null;
            pagination = null;
            var isSuccess = false;

            try
            {
                DbService.ConnectDb(out _context);

                var toursDb = (from t in _context.Tours
                    join ti in _context.TourInfos on t.TourInfoId equals ti.Id
                    join host in _context.Users on ti.CreateById equals host.Id
                    where ti.CreateById == userId
                    select new
                    {
                        Id = t.Id,
                        Name = t.Name,
                        StartDay = t.StartDay,
                        EndDay = t.EndDay,
                        Price = 0,
                        Host = host
                    }).ToList();

                Console.WriteLine(toursDb.Count());

                var total = toursDb.Count();
                var skip = pageSize * (page - 1);

                tours = toursDb
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList().ConvertAll((e) =>
                    {
                        return new SimpleTour(e.Id, e.Name, e.StartDay, e.EndDay,
                            _context.TourMembers.Count(t => t.TourId == e.Id), e.Host.ToSimpleUser(FriendType.Accepted),
                            null,
                            0);
                    });

                pagination = new Pagination(total, page, pageSize);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }

        public bool TryGetTourInfoById(int tourId, out TourInfo tourInfos)
        {
            tourInfos = null;

            try
            {
                DbService.ConnectDb(out _context);
                tourInfos = _context.TourInfos.FirstOrDefault(a => a.Id == tourId);
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryUpdateTourInfo(TourInfo tourInfo)
        {
            try
            {
                DbService.ConnectDb(out _context);
                _context.TourInfos.Update(tourInfo);
                _context.SaveChanges();
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return true;
        }

        public bool TryRemoveTourInfo(int tourInfoId)
        {
            var isSuccess = false;
            try
            {
                DbService.ConnectDb(out _context);
                var tourInfo = _context.TourInfos.FirstOrDefault(u => u.Id == tourInfoId);
                if (tourInfo != null)
                {
                    _context.TourInfos.Remove(tourInfo);
                    _context.SaveChanges();
                    isSuccess = true;
                }
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }

        public bool TryGetFriends(int userId, string type, int page, int pageSize, out List<User> friends,
            out Pagination pagination)
        {
            friends = new List<User>();
            pagination = null;
            var isSuccess = false;

            try
            {
                DbService.ConnectDb(out _context);
                var friendDBs = type?.ToLower() switch
                {
                    "accepted" => _context.Friends.Where(a =>
                        a.IsAccepted && (a.UserId == userId || a.RequestedUserId == userId)).ToArray(),
                    "requesting" => _context.Friends.Where(a => a.IsAccepted == false && (a.UserId == userId))
                        .ToArray(),
                    "waiting" => _context.Friends.Where(a => a.IsAccepted == false && (a.RequestedUserId == userId))
                        .ToArray(),
                    _ => _context.Friends.Where(a => a.UserId == userId || a.RequestedUserId == userId).ToArray()
                };

                var total = friendDBs.Count();
                var skip = pageSize * (page - 1);
                var canPage = skip < total;

                if (canPage)
                {
                    var listFriends = friendDBs.Select(u => u)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();

                    foreach (var friend in listFriends)
                    {
                        var id = friend.UserId != userId ? friend.UserId : friend.RequestedUserId;
                        TryGetUsers(id, out var user);
                        friends.Add(user);
                    }

                    friends = friends.Distinct().ToList();
                }
                else
                {
                    friends = new List<User>();
                }

                pagination = new Pagination(total, page, pageSize);
                isSuccess = true;
            }
            finally
            {
                DbService.DisconnectDb(out _context);
            }

            return isSuccess;
        }

        private void GenerateToken(User user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
        }
    }
}