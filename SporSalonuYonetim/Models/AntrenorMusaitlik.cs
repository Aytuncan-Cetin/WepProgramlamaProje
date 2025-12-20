using System;

namespace SporSalonuYonetim.Models
{
    public class AntrenorMusaitlik
    {
        public int Id { get; set; }

        public int AntrenorId { get; set; }
        public Antrenor? Antrenor { get; set; }

        // Pazartesi, Salı vb.
        public DayOfWeek Gun { get; set; }

        // Gün içi başlangıç ve bitiş saatleri
        public TimeSpan BaslangicSaati { get; set; }
        public TimeSpan BitisSaati { get; set; }
    }
}
