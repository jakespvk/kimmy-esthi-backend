using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

internal class Program
{
    private static void Main(string[] args)
    {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<AppointmentDb>(opt => opt.UseSqlite("Data Source=db1.db"));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
        });
        var app = builder.Build();

        app.UseCors(MyAllowSpecificOrigins);

        app.MapGet("/appointments", async (AppointmentDb db) =>
            await db.Appointments.ToListAsync());

        app.MapGet("/appointments/{date}", async (DateOnly date, AppointmentDb db) =>
        {
            return await db.Appointments.Where(x => x.Date.ToString() == date.ToString()).ToListAsync();
        });

        app.MapGet("/appointment/{id}", async (Guid id, AppointmentDb db) =>
        {
            var appointment = await db.Appointments
                .Where(x =>
                        x.Id.ToString()
                        .Equals(
                            id.ToString()))
                .FirstOrDefaultAsync();
            if (appointment is Appointment)
            {
                return Results.Ok(new AppointmentDateTime { Time = appointment.Time, Date = appointment.Date });
            }
            return Results.NotFound();
        });

        app.MapPost("/appointment", async ([FromBody] Appointment appointment, AppointmentDb db) =>
        {
            await db.Appointments.AddAsync(appointment);
            await db.SaveChangesAsync();
            return Results.Ok("Appointment created successfully");
        });

        app.Run();
    }
}
