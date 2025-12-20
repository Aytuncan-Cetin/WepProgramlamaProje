using System;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public string UyeId { get; set; } = null!;

        [Required]
        public int AntrenorId { get; set; }

        [Required]
        public int HizmetId { get; set; }

        [Required]
        public DateTime Baslangic { get; set; }

        [Required]
        public DateTime Bitis { get; set; }

        public bool Onaylandi { get; set; } = false;

        // Navigation propertiler
        public UygulamaKullanicisi Uye { get; set; } = null!;
        public Antrenor Antrenor { get; set; } = null!;
        public Hizmet Hizmet { get; set; } = null!;
    }
}
