using APICore.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void FromErrorCode(ErrorList.ErrorCode errorCode)
        {
            ErrorCode = (int)errorCode;
            Message = ErrorList.Description(ErrorCode);
            Data = null;
        }
    }
}
