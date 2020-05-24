#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace APICore.Entities
{
    public class Tour
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public int MaxMember { get; set; }
        public int TourInfoId { get; set; }

        public JObject ToSimpleJson(User host, int isFriend, int totalMember, List<Service> services)
        {
            JObject result = JObject.FromObject(this);

            result.Add("TotalMember", totalMember);
            result.Add("Host", host?.ToSimpleJson(isFriend));
            result.Add("Services", services!=null? JArray.FromObject(services) : null);

            return result;
        }
    }
}