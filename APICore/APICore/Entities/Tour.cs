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
        public Tour(TourInfo tourInfo,List<TimeLine> timelines, string? name, DateTime startDay, DateTime endDay, int maxMember, int createBy, int tourInfoId, int totalDay, int totalNight, string[] services,  float price = 0)
        {
            Name = name;
            StartDay = startDay;
            EndDay = endDay;
            MaxMember = maxMember;
            CreateBy = createBy;
            TourInfoId = tourInfoId;
            TotalDay = totalDay;
            TotalNight = totalNight;
            Services = services;
            TimeLines = timelines;
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
        public int TotalDay { get; private set; }
        public int TotalNight { get; private set; }
        public int MaxMember { get; private set; }
        public int TourInfoId { get; private set; }
        public float Price { get; private set; }
        
        public string[] Services { get; private set; }
        
        [NotMapped]
        public TourInfo TourInfo { get; set; }
        [NotMapped]
        public List<TimeLine> TimeLines { get; set; }
        

        public JObject ToSimpleJson(User host, string friendType, int totalMember, TourInfo tourInfo)
        {
            JObject result = JObject.FromObject(this);

            result.Remove("TourInfoId");
            
            result.Add("TotalMember", totalMember);
            result.Add("Host", host?.ToSimpleJson(friendType));
            result.Add("Images", tourInfo?.Images != null ? JArray.FromObject(tourInfo.Images) : new JArray());

            return result;
        }

        public void Update(string name, in DateTime? startDay, in DateTime? endDay, in int? maxMember, in int? totalDay, in int? totalNight, in string[]? services, in List<TimeLine>? timelines, in float? price)
        {
            Name = name ?? Name;
            StartDay = startDay ?? StartDay;
            EndDay = endDay ?? EndDay;
            MaxMember = maxMember ?? MaxMember;
            TotalDay = totalDay ?? TotalDay;
            TotalNight = totalNight ?? TotalNight;
            Services = services ?? Services;
            TimeLines = timelines ?? TimeLines;
            Price = price ?? Price;
        }

        public DateTime? DeletedAt { get; set; }
        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}