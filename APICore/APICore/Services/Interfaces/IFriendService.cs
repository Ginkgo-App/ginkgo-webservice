using APICore.Helpers;

namespace APICore.Services.Interfaces
{
    public interface IFriendService
    {
        ErrorList.ErrorCode CountTotalFriend(int userId, out int total);
        ErrorList.ErrorCode TryAddFriend(int userId, int userRequestId);
        ErrorList.ErrorCode TryAcceptFriend(int userId, int userRequestId);
        ErrorList.ErrorCode TryRemoveFriend(int userId, int userRequestId);
    }
}