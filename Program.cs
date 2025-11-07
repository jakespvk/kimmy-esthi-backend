using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using kimmy_esthi_backend;

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
            Console.WriteLine("DB file: " + db.Database.GetDbConnection().DataSource);
            return await db.Appointments.Where(x => x.DateTime.Date == date.Date).ToListAsync();
        });

        app.MapGet("/appointment/{id}", async (Guid id, AppointmentDb db) =>
        {
            var appointment = await db.Appointments
                .Where(x =>
                        x.Id.ToString()
                        .Equals(
                            id.ToString()))
                .FirstOrDefaultAsync();
            if (appointment is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(new AppointmentDateTime { Date = appointment.DateTime.Date, Time = appointment.DateTime });
        });

        app.MapPost("/appointment", async ([FromBody] AppointmentRequest appointmentRequest, AppointmentDb db) =>
        {
            var appointmentToUpdate = await db.Appointments
                .Where(x => x.Id.ToString().Equals(appointmentRequest.AppointmentId.ToString()))
                .FirstOrDefaultAsync();
            if (appointmentToUpdate is null)
            {
                return Results.NotFound();
            }
            appointmentToUpdate.ServiceName = appointmentRequest.ServiceName;
            appointmentToUpdate.ScheduledAppointment = new ScheduledAppointment
            {
                AppointmentId = new Guid(appointmentRequest.ScheduledAppointment.AppointmentId.ToString()),
                PreferredName = appointmentRequest.ScheduledAppointment.PreferredName,
                Email = appointmentRequest.ScheduledAppointment.Email,
                PhoneNumber = appointmentRequest.ScheduledAppointment.PhoneNumber,
                SkinConcerns = appointmentRequest.ScheduledAppointment.SkinConcerns,
            };
            appointmentToUpdate.Status = AppointmentStatus.Booked;
            await db.SaveChangesAsync();
            return Results.Ok("Appointment request sent!");
        });

        var admin = app.MapGroup("admin");
        admin.MapPost("/", async ([FromBody] AdminUser adminUser, AppointmentDb db) =>
        {
            return await db.AdminUsers.FindAsync(adminUser);
        });

        admin.MapGet("/", async (AppointmentDb db) =>
                await db.Appointments.ToListAsync());

        app.Run();
    }
}
