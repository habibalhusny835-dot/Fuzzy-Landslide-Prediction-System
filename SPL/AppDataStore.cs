using SPL.Model;
using System.Text.Json;

namespace SPL
{
    public static class AppDataStore
    {
        public static List<UserAccount> Users { get; set; } = new();

        public static string PrediksiTerbaru { get; set; } = "Belum ada prediksi.";
        public static string FuzzyTerbaru { get; set; } = "Belum ada hasil fuzzy.";

        public static List<string> HistoriPrediksi { get; set; } = new();
        public static List<string> RiwayatFuzzy { get; set; } = new();
        public static List<string> LaporanMasyarakat { get; set; } = new();

        private static readonly string FilePath =
            Path.Combine(FileSystem.AppDataDirectory, "appdatastore.json");

        public static void SaveData()
        {
            var data = new StorageModel
            {
                Users = Users,
                PrediksiTerbaru = PrediksiTerbaru,
                FuzzyTerbaru = FuzzyTerbaru,
                HistoriPrediksi = HistoriPrediksi,
                RiwayatFuzzy = RiwayatFuzzy,
                LaporanMasyarakat = LaporanMasyarakat
            };

            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(FilePath, json);
        }

        public static void LoadData()
        {
            if (!File.Exists(FilePath))
                return;

            var json = File.ReadAllText(FilePath);
            var data = JsonSerializer.Deserialize<StorageModel>(json);

            if (data == null)
                return;

            Users = data.Users ?? new();
            PrediksiTerbaru = data.PrediksiTerbaru ?? "Belum ada prediksi.";
            FuzzyTerbaru = data.FuzzyTerbaru ?? "Belum ada hasil fuzzy.";
            HistoriPrediksi = data.HistoriPrediksi ?? new();
            RiwayatFuzzy = data.RiwayatFuzzy ?? new();
            LaporanMasyarakat = data.LaporanMasyarakat ?? new();
        }

        public static void SaveUsers()
        {
            SaveData();
        }

        public static void LoadUsers()
        {
            LoadData();
        }

        private class StorageModel
        {
            public List<UserAccount> Users { get; set; } = new();
            public string PrediksiTerbaru { get; set; } = string.Empty;
            public string FuzzyTerbaru { get; set; } = string.Empty;
            public List<string> HistoriPrediksi { get; set; } = new();
            public List<string> RiwayatFuzzy { get; set; } = new();
            public List<string> LaporanMasyarakat { get; set; } = new();
        }
    }
}