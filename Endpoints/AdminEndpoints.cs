using System;
using System.Linq;
using KimmyEsthi.Admin;
using KimmyEsthi.Appointment;
using KimmyEsthi.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder admin)
    {
        admin.MapPost(
            "/",
            async ([FromBody] LoginRequest loginRequest, KimmyEsthiDbContext db) =>
            {
                if (loginRequest.Username == "kimmxthy" && loginRequest.Password == "KakeKakeKake4eva")
                {
                    var user = await db
                        .AdminUsers.Where(x => x.Username == loginRequest.Username)
                        .FirstOrDefaultAsync();

                    if (user is not AdminUser)
                    {
                        await db.AdminUsers.AddAsync(new AdminUser { Username = "kimmxthy", Password = "KakeKakeKake4eva" });
                        await db.SaveChangesAsync();
                        user = await db.AdminUsers.Where(x => x.Username == loginRequest.Username).FirstAsync();
                    }
                    // user.Token = "super-secret-and-special-token-anya-forger-is-the-voice-im-hearing-in-my-head-when-i-type-that";
                    user.Token =
                        new Random(1000000000).GetHashCode().ToString()
                        + new Random(1000000000).GetHashCode().ToString();
                    await db.SaveChangesAsync();
                    return Results.Ok(user.Token);
                }
                else
                {
                    return Results.BadRequest();
                }
            }
        );

        admin.MapPost(
            "/{token}/appointments",
            async (string token, Appointment[] appointments, KimmyEsthiDbContext db) =>
            {
                if (!await db.AdminUsers.AnyAsync(x => x.Token == token))
                {
                    return Results.Challenge();
                }
                foreach (var appt in appointments)
                {
                    if (appt is null)
                    {
                        return Results.BadRequest($"appointment: {appt?.DateTime} was empty");
                    }
                    else if (await db.Appointments.AnyAsync(x => x.DateTime == appt.DateTime))
                    {
                        return Results.BadRequest(
                            $"appointment: {appt.DateTime:yyyy-MM-dd HH:mm} already exists"
                        );
                    }
                    else
                    {
                        await db.Appointments.AddAsync(appt);
                    }
                }
                await db.SaveChangesAsync();
                return Results.Ok();
            }
        );

        admin.MapGet(
            "/{token}/appointments",
            async ([FromQuery] bool booked, [FromQuery] bool includeArchived, [FromQuery] DateTime? date, string token, KimmyEsthiDbContext db) =>
            {
                if (!await db.AdminUsers.AnyAsync(x => x.Token == token))
                {
                    Results.Challenge();
                    return null;
                }
                if (booked && includeArchived)
                {
                    return await db
                        .Appointments.Where(x => x.Status == AppointmentStatus.Booked)
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
                }
                else if (booked && (date is not null))
                {
                    return await db
                        .Appointments.Where(x =>
                            x.Status == AppointmentStatus.Booked && x.DateTime.Date == ((DateTime)date).Date
                        )
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
                }
                else if (booked)
                {
                    return await db
                        .Appointments.Where(x => x.Status == AppointmentStatus.Booked && x.DateTime >= DateTime.Now.AddDays(-1))
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
                }
                else if (includeArchived)
                {
                    return await db
                        .Appointments
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
                }
                else if (date is not null)
                {
                    return await db
                        .Appointments.Where(x => x.DateTime.Date == ((DateTime)date).Date)
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
                }

                return await db
                    .Appointments.Where(x => x.DateTime > DateTime.Now.AddDays(-1))
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
            }
        );

        // admin.MapPost("/secret", async (KimmyEsthiDbContext db) =>
        //         {
        //             await db.AdminUsers.AddAsync(new AdminUser { Username = "kimmxthy", Password = "KakeKakeKake4eva" });
        //             await db.SaveChangesAsync();
        //         });
    }
}
