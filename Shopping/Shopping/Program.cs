using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<AdventureWorksLT2016Context>(options =>
    options.UseSqlServer("Server=.;Database=AdventureWorksLT2016;Trusted_Connection=True;MultipleActiveResultSets=true"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
//=====================================================
// (1) 請先安裝 Nuget套件 -- Microsoft.AspNetCore.Authentication.Cookies
// (2) 自己宣告 Microsoft.AspNetCore.Authentication.Cookies; 命名空間
// (3) 使用這兩者 .AddAuthentication() 和 .AddCookie() 方法來建立驗證中介軟體服務   
builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                // 以下這兩個設定可有可無
                options.AccessDeniedPath = "/LoginDB/AccessDeny";   // 拒絕，不允許登入，會跳到這一頁。
                options.LoginPath = "/LoginDB/Login";     // 登入頁。

                options.Cookie.Name = "MIS2000Lab";
                options.Cookie.HttpOnly = true;
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
