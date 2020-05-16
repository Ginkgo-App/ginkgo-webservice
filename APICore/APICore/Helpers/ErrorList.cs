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
            UserNotFound = 10,
            TourNotFound = 11,
            TourAlreadyExist = 12,
        }

        public static string Description(int errorCode)
        {
            ErrorCode code = (ErrorCode)errorCode;

            return code switch
            {
                ErrorCode.Default => "Default Value",
                ErrorCode.Success => "Success",
                ErrorCode.Fail => "Fail",
                ErrorCode.CannotConnectToDatabase => "Cannot connect to Database",
                ErrorCode.InvalidParameter => "Invalid parameters",
                ErrorCode.UsernamePasswordIncorrect => "Username or password is incorrect",
                ErrorCode.BodyInvalid => "Body is invalid",
                ErrorCode.FieldMissing => "Field is missing or incorrect",
                ErrorCode.UserAlreadyExits => "User already exist",
                ErrorCode.FeatureIsBeingImplemented => "Feature is being implemented",
                ErrorCode.AuthProviderMissingEmail => "Authenticate provider is lack off emails",
                ErrorCode.UserNotFound => "User not found",
                ErrorCode.TourNotFound => "Tour not found",
                ErrorCode.TourAlreadyExist => "Tour already exist",
                _ => "Error code is invalid",
            };
        }
    }

}
