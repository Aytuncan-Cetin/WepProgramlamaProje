using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class FitnessAiRequestViewModel
    {
        [Required]
        public string Gender { get; set; } = "Erkek";

        [Required]
        [Range(10, 90)]
        public int Age { get; set; }

        [Required]
        [Range(100, 250)]
        public int HeightCm { get; set; }

        [Required]
        [Range(30, 300)]
        public double WeightKg { get; set; }

        [Required]
        public string Goal { get; set; } = "Kilo vermek";

        [Required]
        public string ActivityLevel { get; set; } = "Orta";

        public string? AdditionalInfo { get; set; }

        public string? ResultText { get; set; }
    }
}
