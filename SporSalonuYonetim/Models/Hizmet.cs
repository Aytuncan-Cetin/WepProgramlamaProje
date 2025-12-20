using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Ad { get; set; } // Örn: Fitness, Yoga, Pilates

        [Required]
        public int SureDakika { get; set; }

        [Required]
        public decimal Ucret { get; set; }

        [StringLength(50)]
        public string Kategori { get; set; } // İsteğe bağlı: "Grup", "Bireysel" vb.

        // Tek salon için bile ilişkili dursun
        public int SalonBilgiId { get; set; }
        public SalonBilgi Salon { get; set; }

        public ICollection<AntrenorHizmet> AntrenorHizmetler { get; set; }
        public ICollection<Randevu> Randevular { get; set; }
    }
}
