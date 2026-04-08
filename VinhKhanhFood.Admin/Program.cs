var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký HttpClient để Admin có thể gọi sang API (Chỉ cần khai báo 1 lần)
builder.Services.AddHttpClient("MyAPI", client =>
{
    // Đảm bảo cổng 5020 này khớp với dự án VinhKhanhFood.API đang chạy
    client.BaseAddress = new Uri("http://localhost:5020/api/");
});

// 2. Thêm Session để lưu trạng thái đăng nhập
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// MapStaticAssets giúp load CSS/JS nhanh hơn ở bản .NET 9
app.MapStaticAssets();

app.UseRouting();

// 4. Kích hoạt Session (Bắt buộc phải nằm giữa UseRouting và UseAuthorization)
app.UseSession();

app.UseAuthorization();

// 5. Cấu hình Route mặc định là trang Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();