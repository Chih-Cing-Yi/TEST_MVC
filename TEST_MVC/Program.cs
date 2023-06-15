using Microsoft.EntityFrameworkCore;
using TEST_MVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DB連線(EF)
builder.Services.AddDbContext<WebAPIContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("2023MVC"));
});

//驗證cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    //以下設定依照需求添加
    options.Cookie.HttpOnly = true;//限制只能使用https存取cookie
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20); //與SlidingExpiration搭配使用
    options.SlidingExpiration = true;//每次移動滑鼠延長20分鐘
    options.AccessDeniedPath = "/Login/Error";//權限不足，跳轉到這個頁面
    options.LogoutPath = "/Login"; //登出，跳轉到登入頁面
    options.LoginPath = "/Login"; //未登入，跳轉到登入頁面
});

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
//驗證
app.UseAuthentication();
//授權
app.UseAuthorization();
//初始頁面
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();