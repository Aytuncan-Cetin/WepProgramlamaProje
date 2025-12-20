using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuYonetim.Models
{
    public class YapayZekaOneriKaydi
    {
        public int Id { get; set; }

        [Required]
        public string UyeId { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Boy { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Kilo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Hedef { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? VucutTipi { get; set; }

        [Required]
        public string OneriMetni { get; set; } = string.Empty;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        // Opsiyonel: kullanıcıya navigation
        [ForeignKey(nameof(UyeId))]
        public UygulamaKullanicisi? Uye { get; set; }
    }
}
