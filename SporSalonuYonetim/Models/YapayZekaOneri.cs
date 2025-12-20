using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuYonetim.Models
{
    [NotMapped]   // EF Core: Bu sınıfı tablo yapma
    public class YapayZekaOneri
    {
        [Required]
        public string Boy { get; set; } = string.Empty;

        [Required]
        public string Kilo { get; set; } = string.Empty;

        [Required]
        public string Hedef { get; set; } = string.Empty;

        public string? VucutTipi { get; set; }
    }

    [NotMapped]
    public class YapayZekaSonuc
    {
        public string OneriMetni { get; set; } = string.Empty;
    }
}
