using OpenCvSharp;
using SPL.Model;
using CvRect = OpenCvSharp.Rect;
using CvSize = OpenCvSharp.Size;

namespace SPL;

public partial class RegisterPage : ContentPage
{
    private VideoCapture? camera;
    private CascadeClassifier? faceCascade;
    private CancellationTokenSource? cameraToken;

    private Mat? savedFace;
    private bool faceReady = false;

    private const double MIN_FACE_ACCURACY = 80;

    public RegisterPage()
    {
        InitializeComponent();

        PickerRole.Items.Clear();
        PickerRole.Items.Add("Masyarakat");
        PickerRole.Items.Add("Admin");
        PickerRole.Items.Add("Petugas");
    }

    private async void OnCaptureFaceClicked(object sender, EventArgs e)
    {
        await StartCameraAsync();
    }

    private async Task StartCameraAsync()
    {
        try
        {
            string cascadePath = await CopyCascadeToLocalAsync();

            faceCascade = new CascadeClassifier(cascadePath);

            if (faceCascade.Empty())
            {
                await DisplayAlert("Error", "File Haar Cascade gagal dibaca.", "OK");
                return;
            }

            camera = new VideoCapture(0);

            if (!camera.IsOpened())
            {
                await DisplayAlert("Error", "Kamera laptop tidak bisa dibuka.", "OK");
                return;
            }

            faceReady = false;
            cameraToken = new CancellationTokenSource();

            LabelFaceStatus.Text = "Kamera aktif, arahkan wajah...";
            LabelFaceStatus.TextColor = Colors.DeepSkyBlue;

            _ = Task.Run(() => CameraLoop(cameraToken.Token));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Kamera", ex.Message, "OK");
        }
    }

    private void CameraLoop(CancellationToken token)
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

                accuracy = HitungAkurasiDeteksi(frame, face);

                Cv2.PutText(
                    frame,
                    $"Face: {accuracy:F0}%",
                    new OpenCvSharp.Point(20, 35),
                    HersheyFonts.HersheySimplex,
                    1,
                    Scalar.Yellow,
                    2
                );

                if (accuracy >= MIN_FACE_ACCURACY && !faceReady)
                {
                    using Mat faceOnly = new Mat(gray, face);

                    Cv2.Resize(faceOnly, faceOnly, new CvSize(160, 160));

                    savedFace?.Dispose();
                    savedFace = faceOnly.Clone();
                    faceReady = true;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LabelFaceStatus.Text = $"Wajah siap disimpan, akurasi >= {MIN_FACE_ACCURACY}%";
                        LabelFaceStatus.TextColor = Colors.LimeGreen;
                    });
                }
                else if (!faceReady)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LabelFaceStatus.Text = $"Mendeteksi wajah... {accuracy:F0}%";
                        LabelFaceStatus.TextColor = Colors.Orange;
                    });
                }
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!faceReady)
                    {
                        LabelFaceStatus.Text = "Wajah belum terdeteksi";
                        LabelFaceStatus.TextColor = Colors.Red;
                    }
                });
            }

            ImageSource source = MatToImageSource(frame);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ImageFacePreview.Source = source;
            });

            Thread.Sleep(50);
        }
    }

    private double HitungAkurasiDeteksi(Mat frame, CvRect face)
    {
        double frameArea = frame.Width * frame.Height;
        double faceArea = face.Width * face.Height;

        double ukuranScore = Math.Min((faceArea / frameArea) * 500, 100);

        double centerX = face.X + face.Width / 2.0;
        double centerY = face.Y + face.Height / 2.0;

        double jarakX = Math.Abs(centerX - frame.Width / 2.0);
        double jarakY = Math.Abs(centerY - frame.Height / 2.0);

        double centerScore = 100 - ((jarakX + jarakY) / (frame.Width + frame.Height) * 200);
        centerScore = Math.Clamp(centerScore, 0, 100);

        double accuracy = (ukuranScore * 0.5) + (centerScore * 0.5);

        return Math.Clamp(accuracy, 0, 100);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string username = EntryUsername.Text?.Trim() ?? "";
        string password = EntryPassword.Text?.Trim() ?? "";
        string role = PickerRole.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(role))
        {
            await DisplayAlert("Register Gagal", "Username, password, dan hak akses wajib diisi.", "OK");
            return;
        }

        if (!faceReady || savedFace == null)
        {
            await DisplayAlert("Register Gagal", $"Wajah belum mencapai akurasi minimal {MIN_FACE_ACCURACY}%.", "OK");
            return;
        }

        var existingUser = await App.Database.GetUserByUsernameAsync(username);

        if (existingUser != null)
        {
            await DisplayAlert("Register Gagal", "Username sudah terdaftar.", "OK");
            return;
        }

        string faceImagePath = Path.Combine(FileSystem.AppDataDirectory, $"{username}_face.png");

        Cv2.ImWrite(faceImagePath, savedFace);

        var user = new UserAccount
        {
            Username = username,
            Password = password,
            Role = role,    
            FaceImagePath = faceImagePath,
        };

        await App.Database.RegisterUserAsync(user);

        StopCamera();

        await DisplayAlert("Register Berhasil", "Akun dan wajah berhasil disimpan ke SQLite.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        StopCamera();
        await Navigation.PopAsync();
    }

    private void StopCamera()
    {
        cameraToken?.Cancel();
        camera?.Release();
        camera?.Dispose();
        camera = null;
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopCamera();
    }
}