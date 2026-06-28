using OpenCvSharp;
using CvRect = OpenCvSharp.Rect;
using CvSize = OpenCvSharp.Size;

namespace SPL;

public partial class FaceVerificationPage : ContentPage
{
    private readonly string username;
    private readonly string role;

    private VideoCapture? camera;
    private CascadeClassifier? faceCascade;
    private CancellationTokenSource? cameraToken;

    private Mat? registeredFace;
    private bool verified = false;
    private double lastAccuracy = 0;

    private const double MIN_FACE_ACCURACY = 80;

    public FaceVerificationPage(string username, string role)
    {
        InitializeComponent();

        this.username = username;
        this.role = role;

        LabelRole.Text = $"User: {username}\nRole: {role}";
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        await StartVerificationCameraAsync();
    }

    private async Task StartVerificationCameraAsync()
    {
        try
        {
            var user = await App.Database.GetUserForFaceAsync(username, role);

            if (user == null)
            {
                await DisplayAlert("Error", "Akun tidak ditemukan di SQLite.", "OK");
                return;
            }

            if (!File.Exists(user.FaceImagePath))
            {
                await DisplayAlert("Error", "Foto wajah register tidak ditemukan.", "OK");
                return;
            }

            registeredFace = Cv2.ImRead(user.FaceImagePath, ImreadModes.Grayscale);
            Cv2.Resize(registeredFace, registeredFace, new CvSize(160, 160));

            string cascadePath = await CopyCascadeToLocalAsync();
            faceCascade = new CascadeClassifier(cascadePath);

            if (faceCascade.Empty())
            {
                await DisplayAlert("Error", "Haar Cascade gagal dibaca.", "OK");
                return;
            }

            camera = new VideoCapture(0);

            if (!camera.IsOpened())
            {
                await DisplayAlert("Error", "Kamera laptop tidak terbuka.", "OK");
                return;
            }

            verified = false;
            lastAccuracy = 0;

            LabelCameraStatus.Text = "● Kamera Aktif";
            LabelCameraStatus.TextColor = Colors.LimeGreen;
            LabelStatusSaatIni.Text = "Kamera aktif. Arahkan wajah ke frame.";
            LabelDetectionStatus.Text = "Mendeteksi wajah OpenCV Haar Cascade...";

            cameraToken = new CancellationTokenSource();

            _ = Task.Run(() => VerificationLoop(cameraToken.Token));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void VerificationLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using Mat frame = new Mat();

            camera?.Read(frame);

            if (frame.Empty())
                continue;

            using Mat gray = new Mat();

            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);

            CvRect[] faces = faceCascade?.DetectMultiScale(
                gray,
                1.1,
                5,
                HaarDetectionTypes.ScaleImage,
                new CvSize(80, 80)
            ) ?? Array.Empty<CvRect>();

            double accuracy = 0;

            if (faces.Length > 0)
            {
                CvRect face = faces.OrderByDescending(f => f.Width * f.Height).First();

                Cv2.Rectangle(frame, face, Scalar.LimeGreen, 3);

                using Mat currentFace = new Mat(gray, face);
                Cv2.Resize(currentFace, currentFace, new CvSize(160, 160));

                accuracy = CompareFaces(registeredFace!, currentFace);
                lastAccuracy = accuracy;

                Cv2.PutText(
                    frame,
                    $"Accuracy: {accuracy:F1}%",
                    new OpenCvSharp.Point(20, 40),
                    HersheyFonts.HersheySimplex,
                    1,
                    Scalar.Yellow,
                    2
                );

                if (accuracy >= MIN_FACE_ACCURACY)
                {
                    verified = true;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LabelDetectionStatus.Text = "✅ Deteksi wajah berhasil";
                        LabelStatusVerifikasi.Text = "VERIFIED";
                        LabelStatusVerifikasi.TextColor = Colors.LimeGreen;
                        LabelStatusSaatIni.Text = "Wajah terverifikasi. Silakan masuk dashboard.";
                        LabelRiwayat.Text =
                            $"{DateTime.Now:dd-MM-yyyy HH:mm:ss}  Berhasil\n" +
                            LabelRiwayat.Text;
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LabelDetectionStatus.Text = "Wajah terdeteksi, akurasi belum memenuhi.";
                        LabelStatusVerifikasi.Text = "BELUM VALID";
                        LabelStatusVerifikasi.TextColor = Colors.Orange;
                        LabelStatusSaatIni.Text = $"Akurasi belum mencapai {MIN_FACE_ACCURACY}%.";
                    });
                }
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LabelDetectionStatus.Text = "Wajah belum terdeteksi.";
                    LabelStatusVerifikasi.Text = "NO FACE";
                    LabelStatusVerifikasi.TextColor = Colors.Red;
                    LabelStatusSaatIni.Text = "Pastikan wajah berada di depan kamera.";
                });
            }

            ImageSource source = MatToImageSource(frame);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                CameraPreview.Source = source;
                LabelAkurasi.Text = $"Akurasi: {accuracy:F1}%";
                LabelPersenBesar.Text = $"{accuracy:F0}%";
                ProgressAkurasi.Progress = accuracy / 100.0;
            });

            Thread.Sleep(50);
        }
    }

    private async void OnMasukDashboardClicked(object sender, EventArgs e)
    {
        if (!verified || lastAccuracy < MIN_FACE_ACCURACY)
        {
            await DisplayAlert("Akses Ditolak", $"Verifikasi wajah belum mencapai {MIN_FACE_ACCURACY}%.", "OK");
            return;
        }

        StopCamera();

        if (role == "Masyarakat")
            await Navigation.PushAsync(new MasyarakatDashboardPage());
        else
            await Navigation.PushAsync(new MainDashboardPage());
    }

    private static double CompareFaces(Mat face1, Mat face2)
    {
        using Mat hist1 = new Mat();
        using Mat hist2 = new Mat();

        int[] histSize = { 256 };
        Rangef[] ranges = { new Rangef(0, 256) };
        int[] channels = { 0 };

        Cv2.CalcHist(new[] { face1 }, channels, null, hist1, 1, histSize, ranges);
        Cv2.CalcHist(new[] { face2 }, channels, null, hist2, 1, histSize, ranges);

        Cv2.Normalize(hist1, hist1, 0, 1, NormTypes.MinMax);
        Cv2.Normalize(hist2, hist2, 0, 1, NormTypes.MinMax);

        double correlation = Cv2.CompareHist(hist1, hist2, HistCompMethods.Correl);

        double percent = ((correlation + 1) / 2) * 100;

        return Math.Clamp(percent, 0, 100);
    }

    private static ImageSource MatToImageSource(Mat mat)
    {
        Cv2.ImEncode(".jpg", mat, out byte[] data);
        return ImageSource.FromStream(() => new MemoryStream(data));
    }

    private async Task<string> CopyCascadeToLocalAsync()
    {
        string fileName = "haarcascade_frontalface_default.xml";
        string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        if (File.Exists(localPath))
            return localPath;

        using Stream stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using FileStream output = File.Create(localPath);

        await stream.CopyToAsync(output);

        return localPath;
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        StopCamera();
        await Navigation.PopToRootAsync();
    }

    private void StopCamera()
    {
        cameraToken?.Cancel();
        camera?.Release();
        camera?.Dispose();
        camera = null;

        LabelCameraStatus.Text = "Kamera berhenti";
        LabelCameraStatus.TextColor = Colors.Orange;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopCamera();
    }
}