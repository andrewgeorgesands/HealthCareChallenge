using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using System.Net;
using HealthCareChallenge.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Using In-Memory DB for easy setup/portability in a coding challenge
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("HealthcareTestDb"));


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["VismaConnect:Authority"];
        options.ClientId = builder.Configuration["VismaConnect:ClientId"];
        options.ClientSecret = builder.Configuration["VismaConnect:ClientSecret"];

        if (string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.Authority) || string.IsNullOrEmpty(options.ClientSecret))
        {
             var debugView = builder.Configuration.GetDebugView();
             throw new Exception($"Missing VismaConnect configuration. ClientId: '{options.ClientId}', Authority: '{options.Authority}', Secret: '{(string.IsNullOrEmpty(options.ClientSecret) ? "missing" : "present")}'. Configuration Debug View: {debugView}");
        }

        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        if (builder.Environment.IsDevelopment())
        {
            // temp code to allow proxying with Burp Suite.  Remember to remove this before committing.
            var proxyUrl = builder.Configuration["VismaConnect:ProxyUrl"];
            if (!string.IsNullOrEmpty(proxyUrl))
            {
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxyUrl),
                    UseProxy = true,
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            }
        }
    });

var app = builder.Build();
// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseExceptionHandler("/Home/Error");
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();