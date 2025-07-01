using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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

        app.MapPost("/appointment", async (Appointment appointment, AppointmentDb db) =>
        {
            await db.Appointments.AddAsync(appointment);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        app.Run();
    }
}
