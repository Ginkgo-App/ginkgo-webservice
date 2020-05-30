using APICore.Helpers;

namespace APICore.Services.Interfaces
{
    public interface IFriendService
    {
        string CalculateIsFriend(int userId, int userRequestId);
        ErrorList.ErrorCode CountTotalFriendAsync(int userId, out int total);
        ErrorList.ErrorCode TryAcceptFriend(int userId, int userRequestedId);
        ErrorList.ErrorCode TryAddFriend(int userId, int userRequestId);
        ErrorList.ErrorCode TryRemoveFriend(int userId, int userRequestId);
    }
}