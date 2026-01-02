using System;
using System.Collections.Generic;
using System.Linq;
using KimmyEsthi.Appointment;
using KimmyEsthi.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/appointments/{date}",
            async (DateTime date, KimmyEsthiDbContext db) =>
            {
                return await db.Appointments.Where(x => x.DateTime.Date == date.Date).OrderBy(x => x.DateTime).ToListAsync();
            }
        );

        app.MapGet(
            "/appointments/status",
            async (KimmyEsthiDbContext db) =>
            {
                var availableAndBookedDatesList = new List<AppointmentDateAndStatus>();
                var datesList = await db
                    // INCLUDE TODAY OR NO??
                    .Appointments.Where(x => x.DateTime >= DateTime.Today)
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
            async ([FromBody] AppointmentRequest appointmentRequest, KimmyEsthiDbContext db) =>
            {
                var appointmentToUpdate = await db
                    .Appointments.Include(a => a.ScheduledAppointment)
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
                appointmentToUpdate.ScheduledAppointment = new ScheduledAppointment
                {
                    ServiceName = appointmentRequest.ScheduledAppointment.ServiceName,
                    PreferredName = appointmentRequest.ScheduledAppointment.PreferredName,
                    Email = appointmentRequest.ScheduledAppointment.Email,
                    PhoneNumber = appointmentRequest.ScheduledAppointment.PhoneNumber,
                    SkinConcerns = appointmentRequest.ScheduledAppointment.SkinConcerns,
                };
                appointmentToUpdate.Status = AppointmentStatus.Booked;
                await db.SaveChangesAsync();
                return Results.Ok("Appointment request sent!");
            }
        );

        app.MapPost("/consentForm", async ([FromBody] ConsentForm consentForm, KimmyEsthiDbContext db) =>
        {
            var appt = await db.Appointments.FindAsync(consentForm.AppointmentId);
            if (appt is not Appointment)
            {
                return Results.NotFound();
            }
            appt.ConsentForm = consentForm;
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
