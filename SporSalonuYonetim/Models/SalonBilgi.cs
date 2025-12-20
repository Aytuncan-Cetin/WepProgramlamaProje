using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class SalonBilgi
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Ad { get; set; }

        [StringLength(200)]
        public string Adres { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan AcilisSaati { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan KapanisSaati { get; set; }

        public ICollection<Hizmet> Hizmetler { get; set; }
        public ICollection<Antrenor> Antrenorler { get; set; }
    }
}
