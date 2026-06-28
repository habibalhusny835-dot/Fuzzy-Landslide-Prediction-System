namespace SPL;

public partial class MainDashboardPage : ContentPage
{
    public MainDashboardPage()
    {
        InitializeComponent();

        PickerJenisTanah.Items.Clear();
        PickerJenisTanah.Items.Add("Tanah Lempung");
        PickerJenisTanah.Items.Add("Tanah Pasir");
        PickerJenisTanah.Items.Add("Tanah Lanau");
        PickerJenisTanah.Items.Add("Tanah Berbatu");

        TampilkanDashboard();
        TampilkanHistori();
        TampilkanLaporan();
    }

    private void HideAllViews()
    {
        DashboardView.IsVisible = false;
        PrediksiView.IsVisible = false;
        RiwayatView.IsVisible = false;
        NlpView.IsVisible = false;
        LaporanView.IsVisible = false;
        PengaturanView.IsVisible = false;
    }

    private void ResetMenuButton()
    {
        BtnDashboard.BackgroundColor = Colors.Transparent;
        BtnPrediksi.BackgroundColor = Colors.Transparent;
        BtnRiwayat.BackgroundColor = Colors.Transparent;
        BtnNlp.BackgroundColor = Colors.Transparent;
        BtnLaporan.BackgroundColor = Colors.Transparent;
        BtnPengaturan.BackgroundColor = Colors.Transparent;

        BtnDashboard.TextColor = Color.FromArgb("#CBD5E1");
        BtnPrediksi.TextColor = Color.FromArgb("#CBD5E1");
        BtnRiwayat.TextColor = Color.FromArgb("#CBD5E1");
        BtnNlp.TextColor = Color.FromArgb("#CBD5E1");
        BtnLaporan.TextColor = Color.FromArgb("#CBD5E1");
        BtnPengaturan.TextColor = Color.FromArgb("#CBD5E1");
    }

    private void SetActiveButton(Button button)
    {
        ResetMenuButton();
        button.BackgroundColor = Color.FromArgb("#0B5ED7");
        button.TextColor = Colors.White;
    }

    private void TampilkanDashboard()
    {
        HideAllViews();
        DashboardView.IsVisible = true;
        LabelJudulMenu.Text = "Dashboard Admin / Petugas";
        SetActiveButton(BtnDashboard);
    }

    private void OnDashboardMenuClicked(object sender, EventArgs e)
    {
        TampilkanDashboard();
    }

    private void OnPrediksiMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        PrediksiView.IsVisible = true;
        LabelJudulMenu.Text = "Menu Prediksi Fuzzy";
        SetActiveButton(BtnPrediksi);
    }

    private void OnRiwayatMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        RiwayatView.IsVisible = true;
        LabelJudulMenu.Text = "Riwayat Prediksi";
        SetActiveButton(BtnRiwayat);
        TampilkanHistori();
    }

    private void OnNlpMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        NlpView.IsVisible = true;
        LabelJudulMenu.Text = "AI Chatbot Gemini";
        SetActiveButton(BtnNlp);
    }

    private void OnLaporanMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        LaporanView.IsVisible = true;
        LabelJudulMenu.Text = "Laporan Masyarakat";
        SetActiveButton(BtnLaporan);
        TampilkanLaporan();
    }

    private void OnPengaturanMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        PengaturanView.IsVisible = true;
        LabelJudulMenu.Text = "Pengaturan Sistem";
        SetActiveButton(BtnPengaturan);
    }

    private async void OnPrediksiClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCurahHujan.Text) ||
            string.IsNullOrWhiteSpace(EntryKelembapan.Text) ||
            string.IsNullOrWhiteSpace(EntryKemiringan.Text) ||
            PickerJenisTanah.SelectedIndex == -1)
        {
            await DisplayAlert("Data belum lengkap", "Semua parameter wajib diisi.", "OK");
            return;
        }

        if (!double.TryParse(EntryCurahHujan.Text, out double hujan) ||
            !double.TryParse(EntryKelembapan.Text, out double lembap) ||
            !double.TryParse(EntryKemiringan.Text, out double miring))
        {
            await DisplayAlert("Format salah", "Input angka tidak valid.", "OK");
            return;
        }

        string tanah = PickerJenisTanah.SelectedItem?.ToString() ?? "";

        FuzzyResult fuzzy = FuzzyService.HitungRisiko(hujan, lembap, miring, tanah);

        LabelHujanCard.Text = $"{hujan} mm";
        LabelKelembapanCard.Text = $"{lembap}%";
        LabelKemiringanCard.Text = $"{miring}°";
        LabelHasil.Text = fuzzy.Risiko.ToUpper();
        LabelStatusBesar.Text = fuzzy.Risiko.ToUpper();
        LabelKeterangan.Text = fuzzy.Rekomendasi;

        LabelHasilPrediksiDetail.Text =
            $"Metode: Fuzzy Logic\n" +
            $"Curah Hujan: {hujan} mm\n" +
            $"Kelembapan: {lembap}%\n" +
            $"Kemiringan: {miring}°\n" +
            $"Jenis Tanah: {tanah}\n" +
            $"Skor Fuzzy: {fuzzy.Skor}\n" +
            $"Risiko: {fuzzy.Risiko}\n\n" +
            $"Rekomendasi: {fuzzy.Rekomendasi}";

        string data =
            $"[{DateTime.Now:dd-MM-yyyy HH:mm}]\n" +
            $"Metode: Fuzzy Logic\n" +
            $"Hujan: {hujan} mm | Kelembapan: {lembap}% | Kemiringan: {miring}°\n" +
            $"Jenis Tanah: {tanah}\n" +
            $"Skor Fuzzy: {fuzzy.Skor}\n" +
            $"Risiko: {fuzzy.Risiko}\n";

        AppDataStore.PrediksiTerbaru = data;
        AppDataStore.FuzzyTerbaru = data;
        AppDataStore.HistoriPrediksi.Insert(0, data);
        AppDataStore.RiwayatFuzzy.Insert(0, data);

        TampilkanHistori();

        await DisplayAlert("Prediksi Fuzzy Berhasil", $"Risiko: {fuzzy.Risiko}\nSkor: {fuzzy.Skor}", "OK");
    }

    private void TampilkanHistori()
    {
        LabelHistori.Text = AppDataStore.HistoriPrediksi.Count == 0
            ? "Belum ada histori prediksi."
            : string.Join("\n------------------\n", AppDataStore.HistoriPrediksi.Take(10));
    }

    private void TampilkanLaporan()
    {
        LabelLaporan.Text = AppDataStore.LaporanMasyarakat.Count == 0
            ? "Belum ada laporan masyarakat."
            : string.Join("\n------------------\n", AppDataStore.LaporanMasyarakat.Take(10));
    }

    private async void OnNlpClicked(object sender, EventArgs e)
    {
        string tanya = EditorNLP.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tanya))
        {
            LabelOutputNLP.Text = "Silakan masukkan pertanyaan terlebih dahulu.";
            return;
        }

        LabelOutputNLP.Text = "Gemini sedang memproses jawaban...";

        string konteks =
            $"Prediksi terbaru:\n{AppDataStore.PrediksiTerbaru}\n\n" +
            $"Fuzzy terbaru:\n{AppDataStore.FuzzyTerbaru}\n\n" +
            $"Laporan masyarakat:\n{string.Join("\n", AppDataStore.LaporanMasyarakat.Take(3))}";

        string jawaban = await GeminiService.TanyaGeminiAsync(tanya, konteks);

        LabelOutputNLP.Text = jawaban;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool keluar = await DisplayAlert("Logout", "Yakin ingin keluar?", "Ya", "Tidak");

        if (keluar)
            Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}