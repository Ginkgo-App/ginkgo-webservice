﻿using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks.Dataflow;
using APICore.Models;
using Newtonsoft.Json.Linq;

#nullable enable
namespace APICore.Entities
{
    public class Place : IIsDeleted
    {
        public Place(int typeId, string? name, string[]? images, string? description, double? longitude,
            double? latitude, string? address)
        {
            TypeId = typeId;
            Name = name;
            Images = images;
            Description = description;
            Longitude = longitude;
            Latitude = latitude;
            Address = address;
        }

        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Address { get; private set; }
        public string[]? Images { get; private set; }
        public string? Description { get; private set; }
        public double? Longitude { get; private set; }
        public double? Latitude { get; private set; }
        public int TypeId { get; private set; }

        public Place Update(string? name, string[]? images, string? description, int? typeId, double? longitude,
            double? latitude, string? address)
        {
            Name = name ?? Name;
            Images = images ?? Images;
            Description = description ?? Description;
            TypeId = typeId ?? TypeId;
            Longitude = longitude ?? Longitude;
            Latitude = latitude ?? Latitude;
            Address = address ?? Address;
            return this;
        }

        public DateTime? DeletedAt { get; set; }

        public void Delete()
        {
            DeletedAt = DateTime.Now;
        }
    }
}