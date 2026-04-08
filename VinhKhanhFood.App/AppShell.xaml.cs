namespace VinhKhanhFood.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Subscribe vào event LanguageChanged
            Services.LocalizationService.LanguageChanged += OnLanguageChanged;

            // Cập nhật tab titles khi app khởi động
            UpdateTabTitles();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe từ event khi app tắt
            Services.LocalizationService.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(object? sender, Services.LanguageChangedEventArgs e)
        {
            // Cập nhật tab titles khi ngôn ngữ thay đổi
            UpdateTabTitles();
        }

        private void UpdateTabTitles()
        {
            if (Items.Count >= 3)
            {
                Items[0].Title = Services.LocalizationService.GetString("Map");
                Items[1].Title = Services.LocalizationService.GetString("Explore");
                Items[2].Title = Services.LocalizationService.GetString("Settings");
            }
        }
    }
}
