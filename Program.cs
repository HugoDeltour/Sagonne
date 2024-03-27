using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Sagonne.AuthorizationRequirements;
using WebMarkupMin.AspNetCore6;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebMarkupMin()
    .AddHtmlMinification()
    .AddXmlMinification()
    .AddHttpCompression()
    ;

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IAuthorizationHandler, AdminUserHandler>();

builder.Services.
    AddAuthorization(
    options =>
    {
        options.AddPolicy("IsAdmin", policy => policy.Requirements.Add(new AdminUserRequirement()));
    });



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Account/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
