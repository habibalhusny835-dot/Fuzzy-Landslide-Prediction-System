namespace SPL.Services
{
    public static class PredictionService
    {
        public static string PredictRisk(double curahHujan, double kemiringan, double kelembapan, string jenisTanah)
        {
            if (curahHujan >= 100 || kemiringan >= 45 || kelembapan >= 80)
                return "Tinggi";

            if (curahHujan >= 50 || kemiringan >= 25 || kelembapan >= 50)
                return "Sedang";

            return "Rendah";
        }
    }
}