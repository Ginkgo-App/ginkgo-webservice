#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace APICore.Entities
{
    public class Tour
    {
        public Tour(string? name, DateTime startDay, DateTime endDay, int maxMember, int tourInfoId)
        {
            Name = name;
            StartDay = startDay;
            EndDay = endDay;
            MaxMember = maxMember;
            TourInfoId = tourInfoId;
        }

        public int Id { get; private set; }
        public string? Name { get; private set; }
        public DateTime StartDay { get; private set; }
        public DateTime EndDay { get; private set; }
        public int MaxMember { get; private set; }
        public int TourInfoId { get; private set; }

        public JObject ToSimpleJson(User host, int isFriend, int totalMember, List<Service> services)
        {
            JObject result = JObject.FromObject(this);

            result.Add("TotalMember", totalMember);
            result.Add("Host", host?.ToSimpleJson(isFriend));
            result.Add("Services", services!=null? JArray.FromObject(services) : null);

            return result;
        }

        public void Update(string name, in DateTime? startDay, in DateTime? endDay, in int? maxMember)
        {
            Name = name ?? Name;
            StartDay = startDay ?? StartDay;
            EndDay = endDay ?? EndDay;
            MaxMember = maxMember ?? MaxMember;
        }
    }
}