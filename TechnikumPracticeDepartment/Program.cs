using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using System.Globalization;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using TechnikumPracticeDepartment;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("env.json", optional: true, reloadOnChange: true);

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
        options.HttpsPort = 443;
    });
}

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["TechnikumPracticeDepartment:SyncfussionKey"]);
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TechnikumPracticeDepartmentContext>(options => options.UseMySql(builder.Configuration["TechnikumPracticeDepartment:ConnectionString"], Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql")));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
});
builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = "Application";
        o.DefaultSignInScheme = "External";
    })
    .AddCookie("Application")
    .AddCookie("External")
    .AddGoogle(o =>
    {
        o.ClientId = builder.Configuration["TechnikumPracticeDepartment:GoogleClientID"];
        o.ClientSecret = builder.Configuration["TechnikumPracticeDepartment:GoogleClientSecret"];
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseHsts();
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithRedirects("/Home/Error");
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

var ci = new CultureInfo("ru-RU");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ci),
    SupportedCultures = new List<CultureInfo>
    {
        ci,
    },
    SupportedUICultures = new List<CultureInfo>
    {
        ci,
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();