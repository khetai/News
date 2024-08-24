using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using News.Models;
using System.Globalization;





var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RmlubecoMediaContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<RmlubecoMediaContext>();





builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
          .AddCookie(options =>
          {
              options.Cookie.Name = "User";
              options.LoginPath = "/Admin/Login";
              options.AccessDeniedPath = "/Admin/Login";
              options.ExpireTimeSpan = TimeSpan.FromDays(7);
              options.SlidingExpiration = false;
          });
builder.Services.AddResponseCompression(options =>
{
	options.EnableForHttps = true;
	options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/html" });
});
//Globalization
builder.Services.AddLocalization(opt => { opt.ResourcesPath = "Resources"; });
builder.Services.AddMvc().AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();
//builder.Services.Configure<RequestLocalizationOptions>(
//    opt =>
//    {
//        var supportedCultures = new List<CultureInfo>
//        {
//                    new CultureInfo("en"),
//                    new CultureInfo("az"),
//                    //new CultureInfo("ru")
//        };
//        opt.DefaultRequestCulture = new RequestCulture("en");
//        opt.SupportedCultures = supportedCultures;
//        opt.SupportedUICultures = supportedCultures;
//    });

builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en"),
        new CultureInfo("az"),
        //new CultureInfo("ru")
    };

    opt.DefaultRequestCulture = new RequestCulture("en");
    opt.SupportedCultures = supportedCultures;
    opt.SupportedUICultures = supportedCultures;

    opt.SetDefaultCulture("en");
});

//Globalization

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
           );

builder.Services.AddHangfire(configuration => configuration
           .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		const int durationInSeconds = 60 * 60 * 24 * 365;
		ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
	}
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseHangfireDashboard();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
//Globalization
var options = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(options.Value);
//Globalization


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
