using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APICore.Entities;
using Newtonsoft.Json.Linq;

namespace APICore.Models
{
    public class SimpleTour
    {
        public int Id { get; }
        public string Name { get; }
        public DateTime StartDay { get; }
        public DateTime EndDay { get; }
        public int TotalMember { get; }
        public SimpleUser Host { get; }
        public List<SimpleUser> Friends { get; }
        public double Price { get; }

        public TourInfo TourInfo { get; }

        public SimpleTour(int id, string name, DateTime startDay, DateTime endDay, int totalMember, SimpleUser host,
            List<SimpleUser> friends, double price, TourInfo tourInfo)
        {
            Id = id;
            Name = name;
            StartDay = startDay;
            EndDay = endDay;
            TotalMember = totalMember;
            Host = host;
            Friends = friends;
            Price = price;
            TourInfo = tourInfo;
        }

        public JObject ToJson()
        {
            var json = JObject.FromObject(this);

            json.Remove("TourInfo");

            json.Add("Images", TourInfo?.Images != null ? JArray.FromObject(TourInfo.Images) : new JArray());

            return json;
        }
    }
}