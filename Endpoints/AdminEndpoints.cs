using System;
using System.Collections.Generic;
using System.Linq;
using KimmyEsthi.Admin;
using KimmyEsthi.Appointments;
using KimmyEsthi.ConsentForm;
using KimmyEsthi.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace KimmyEsthi.Endpoints;

public static class AdminEndpoints
{
    public static void Map(WebApplication app)
    {
        var admin = app.MapGroup("admin");

        admin.MapPost(
            "/",
            async ([FromBody] LoginRequest loginRequest, KimmyEsthiDbContext db) =>
            {
                if (loginRequest.Username == "kimmxthy" && loginRequest.Password == "KakeKakeKake4eva")
                {
                    var user = await db
                        .AdminUsers.Where(x => x.Username == loginRequest.Username)
                        .FirstOrDefaultAsync();

                    if (user is null)
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
            async (string token, List<Appointment> appointments, KimmyEsthiDbContext db) =>
            {
                if (!await db.AdminUsers.AnyAsync(x => x.Token == token))
                {
                    return Results.Challenge();
                }
                var failedToAddAppointments = new List<string>();
                foreach (var appt in appointments)
                {
                    if (appt is null)
                    {
                        failedToAddAppointments.Add($"Appointment: {appt} is empty");
                    }
                    else if (await db.Appointments.AnyAsync(x => x.DateTime == appt.DateTime))
                    {
                        failedToAddAppointments.Add($"Appointment: {appt.DateTime:yyyy-MM-dd HH:mm} already exists");
                        appointments.Remove(appt);
                    }
                }
                await db.AddRangeAsync(appointments);
                await db.SaveChangesAsync();
                if (failedToAddAppointments.Count > 0)
                {
                    Console.WriteLine(failedToAddAppointments[0]);
                    return Results.BadRequest(failedToAddAppointments);
                }
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
                        .Appointments.Where(x => x.Status == true)
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
                }
                else if (booked && (date is not null))
                {
                    return await db
                        .Appointments.Where(x =>
                            x.Status == true && x.DateTime.Date == ((DateTime)date).Date
                        )
                        .Include(x => x.ScheduledAppointment)
                        .OrderBy(x => x.DateTime)
                        .ToListAsync();
                }
                else if (booked)
                {
                    return await db
                        .Appointments.Where(x => x.Status == true && x.DateTime >= DateTime.Now.AddDays(-1))
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

        admin.MapPost("{token}/appointments/promotion",
                async ([FromBody] Appointment[] appointments, string token, KimmyEsthiDbContext db) =>
                {
                    if (!await db.AdminUsers.AnyAsync(x => x.Token == token))
                    {
                        Results.Challenge();
                        return null;
                    }
                    foreach (var appointment in appointments)
                    {
                        if (await db.Appointments.AnyAsync(x => x.DateTime == appointment.DateTime))
                            return Results.BadRequest("Already an appointment at that time!");
                        if (appointment.Promotion == null)
                            return Results.BadRequest("Promotion must be specified for this endpoint!");
                        await db.Appointments.AddAsync(appointment);
                    }
                    await db.SaveChangesAsync();
                    return Results.Accepted("Appointment created for promotion");
                });

        admin.MapGet("{token}/consentForms", async ([FromQuery] string query, string token, KimmyEsthiDbContext db) =>
                {
                    if (!await db.AdminUsers.AnyAsync(x => x.Token == token))
                    {
                        Results.Challenge();
                        return null;
                    }
                    return Results.Ok(await db.Appointments
                            .Where(x => (x.ConsentForm != null
                                    && x.ConsentForm.PrintedName.Contains(query))
                                || (x.ScheduledAppointment != null
                                    && (x.ScheduledAppointment.Client.Email != null
                                        && x.ScheduledAppointment.Client.Email.Contains(query)
                                        || x.ScheduledAppointment.Client.PhoneNumber != null
                                        && x.ScheduledAppointment.Client.PhoneNumber.Contains(query))))
                            .Select(x => new ConsentFormSummaryDTO
                            {
                                ConsentFormId = x.Id,
                                ClientName = x.ScheduledAppointment!.Client.PreferredName,
                                ClientEmail = x.ScheduledAppointment.Client.Email!,
                                ClientPhone = x.ScheduledAppointment.Client.PhoneNumber!
                            })
                            .FirstOrDefaultAsync());
                });

        admin.MapGet("{token}/consentForms/{consentFormId}", async (Guid consentFormId, string token, KimmyEsthiDbContext db) =>
                {
                    if (!await db.AdminUsers.AnyAsync(x => x.Token == token))
                    {
                        Results.Challenge();
                        return null;
                    }
                    return Results.Ok(await db.Appointments
                            .Where(x => x.ConsentForm != null && x.ConsentForm.Id == consentFormId)
                            .Select(x => x.ConsentForm).FirstOrDefaultAsync());
                });

        // admin.MapPost("/secret", async (KimmyEsthiDbContext db) =>
        //         {
        //             await db.AdminUsers.AddAsync(new AdminUser { Username = "kimmxthy", Password = "KakeKakeKake4eva" });
        //             await db.SaveChangesAsync();
        //         });
    }
}
