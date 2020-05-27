using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public SimpleUser Friend { get; }
        public double Price { get; }

        public SimpleTour(int id, string name, DateTime startDay, DateTime endDay, int totalMember, SimpleUser host,
            SimpleUser friend, double price)
        {
            Id = id;
            Name = name;
            StartDay = startDay;
            EndDay = endDay;
            TotalMember = totalMember;
            Host = host;
            Friend = friend;
            Price = price;
        }
    }
}