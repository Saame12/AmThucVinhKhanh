using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddDbContext<VinhKhanhFood.API.Data.AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<UserPresenceService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<AndroidAdbReverseHostedService>();
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.EnsureSchemaAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/scalar/v1");
    await Task.CompletedTask;
});

app.Run();
