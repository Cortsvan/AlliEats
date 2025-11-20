using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class OrderTimerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderTimerService> _logger;

        // TESTING MODE - Very short intervals for quick testing
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // Check every 30 seconds
        private readonly TimeSpan _autoReceiveTimeout = TimeSpan.FromMinutes(2); // Auto-receipt after 2 minutes

        // PRODUCTION MODE - Uncomment these when ready for production
        // private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes
        // private readonly TimeSpan _autoReceiveTimeout = TimeSpan.FromHours(2); // Auto-receipt after 2 hours

        public OrderTimerService(IServiceScopeFactory scopeFactory, ILogger<OrderTimerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Order Timer Service STARTED! [TESTING MODE - 30s check / 2min timeout]");
            _logger.LogInformation("⏰ Checking for overdue orders every {Interval} seconds", _checkInterval.TotalSeconds);
            _logger.LogInformation("🕐 Auto-receipt timeout: {Timeout} minutes", _autoReceiveTimeout.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    CheckOverdueOrders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error occurred while processing overdue orders");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("🛑 Order Timer Service STOPPED");
        }

        private void CheckOverdueOrders()
        {
            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            _logger.LogDebug("🔍 Timer check at {Time} - Looking for overdue orders...", DateTime.Now.ToString("HH:mm:ss"));

            try
            {
                var allOrders = orderService.GetAllOrders("On the Way");
                var onTheWayOrders = allOrders.ToList();

                if (!onTheWayOrders.Any())
                {
                    _logger.LogDebug("✅ No 'On the Way' orders found");
                    return;
                }

                _logger.LogInformation("📊 Found {Count} orders 'On the Way'", onTheWayOrders.Count);

                var now = DateTime.Now;
                var overdueOrders = onTheWayOrders.Where(order =>
                {
                    var statusChangeTime = order.UpdatedTime ?? order.CreatedTime;
                    var timeSinceUpdate = now - statusChangeTime;

                    _logger.LogDebug("📋 Order {OrderNumber}: {Minutes}m {Seconds}s ago (overdue: {IsOverdue})",
                        order.OrderNumber,
                        (int)timeSinceUpdate.TotalMinutes,
                        timeSinceUpdate.Seconds,
                        timeSinceUpdate >= _autoReceiveTimeout);

                    return timeSinceUpdate >= _autoReceiveTimeout;
                }).ToList();

                if (!overdueOrders.Any())
                {
                    _logger.LogDebug("⏳ No overdue orders found");
                    return;
                }

                _logger.LogWarning("🎯 FOUND {Count} OVERDUE ORDERS - AUTO-MARKING AS RECEIVED!", overdueOrders.Count);

                foreach (var order in overdueOrders)
                {
                    try
                    {
                        _logger.LogWarning("🔄 AUTO-RECEIVING Order #{OrderNumber}", order.OrderNumber);
                        orderService.UpdateOrderStatus(order.Id, "Received");
                        _logger.LogInformation("✅ Order #{OrderNumber} automatically marked as RECEIVED!", order.OrderNumber);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to auto-mark order {OrderNumber}", order.OrderNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in timer check");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🛑 Order Timer Service stopping...");
            await base.StopAsync(stoppingToken);
            _logger.LogInformation("🛑 Order Timer Service stopped");
        }
    }
}