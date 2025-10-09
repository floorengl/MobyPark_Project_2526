﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace MobyPark_api.Data.Models
{
    public class ParkingLot
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }

        public string Address { get; set; }

        public long Capacity { get; set; }

        public float? Tariff { get; set; }

        public float? DayTariff { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Point Coordinates { get; set; }
    }
}
