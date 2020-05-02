using APICore.Models;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.Helpers
{
    public class CoreHelper
    {
        public static bool GetParameter(out JToken result, JObject body, string fieldName, JTokenType tokenType, ref ResponseModel response, bool isNullable = false, bool isIgnoreCase = true)
        {
            bool isSuccess = false;
            result = null;

            do
            {
                if (body == null)
                {
                    response.ErrorCode = (int)ErrorList.ErrorCode.BodyInvalid;
                    response.Message = ErrorList.Description(response.ErrorCode);
                    Vars.LOGGER.Debug("Body is null");
                    break;
                }

                var stringComparation = isIgnoreCase
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

                body.TryGetValue(fieldName, stringComparation, out result);

                if (isNullable)
                {
                    isSuccess = true;
                    break;
                }

                if (result == null || result.Type != tokenType)
                {
                    response.ErrorCode = (int)ErrorList.ErrorCode.InvalidParameter;
                    response.Message = ErrorList.Description(response.ErrorCode);
                    Vars.LOGGER.Debug("Field '" + fieldName + "' is missing or incorrect");
                    break;
                }

                isSuccess = true;
            } while (false);
            return isSuccess;
        }
    }
} 
