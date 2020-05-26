using System.Collections.Generic;
using APICore.Entities;
using APICore.Helpers;

namespace APICore.Services.Interfaces
{
    public interface IServiceService
    {
        ErrorList.ErrorCode TryGetServiceByTourId(int tourId, out List<Service> tourServices);
    }
}