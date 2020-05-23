using APICore.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace APICore.Models
{
    public class ResponseModel
    {
        public ResponseModel()
        {
            ErrorCode = (int)ErrorList.ErrorCode.Default;
            Message = string.Empty;
            Data = null;
        }

        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public JArray Data { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> AdditionalProperties { get; set; } = new Dictionary<string, JToken>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        public object ToJson()
        {
            return JsonConvert.DeserializeObject(this.ToString());
        }

        public void FromErrorCode(ErrorList.ErrorCode errorCode)
        {
            ErrorCode = (int)errorCode;
            Message = ErrorList.Description(ErrorCode);
        }
        
        public void FromException(Exception ex)
        {
            Console.WriteLine(ex);
            ErrorCode = 501;
            Message = "An error has occurred";
            Data = new JArray
            {
                JObject.FromObject(new
                {
                    Error = ex.Message,
                    Stack = ex.StackTrace,
                    Source = ex.Source,
                    InnnerException = ex.InnerException
                })
            };
        }

        //public void AddData(JObject jObject)
        //{
        //    this.Merge(jObject, new JsonMergeSettings
        //    {
        //        // union array values together to avoid duplicates
        //        MergeArrayHandling = MergeArrayHandling.Merge
        //    });
        //}
    }
}
