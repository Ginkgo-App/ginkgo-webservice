﻿using System.Collections.Generic;
using System.Linq;
using APICore.DBContext;
using APICore.Entities;
using APICore.Helpers;
using APICore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;

namespace APICore.Services
{
    public class ServiceService : IServiceService
    {
        private PostgreSQLContext _context;
        
        public ErrorList.ErrorCode TryGetServiceByTourId(int tourId, out List<APICore.Entities.TourService> tourServices)
        {
            ErrorList.ErrorCode errorCode;
            do
            {
                    try
                    {
                        ConnectDb();
                        tourServices = _context.TourServices.Where(s => s.TourId == tourId).ToList();

                        if (!tourServices.Any())
                        {
                            errorCode = ErrorList.ErrorCode.ServiceNotFound;
                            break;
                        }
                        errorCode = ErrorList.ErrorCode.Success;
                    }
                    finally
                    {
                        DisconnectDb();
                    }
            } while (false);

            return errorCode;
        }
        
        #region ConnectDB

        private void ConnectDb()
        {
            if (_context != null) return;
            _context = PostgreSQLContext.Instance;
        }

        private void DisconnectDb()
        {
            if (_context == null) return;
            _context.Dispose();
            _context = null;
        }

        #endregion
    }
}