using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using kimmy_esthi_backend;
using System.Collections.Generic;

internal class Program
{
    private static void Main(string[] args)
    {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<AppointmentDb>(opt => opt.UseSqlite("Data Source=db1.db"));
        // builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
        });
        var app = builder.Build();

        app.UseCors(MyAllowSpecificOrigins);

        app.MapGet("/appointments", async (AppointmentDb db) =>
            await db.Appointments.ToListAsync());

        app.MapGet("/appointments/{date}", async (DateTime date, AppointmentDb db) =>
        {
            return await db.Appointments.Where(x => x.DateTime.Date == date.Date).ToListAsync();
        });

        app.MapGet("/appointment/{id}", async (Guid id, AppointmentDb db) =>
        {
            var appointment = await db.Appointments.FindAsync(id);
            if (appointment is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(new AppointmentDateTime { Date = appointment.DateTime.Date, Time = appointment.DateTime });
        });

        app.MapPost("/appointment", async ([FromBody] AppointmentRequest appointmentRequest, AppointmentDb db) =>
        {
            var appointmentToUpdate = await db.Appointments
                .Include(a => a.ScheduledAppointment)
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
        });

        app.MapPut("/appointment", async (AppointmentDb db) =>
        {
            var appointment = new Appointment { DateTime = DateTime.Now, Status = AppointmentStatus.Available };
            await db.AddAsync(appointment);
            await db.SaveChangesAsync();
            return Results.Ok(appointment);
        });

        app.MapGet("/appointment1", async (AppointmentDb db) =>
        {
            return await db.Appointments.Where(a => a.DateTime.Date == DateTime.Now.Date).ToListAsync();
        });

        var admin = app.MapGroup("admin");
        admin.MapPost("/", async ([FromBody] LoginRequest loginRequest, AppointmentDb db) =>
        {
            if (loginRequest.Username == "test" && loginRequest.Password == "test")
            {
                return Results.Ok("hi!!!");
            }
            else
            {
                return Results.BadRequest();
            }
        });

        admin.MapPost("/old", async ([FromBody] AdminUser adminUser, AppointmentDb db) =>
        {
            return await db.AdminUsers.FindAsync(adminUser);
        });

        admin.MapGet("/", async (AppointmentDb db) =>
                await db.Appointments.ToListAsync());

        admin.MapPost("/appointments", async (List<Appointment> appointments, AppointmentDb db) =>
        {
            foreach (var appt in appointments)
            {
                if (appt is Appointment)
                {
                    await db.AddAsync(appt);
                }
            }
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        app.Run();
    }
}
