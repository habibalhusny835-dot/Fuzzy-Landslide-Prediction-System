namespace SPL;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        PickerRole.Items.Clear();
        PickerRole.Items.Add("Masyarakat");
        PickerRole.Items.Add("Admin");
        PickerRole.Items.Add("Petugas");
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = EntryUsername.Text?.Trim() ?? "";
        string password = EntryPassword.Text?.Trim() ?? "";
        string role = PickerRole.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(role))
        {
            await DisplayAlert("Login Gagal", "Username, password, dan hak akses wajib diisi.", "OK");
            return;
        }

        var user = await App.Database.LoginUserAsync(username, password, role);

        if (user == null)
        {
            await DisplayAlert("Login Gagal", "Akun belum terdaftar atau data login salah.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(user.FaceImagePath) || !File.Exists(user.FaceImagePath))
        {
            await DisplayAlert("Login Gagal", "Data wajah tidak ditemukan. Silakan register ulang.", "OK");
            return;
        }

        await Navigation.PushAsync(new FaceVerificationPage(username, role));
    }

    private async void OnGoToRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}