using APICore.Helpers;

namespace APICore.Services.Interfaces
{
    public interface IFriendService
    {
        int CalculateIsFriend(int userId, int userRequestId);
        ErrorList.ErrorCode CountTotalFriend(int userId, out int total);
        ErrorList.ErrorCode TryAcceptFriend(int userId, int userRequestId);
        ErrorList.ErrorCode TryAddFriend(int userId, int userRequestId);
        ErrorList.ErrorCode TryRemoveFriend(int userId, int userRequestId);
    }
}