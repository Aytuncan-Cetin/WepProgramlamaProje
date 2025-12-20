using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;
using SporSalonuYonetim.Services;


var builder = WebApplication.CreateBuilder(args);

// DbContext kaydý
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity + Rol sistemi
builder.Services.AddIdentity<UygulamaKullanicisi, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // ÞÝFRE KURALLARINI GEVÞET
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<GeminiAiService>();






// Kültürü Türkçe yap (300,00 gibi virgüllü sayýlar için)
var culture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var app = builder.Build();

// Uygulama start olurken seed çalýþtýr
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await VeriBaslangic.VerileriOlusturAsync(services);
}

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
