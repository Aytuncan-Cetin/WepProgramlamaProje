// Models/MusaitlikDtos.cs
using System;

namespace SporSalonuYonetim.Models
{
    public class MusaitlikEkleDto
    {
        public int AntrenorId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class MusaitlikSilDto
    {
        public int Id { get; set; }
    }
}
