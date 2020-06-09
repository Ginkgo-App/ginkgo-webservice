#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using APICore.Models;

namespace APICore.Entities
{
    public class Tour : IIsDeleted
    {
        public Tour(TourInfo tourInfo, string? name, DateTime startDay, DateTime endDay, int maxMember, int createBy, int tourInfoId, int price = 0)
        {
            Name = name;
            StartDay = startDay;
            EndDay = endDay;
            MaxMember = maxMember;
            CreateBy = createBy;
            TourInfoId = tourInfoId;
            Price = price;
            DeletedAt = null;
        }

        public Tour()
        {
        }

        public int Id { get; private set; }
        public int CreateBy { get; private set; }
        public string? Name { get; private set; }
        public DateTime StartDay { get; private set; }
        public DateTime EndDay { get; private set; }
        public int MaxMember { get; private set; }
        public int TourInfoId { get; private set; }
        public int Price { get; private set; }
        
        [NotMapped]
        public TourInfo TourInfo { get; private set; }
        

        public JObject ToSimpleJson(User host, string friendType, int totalMember, List<Service> services, TourInfo tourInfo)
        {
            JObject result = JObject.FromObject(this);

            result.Add("TotalMember", totalMember);
            result.Add("Host", host?.ToSimpleJson(friendType));
            result.Add("Services", services!=null? JArray.FromObject(services) : null);
            result.Add("Images", tourInfo?.Images != null ? JArray.FromObject(tourInfo.Images) : new JArray());

            return result;
        }

        public void Update(string name, in DateTime? startDay, in DateTime? endDay, in int? maxMember)
        {
            Name = name ?? Name;
            StartDay = startDay ?? StartDay;
            EndDay = endDay ?? EndDay;
            MaxMember = maxMember ?? MaxMember;
        }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}