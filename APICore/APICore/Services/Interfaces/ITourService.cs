using APICore.Helpers;

namespace APICore.Services.Interfaces
{
    public interface ITourService
    {
        ErrorList.ErrorCode TryGetTotalMember(int tourId, out int totalMember);
    }
}