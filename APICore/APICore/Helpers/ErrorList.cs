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

            FriendRequestAlreadySent = 13,
            AlreadyFriend = 14,
            FriendRequestNotFound = 15,
            FriendNotFound = 16,
            
            ServiceNotFound = 17,
        }

        public static string Description(int errorCode)
        {
            var code = (ErrorCode) errorCode;

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
                ErrorCode.FriendRequestAlreadySent => "Request have already sent.",
                ErrorCode.AlreadyFriend => "You both were friend already.",
                ErrorCode.FriendRequestNotFound => "Friend request not found.",
                ErrorCode.FriendNotFound => "Friend not found.",
                ErrorCode.ServiceNotFound => "Service not found",
                _ => "Error code is invalid"
            };
        }
    }
}