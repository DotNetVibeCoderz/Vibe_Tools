using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using SimpleETL.Data;
using SimpleETL.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SimpleETL
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = default!;
        private WebApplication? _webApp;

        protected override async void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddBlazorWebViewDeveloperTools();
            serviceCollection.AddMudServices();

            // Add App Services
            serviceCollection.AddSingleton<SettingsService>();
            serviceCollection.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
            serviceCollection.AddSingleton<FlowEngine>();
            serviceCollection.AddHostedService<JobSchedulerService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Initialize DB
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // Start Embedded Web API
            await StartWebApi();

            base.OnStartup(e);
        }

        private async Task StartWebApi()
        {
            var settings = ServiceProvider.GetRequiredService<SettingsService>();
            int port = settings.ApiPort > 0 ? settings.ApiPort : 5000;

            var builder = WebApplication.CreateBuilder();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddSingleton<FlowEngine>(ServiceProvider.GetRequiredService<FlowEngine>());

            _webApp = builder.Build();

            _webApp.MapGet("/api/flows", (AppDbContext db) => 
            {
                return Results.Ok(db.DataFlows);
            });

            _webApp.MapPost("/api/flows/{id}/trigger", async (int id, FlowEngine engine) => 
            {
                _ = engine.ExecuteFlowAsync(id);
                return Results.Ok(new { message = $"Flow {id} triggered." });
            });

            _webApp.MapGet("/api/logs", (AppDbContext db) => 
            {
                return Results.Ok(db.JobLogs);
            });

            _webApp.Urls.Add($"http://localhost:{port}");

            try 
            {
                await _webApp.StartAsync();
            }
            catch 
            {
                // Fallback or ignore if port is in use
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_webApp != null)
            {
                await _webApp.StopAsync();
            }
            base.OnExit(e);
        }
    }
}
