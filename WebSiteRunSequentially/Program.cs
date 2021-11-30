using Microsoft.EntityFrameworkCore;
using RunMethodsSequentially;
using WebSiteRunSequentially.Database;
using WebSiteRunSequentially.StartupServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");
var lockFolder = builder.Environment.WebRootPath;

builder.Services.AddDbContext<WebSiteDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.RegisterRunMethodsSequentially(options =>
    {
        options.AddSqlServerLockAndRunMethods(connectionString);
        options.AddFileSystemLockAndRunMethods(lockFolder);
    })
    .RegisterServiceToRunInJob<StartupServiceEnsureCreated>()
    .RegisterServiceToRunInJob<StartupServiceSeedDatabase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
