namespace VinhKhanhFood.App
{
    public partial class App : Application
    {
        public static string CurrentLanguage { get; set; } = "vi";
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}