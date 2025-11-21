using EVMarketPlace.Services.Interfaces;
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
    public class AuctionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuctionBackgroundService> _logger;
        // ⏱️ TEST MODE: Check mỗi 10 giây thay vì 1 phút
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

        public AuctionBackgroundService(IServiceProvider serviceProvider, ILogger<AuctionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Auction background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // ✅ Tạo scope mới để dùng các service dạng Scoped (như IAuctionService, DbContext, Repository,...)
                    using (var scope = _serviceProvider.CreateScope())
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
