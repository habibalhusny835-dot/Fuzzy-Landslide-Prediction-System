namespace SPL;

public partial class MasyarakatDashboardPage : ContentPage
{
    public MasyarakatDashboardPage()
    {
        InitializeComponent();
        TampilkanHome();
        TampilkanData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TampilkanData();
    }

    private void HideAllViews()
    {
        HomeView.IsVisible = false;
        PrediksiView.IsVisible = false;
        HistoriView.IsVisible = false;
        LaporanView.IsVisible = false;
        NlpView.IsVisible = false;
    }

    private void ResetButton()
    {
        BtnHome.BackgroundColor = Colors.Transparent;
        BtnPrediksi.BackgroundColor = Colors.Transparent;
        BtnHistori.BackgroundColor = Colors.Transparent;
        BtnLaporan.BackgroundColor = Colors.Transparent;
        BtnNlp.BackgroundColor = Colors.Transparent;

        BtnHome.TextColor = Color.FromArgb("#CBD5E1");
        BtnPrediksi.TextColor = Color.FromArgb("#CBD5E1");
        BtnHistori.TextColor = Color.FromArgb("#CBD5E1");
        BtnLaporan.TextColor = Color.FromArgb("#CBD5E1");
        BtnNlp.TextColor = Color.FromArgb("#CBD5E1");
    }

    private void SetActive(Button button)
    {
        ResetButton();
        button.BackgroundColor = Color.FromArgb("#0B5ED7");
        button.TextColor = Colors.White;
    }

    private void TampilkanHome()
    {
        HideAllViews();
        HomeView.IsVisible = true;
        LabelJudulMenu.Text = "Beranda Masyarakat";
        SetActive(BtnHome);
    }

    private void OnHomeMenuClicked(object sender, EventArgs e)
    {
        TampilkanHome();
    }

    private void OnPrediksiMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        PrediksiView.IsVisible = true;
        LabelJudulMenu.Text = "Prediksi Fuzzy Terbaru";
        SetActive(BtnPrediksi);
        TampilkanData();
    }

    private void OnHistoriMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        HistoriView.IsVisible = true;
        LabelJudulMenu.Text = "Histori Prediksi";
        SetActive(BtnHistori);
        TampilkanData();
    }

    private void OnLaporanMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        LaporanView.IsVisible = true;
        LabelJudulMenu.Text = "Laporan Kondisi Sekitar";
        SetActive(BtnLaporan);
    }

    private void OnNlpMenuClicked(object sender, EventArgs e)
    {
        HideAllViews();
        NlpView.IsVisible = true;
        LabelJudulMenu.Text = "AI Chatbot Gemini";
        SetActive(BtnNlp);
    }

    private void TampilkanData()
    {
        string prediksi = string.IsNullOrWhiteSpace(AppDataStore.PrediksiTerbaru)
            ? "Belum ada prediksi terbaru dari admin."
            : AppDataStore.PrediksiTerbaru;

        LabelPrediksiTerbaru.Text = prediksi;
        LabelPrediksiDetail.Text = prediksi;

        LabelHistori.Text = AppDataStore.HistoriPrediksi.Count == 0
            ? "Belum ada histori prediksi."
            : string.Join("\n------------------\n", AppDataStore.HistoriPrediksi.Take(10));
    }

    private async void OnKirimLaporanClicked(object sender, EventArgs e)
    {
        string lokasi = EntryLokasi.Text?.Trim() ?? "";
        string laporanText = EditorLaporan.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(lokasi) || string.IsNullOrWhiteSpace(laporanText))
        {
            await DisplayAlert("Gagal", "Lokasi dan isi laporan wajib diisi.", "OK");
            return;
        }

        string laporan =
            $"[{DateTime.Now:dd-MM-yyyy HH:mm}]\n" +
            $"Lokasi: {lokasi}\n" +
            $"Laporan: {laporanText}\n" +
            $"Status: Baru\n";

        AppDataStore.LaporanMasyarakat.Insert(0, laporan);

        EntryLokasi.Text = "";
        EditorLaporan.Text = "";

        await DisplayAlert("Berhasil", "Laporan berhasil dikirim ke admin.", "OK");
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
            $"Prediksi terbaru dari admin:\n{AppDataStore.PrediksiTerbaru}\n\n" +
            $"Fuzzy terbaru:\n{AppDataStore.FuzzyTerbaru}\n\n" +
            $"Histori terbaru:\n{string.Join("\n", AppDataStore.HistoriPrediksi.Take(3))}";

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