namespace APICore.Helpers
{
    public static class ErrorList
    {
        public enum ErrorCode
        {
            Default = -1,
            Success = 0,
            Fail = 1,
            CannotConnectToDatabase = 2,
            InvalidParameter = 3,
            UsernamePasswordIncorrect = 4,
            BodyInvalid = 5,
            FieldMissing = 6,
            UserAlreadyExits = 7,
            FeatureIsBeingImplemented = 8,
            AuthProviderMissingEmail = 9,
        }

        public static string Description(int errorCode)
        {
            ErrorCode code = (ErrorCode)errorCode;

            switch (code)
            {
                case ErrorCode.Default:
                    return "Default Value";
                case ErrorCode.Success:
                    return "Success";
                case ErrorCode.Fail:
                    return "Fail";
                case ErrorCode.CannotConnectToDatabase:
                    return "Cannot connect to Database";
                case ErrorCode.InvalidParameter:
                    return "Invalid parameters";
                case ErrorCode.UsernamePasswordIncorrect:
                    return "Username or password is incorrect";
                case ErrorCode.BodyInvalid:
                    return "Body is invalid";
                case ErrorCode.FieldMissing:
                    return "Field is missing or incorrect";
                case ErrorCode.UserAlreadyExits:
                    return "User already exist";
                case ErrorCode.FeatureIsBeingImplemented:
                    return "Feature is being implemented";
                case ErrorCode.AuthProviderMissingEmail:
                    return "Authenticate provider is lack off emails";
                default:
                    return "Error code is invalid";
            }
        }
    }

}
