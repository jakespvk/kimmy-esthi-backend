using System;
using System.Collections.Generic;
using System.Linq;
using KimmyEsthi.Appointments;
using KimmyEsthi.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class PromotionAppointmentEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/promotions", async (KimmyEsthiDbContext db) =>
                {
                    return await db.Promotions.ToListAsync();
                });

        app.MapGet(
            "/appointments/{date}/promotion/{promotion_str}",
            async (DateTime date, string promotion_str, KimmyEsthiDbContext db) =>
            {
                return await db.Appointments.Where(x => x.Promotion != null && x.Promotion.Name == promotion_str && x.DateTime.Date == date.Date).OrderBy(x => x.DateTime).ToListAsync();
            }
        );

        app.MapGet(
            "/appointments/promotion/status/{promotion_str}",
            async (string promotion_str, KimmyEsthiDbContext db) =>
            {
                var availableAndBookedDatesList = new List<AppointmentDateAndStatus>();
                var datesList = await db
                    // INCLUDE TODAY OR NO??
                    .Appointments.Where(x => x.DateTime >= DateTime.Today && x.Promotion != null && x.Promotion.Name == promotion_str)
                    .ToListAsync();
                foreach (var date in datesList)
                {
                    availableAndBookedDatesList.Add(
                        new AppointmentDateAndStatus
                        {
                            DateTime = date.DateTime,
                            Status = await db.Appointments.AnyAsync(x =>
                                x.DateTime.Date == date.DateTime.Date
                                && x.Status == false
                            ),
                        }
                    );
                }
                return Results.Ok(availableAndBookedDatesList);
            }
        );

        app.MapPost("/promotion",
                async ([FromBody] Promotion promotion, KimmyEsthiDbContext db) =>
                {
                    if (await db.Promotions.AnyAsync(x => x.Name == promotion.Name))
                        return Results.BadRequest("Promotion already exists!");
                    await db.Promotions.AddAsync(promotion);
                    await db.SaveChangesAsync();
                    return Results.Accepted($"Successfully created promotion: {promotion.Name}");
                });
    }
}
