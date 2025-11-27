using EVMarketPlace.Repositories.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class PostExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PostExpirationService> _logger;

        public PostExpirationService(IServiceScopeFactory scopeFactory, ILogger<PostExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PostExpirationService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var postRepo = scope.ServiceProvider.GetRequiredService<PostRepository>();

                    var count = await postRepo.HideExpiredPostsAsync();
                    
                    if (count > 0)
                    {
                        _logger.LogInformation("Hidden {count} expired posts at {time}", count, DateTime.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PostExpirationService.");
                }

                // Chờ 1 phút trước khi chạy lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    
}
}
