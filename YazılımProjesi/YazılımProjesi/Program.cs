using YazılımProjesi.Models;
using YazılımProjesi.Services;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Session with enhanced configuration
builder.Services.AddDistributedMemoryCache(); // Session verilerini saklamak için memory cache ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // Session süresini 24 saat yap
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".YazilimProjesi.Session";
});

// Add Application Services as Singletons for data persistence
builder.Services.AddSingleton<IItemService, ItemService>();
builder.Services.AddSingleton<ICartService, CartService>();
builder.Services.AddSingleton<IPurchaseHistoryService, PurchaseHistoryService>();
builder.Services.AddSingleton<IBalanceService, BalanceService>();

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

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
