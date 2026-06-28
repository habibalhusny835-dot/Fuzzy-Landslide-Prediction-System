namespace SPL;

public class FuzzyResult
{
    public string Risiko { get; set; } = "";
    public double Skor { get; set; }
    public string Rekomendasi { get; set; } = "";
}

public static class FuzzyService
{
    public static FuzzyResult HitungRisiko(double hujan, double lembap, double miring, string tanah)
    {
        double skorHujan = FuzzyHujan(hujan);
        double skorLembap = FuzzyKelembapan(lembap);
        double skorMiring = FuzzyKemiringan(miring);
        double skorTanah = FuzzyJenisTanah(tanah);

        double skorAkhir =
            (skorHujan * 0.30) +
            (skorLembap * 0.25) +
            (skorMiring * 0.30) +
            (skorTanah * 0.15);

        string risiko;
        string rekomendasi;

        if (skorAkhir >= 70)
        {
            risiko = "Tinggi";
            rekomendasi = "Risiko tinggi. Hindari area lereng, aktifkan peringatan, dan lakukan evakuasi bila diperlukan.";
        }
        else if (skorAkhir >= 40)
        {
            risiko = "Sedang";
            rekomendasi = "Risiko sedang. Pantau curah hujan, kelembapan tanah, dan kondisi lereng secara berkala.";
        }
        else
        {
            risiko = "Rendah";
            rekomendasi = "Risiko rendah. Kondisi relatif aman, namun monitoring tetap perlu dilakukan.";
        }

        return new FuzzyResult
        {
            Risiko = risiko,
            Skor = Math.Round(skorAkhir, 2),
            Rekomendasi = rekomendasi
        };
    }

    private static double FuzzyHujan(double hujan)
    {
        if (hujan <= 20) return 20;
        if (hujan <= 50) return 40;
        if (hujan <= 80) return 70;
        return 100;
    }

    private static double FuzzyKelembapan(double lembap)
    {
        if (lembap <= 30) return 20;
        if (lembap <= 60) return 50;
        if (lembap <= 80) return 75;
        return 100;
    }

    private static double FuzzyKemiringan(double miring)
    {
        if (miring <= 15) return 20;
        if (miring <= 30) return 55;
        if (miring <= 45) return 80;
        return 100;
    }

    private static double FuzzyJenisTanah(string tanah)
    {
        if (tanah == "Tanah Lempung") return 85;
        if (tanah == "Tanah Lanau") return 70;
        if (tanah == "Tanah Pasir") return 55;
        if (tanah == "Tanah Berbatu") return 30;
        return 50;
    }
}