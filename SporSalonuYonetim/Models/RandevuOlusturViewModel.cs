using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class RandevuOlusturViewModel
    {
        [Required(ErrorMessage = "Antrenör seçiniz.")]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Hizmet seçiniz.")]
        public int HizmetId { get; set; }

        [Required(ErrorMessage = "Tarih seçiniz.")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Saat giriniz.")]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Saat formatı HH:mm olmalıdır.")]
        public string Saat { get; set; } = string.Empty;

        // Drop-down listeler için
        public List<Antrenor> Antrenorler { get; set; } = new();
        public List<Hizmet> Hizmetler { get; set; } = new();
    }
}
