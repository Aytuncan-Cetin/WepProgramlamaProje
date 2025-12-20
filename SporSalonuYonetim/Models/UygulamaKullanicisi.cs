using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class UygulamaKullanicisi : IdentityUser
    {
        [StringLength(50)]
        public string Ad { get; set; }

        [StringLength(50)]
        public string Soyad { get; set; }

        // Boy cm cinsinden
        public int? BoyCm { get; set; }

        // Kilo kg cinsinden
        public decimal? KiloKg { get; set; }

        [StringLength(100)]
        public string Hedef { get; set; } // kilo verme, kas geliştirme vb.

        public ICollection<Randevu> Randevular { get; set; }
        public ICollection<YapayZekaOneri> YapayZekaOnerileri { get; set; }
    }
}
