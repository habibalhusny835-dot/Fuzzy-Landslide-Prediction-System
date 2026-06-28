using SPL.Services;

namespace SPL;

public partial class App : Application
{
    public static DatabaseService Database { get; private set; } = new DatabaseService();

    public App()
    {
        InitializeComponent();

        MainPage = new NavigationPage(new MainPage());
    }
}