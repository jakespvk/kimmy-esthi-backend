using System;
using System.Collections.Generic;
using System.Linq;
using kimmy_esthi_backend;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppointmentDb>(opt => opt.UseSqlite("Data Source=db1.db;"));
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

app.MapGet(
    "/appointments/{date}",
    async (DateTime date, AppointmentDb db) =>
    {
        return await db.Appointments.Where(x => x.DateTime.Date == date.Date).OrderBy(x => x.DateTime).ToListAsync();
    }
);

app.MapGet(
    "/appointments/status",
    async (AppointmentDb db) =>
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
    async (Guid id, AppointmentDb db) =>
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
    async ([FromBody] AppointmentRequest appointmentRequest, AppointmentDb db) =>
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

app.MapPost("/consentForm", async ([FromBody] ConsentForm consentForm, AppointmentDb db) =>
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

var admin = app.MapGroup("admin");

admin.MapPost(
    "/",
    async ([FromBody] LoginRequest loginRequest, AppointmentDb db) =>
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
    async (string token, Appointment[] appointments, AppointmentDb db) =>
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
    async ([FromQuery] bool booked, [FromQuery] bool includeArchived, [FromQuery] DateTime? date, string token, AppointmentDb db) =>
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

// admin.MapPost("/secret", async (AppointmentDb db) =>
//         {
//             await db.AdminUsers.AddAsync(new AdminUser { Username = "kimmxthy", Password = "KakeKakeKake4eva" });
//             await db.SaveChangesAsync();
//         });

app.Run();
