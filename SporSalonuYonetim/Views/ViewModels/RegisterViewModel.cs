using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [Display(Name = "Boy (cm)")]
        public int? BoyCm { get; set; }

        [Display(Name = "Kilo (kg)")]
        public decimal? KiloKg { get; set; }

        [StringLength(100)]
        [Display(Name = "Hedef (kilo verme, kas geliştirme vb.)")]
        public string Hedef { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifre ve şifre tekrarı uyuşmuyor.")]
        [Display(Name = "Şifre (Tekrar)")]
        public string ConfirmPassword { get; set; }
    }
}
