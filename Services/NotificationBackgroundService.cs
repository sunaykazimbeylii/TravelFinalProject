﻿using Microsoft.EntityFrameworkCore;
using TravelFinalProject.DAL;
using TravelFinalProject.Interfaces;

namespace TravelFinalProject.Services
{

    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

                    var finishedTours = await context.Tours
                        .Where(t => t.End_Date <= nowDateOnly)
                        .ToListAsync();

                    foreach (var tour in finishedTours)
                    {
                        var bookings = await context.Bookings
                            .Where(b => b.TourId == tour.Id)
                            .ToListAsync();

                        foreach (var booking in bookings)
                        {
                            await notificationService.SendNotificationAsync(booking.UserId, tour.Id);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }


}

