using System;
using System.Collections.Generic;
using System.Linq;
using KimmyEsthi.Appointments;
using KimmyEsthi.Client;
using KimmyEsthi.Db;
using KimmyEsthi.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class AppointmentEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet(
            "/appointments/{date}",
            async (DateTime date, KimmyEsthiDbContext db) =>
            {
                return await db.Appointments.Where(x => x.Promotion == null && x.DateTime.Date == date.Date).OrderBy(x => x.DateTime).ToListAsync();
            }
        );

        app.MapGet(
            "/appointments/status",
            async (KimmyEsthiDbContext db) =>
            {
                var availableAndBookedDatesList = new List<AppointmentDateAndStatus>();
                var datesList = await db
                    // INCLUDE TODAY OR NO??
                    .Appointments.Where(x => x.Promotion == null && x.DateTime >= DateTime.Today)
                    .ToListAsync();
                foreach (var date in datesList)
                {
                    availableAndBookedDatesList.Add(
                        new AppointmentDateAndStatus
                        {
                            DateTime = date.DateTime,
                            Status = await db.Appointments.AnyAsync(x =>
                                x.DateTime.Date == date.DateTime.Date
                                && x.Status == AppointmentStatus.Available
                            ),
                        }
                    );
                }
                return Results.Ok(availableAndBookedDatesList);
            }
        );

        app.MapGet(
            "/appointment/{id}",
            async (Guid id, KimmyEsthiDbContext db) =>
            {
                var appointment = await db.Appointments.FindAsync(id);
                if (appointment is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(
                    new AppointmentDateTime
                    {
                        Date = appointment.DateTime.Date,
                        Time = appointment.DateTime,
                    }
                );
            }
        );

        app.MapPost(
            "/appointment",
            async ([FromBody] AppointmentRequest appointmentRequest, KimmyEsthiDbContext db, EmailService emailService) =>
            {
                var appointmentToUpdate = await db
                    .Appointments.Include(a => a.ScheduledAppointment)
                    .Include(a => a.Promotion)
                    .Where(x => x.Id == appointmentRequest.AppointmentId)
                    .FirstOrDefaultAsync();
                if (appointmentToUpdate is null)
                {
                    return Results.NotFound();
                }
                if (appointmentToUpdate.ScheduledAppointment != null)
                {
                    return Results.BadRequest();
                }
                var client = await db.Clients.FindAsync(appointmentRequest.ScheduledAppointment.Client.Email);
                appointmentToUpdate.ScheduledAppointment = new ScheduledAppointment
                {
                    ServiceName = appointmentRequest.ScheduledAppointment.ServiceName,
                    Client = client ?? new Client
                    {
                        AppointmentId = appointmentRequest.AppointmentId,
                        PreferredName = appointmentRequest.ScheduledAppointment.Client.PreferredName,
                        Email = appointmentRequest.ScheduledAppointment.Client.Email,
                        PhoneNumber = appointmentRequest.ScheduledAppointment.Client.PhoneNumber,
                    },
                    SkinConcerns = appointmentRequest.ScheduledAppointment.SkinConcerns,
                };
                if (appointmentRequest.Promotion != null && appointmentRequest.Promotion.Id != Guid.Empty)
                {
                    appointmentToUpdate.Promotion = await db.Promotions.FindAsync(appointmentRequest.Promotion.Id);
                }
                else if (appointmentRequest.Promotion != null && !string.IsNullOrEmpty(appointmentRequest.Promotion.Name))
                {
                    appointmentToUpdate.Promotion = new Promotion
                    {
                        Name = appointmentRequest.Promotion.Name,
                    };
                }
                appointmentToUpdate.Status = AppointmentStatus.Booked;
                await db.SaveChangesAsync();
                await emailService.SendAppointmentRequestEmail(
                                    appointmentToUpdate.ScheduledAppointment.ClientId,
                                    appointmentToUpdate.Id,
                                    appointmentToUpdate.ScheduledAppointment.Client.Email
                                );
                return Results.Ok(appointmentToUpdate.ScheduledAppointment.ClientId);
            }
        );

        // app.MapPost("/consentForm", async ([FromBody] ConsentForm consentForm, KimmyEsthiDbContext db) =>
        // {
        //     var appt = await db.Appointments.FindAsync(consentForm.AppointmentId);
        //     if (appt is null)
        //     {
        //         return Results.NotFound();
        //     }
        //     appt.ConsentForm = consentForm;
        //     await db.SaveChangesAsync();
        //     return Results.Ok();
        // });
    }
}
