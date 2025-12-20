using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [StringLength(200)]
        public string UzmanlikAlanlari { get; set; } = string.Empty; // kas geliştirme, kilo verme vb.

        [StringLength(500)]
        public string Ozgecmis { get; set; } = string.Empty;

        [StringLength(250)]
        public string FotoUrl { get; set; } = string.Empty;

        public int SalonBilgiId { get; set; }
        public SalonBilgi Salon { get; set; }

        public ICollection<AntrenorHizmet> AntrenorHizmetler { get; set; } = new List<AntrenorHizmet>();

        // HATALARIN ASIL NEDENİ: Bu property kesin böyle olmalı
        public ICollection<AntrenorMusaitlik> Musaitliklar { get; set; } = new List<AntrenorMusaitlik>();

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
