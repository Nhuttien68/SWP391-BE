using EVMarketPlace.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class AuctionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuctionBackgroundService> _logger;

        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10); // Test mode

        public AuctionBackgroundService(IServiceScopeFactory scopeFactory,
                                        ILogger<AuctionBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Auction background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();
                        await auctionService.CloseExpiredAuctionsAsync();

                        _logger.LogInformation("✅ Closed expired auctions at {Time}", DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in auction background service.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("🛑 Auction background service stopped.");
        }
    }
}
