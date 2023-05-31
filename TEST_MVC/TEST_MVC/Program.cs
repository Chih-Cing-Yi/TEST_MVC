using Microsoft.EntityFrameworkCore;
using TEST_MVC.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DB�s�u
builder.Services.AddDbContext<WebAPIContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("2023MVC"));
});

//����cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    //�H�U�]�w�̷ӻݨD�K�[
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20); //�PSlidingExpiration�f�t�ϥ�
    options.SlidingExpiration = true;//�C�����ʷƹ�����20����
    options.AccessDeniedPath = "/Login/Error";//�v�������A�����o�ӭ���
    options.LogoutPath = "/Login"; //�n�J���ѡA�����n�J����
    options.LoginPath = "/Login"; //���n�J�A�����n�J����
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
//����
app.UseAuthentication();
//���v
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
