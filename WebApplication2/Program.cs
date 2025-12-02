using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using WebApplication2.Services;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            builder.Services.AddControllersWithViews();

            builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

            builder.Services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();
            builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();
            builder.Services.AddHostedService<QueuedEmailSender>();

            builder.Services.AddHealthChecks();

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error/500");
                app.UseHsts();
            }

            app.UseForwardedHeaders();
            app.UseResponseCompression();
            app.UseHttpsRedirection();

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                        return;

                    const int cacheDuration = 60 * 60 * 24 * 30; // 30 дней
                    ctx.Context.Response.Headers.CacheControl = $"public,max-age={cacheDuration}";
                }
            });

            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
