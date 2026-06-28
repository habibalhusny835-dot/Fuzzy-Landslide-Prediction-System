using SQLite;

namespace SPL.Model
{
    public class PredictionHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public double CurahHujan { get; set; }
        public double Kemiringan { get; set; }
        public double Kelembapan { get; set; }

        public string JenisTanah { get; set; } = string.Empty;
        public string HasilPrediksi { get; set; } = string.Empty;
        public string Rekomendasi { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}