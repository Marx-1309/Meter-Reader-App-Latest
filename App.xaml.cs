using SampleMauiMvvmApp.Services;

namespace SampleMauiMvvmApp;

public partial class App : Application
{
    public static UserInfo UserInfo;
    public static CustomerService CustomerService { get; private set; }
    public static ReadingService ReadingService { get; private set; }
    public static BaseService BaseService { get; private set; }

    public App(CustomerService _customerService, ReadingService _readingService, BaseService _baseService)
    {
        InitializeComponent();
        ThemeService.ApplySaved();   // ← only addition
        MainPage = new AppShell();
        CustomerService = _customerService;
        ReadingService = _readingService;
        BaseService = _baseService;
    }
}
