using HealthCareChallenge.Models;
using HealthCareChallenge.Service.PatientService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPatientService, PatientService>();

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
        // get the client id from Andrew Sands (email : andrew.sands@visma.com or phone 96239993) and add it to developer secrets to run locally
        options.ClientId = builder.Configuration["VismaConnect:ClientId"];
        // get the client secret from Andrew Sands (email : andrew.sands@visma.com or phone 96239993) and add it to developer secrets to run locally
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
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
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

