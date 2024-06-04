using MealFacility_Backend.Context;

namespace MealFacility_Backend.Helpers
{
    public class ExpireCoupon : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public ExpireCoupon(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DeleteExpiredCoupons, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void DeleteExpiredCoupons(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var expiredCoupons = context.Coupons.Where(c => c.ExpirationTime <= DateTime.Now).ToList();

                if (expiredCoupons.Any())
                {
                    context.Coupons.RemoveRange(expiredCoupons);
                    await context.SaveChangesAsync();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
