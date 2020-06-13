using APICore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace APICore.Helpers
{
    public class CoreHelper
    {
        public static bool GetParameter(out JToken result, JObject body, string fieldName, JTokenType tokenType, ref ResponseModel response, bool isNullable = false, bool isIgnoreCase = true)
        {
            var isSuccess = false;
            result = null;

            do
            {
                if (body == null)
                {
                    response.ErrorCode = (int)ErrorList.ErrorCode.BodyInvalid;
                    response.Message = ErrorList.Description(response.ErrorCode);
                    Vars.Logger.Debug("Body is null");
                    break;
                }

                var stringComparision = isIgnoreCase
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

                body.TryGetValue(fieldName, stringComparision, out result);

                if (isNullable)
                {
                    isSuccess = true;
                    break;
                }

                if (result == null || result.Type != tokenType)
                {
                    response.ErrorCode = (int)ErrorList.ErrorCode.InvalidParameter;
                    response.Message = ErrorList.Description(response.ErrorCode);
                    response.Data = new JArray
                    {
                       new JObject{{"Field", fieldName}}
                    };
                    Vars.Logger.Debug("Field '" + fieldName + "' is missing or incorrect");
                    break;
                }

                isSuccess = true;
            } while (false);
            return isSuccess;
        }

        public bool ConvertFormToJson(ref IFormCollection form, out JObject json)
        {
            json = new JObject();

            do
            {
                foreach (var key in form.Keys)
                {
                    form.TryGetValue(key, out StringValues value);
                    json.Add(key, value.ToString());
                }
            } while (false);

            return true;
        }
        
        public static string ValidateEmail(string email)
        {
            if (!Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            {
                throw new ExceptionWithMessage("'" + email + "' is invalid format");
            }
            return email;
        }
        
        public static string ValidatePhoneNumber(string phoneNumber)
        {
            if (!Regex.IsMatch(phoneNumber, @"^-*[0-9,\.?\-?\(?\)?\ ]+$"))
            {
                throw new ExceptionWithMessage("'" + phoneNumber + "' is invalid format");
            }
            return phoneNumber;
        }

        public static void ValidatePageSize(ref int page, ref int pageSize)
        {
            page = page < 0 ? Vars.DefaultPage : page;
            pageSize = pageSize < 0 ? Vars.DefaultPageSize : pageSize;
        }
        
        public static string HashPassword(string password)
        {
            var salt = Encoding.ASCII.GetBytes(Vars.PasswordSalt);

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }
        
        public static int GetUserId(HttpContext requestContext, ref  ResponseModel responseModel)
        {
            var identity = requestContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                responseModel.FromErrorCode(ErrorList.ErrorCode.Fail);
            }

            var claims = identity.Claims;
            int.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                out var userId);
            return userId;
        }
    }
} 
