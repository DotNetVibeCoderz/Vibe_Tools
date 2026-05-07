using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimpleETL.Data;
using Cronos;

namespace SimpleETL.Services
{
    public class JobSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public JobSchedulerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var flowEngine = scope.ServiceProvider.GetRequiredService<FlowEngine>();
                        
                        var flows = db.DataFlows.Where(f => f.IsScheduled && !string.IsNullOrEmpty(f.CronExpression)).ToList();
                        var now = DateTime.UtcNow;

                        foreach (var flow in flows)
                        {
                            try 
                            {
                                CronExpression expression = CronExpression.Parse(flow.CronExpression);
                                // A simplistic way to check if it should run this minute
                                // For real production, use Quartz.NET. Here we check if NextOccurrence is close.
                                var next = expression.GetNextOccurrence(now.AddMinutes(-1));
                                
                                if (next.HasValue && Math.Abs((now - next.Value).TotalMinutes) < 1)
                                {
                                    // Trigger execution
                                    _ = flowEngine.ExecuteFlowAsync(flow.Id);
                                }
                            }
                            catch
                            {
                                // Invalid cron expression, skip
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore transient errors
                }

                // Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
