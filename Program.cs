using Microsoft.EntityFrameworkCore;

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

/*app.MapGet("/api/appointments/{date}", (HttpRequest request) =>
{
    var apptDate = request.RouteValues["date"]?.ToString();
    var cleanedInput = HttpUtility.UrlDecode(apptDate);
    return AppointmentCollection.GetAppointmentsForGivenDay(cleanedInput);
});*/


/*app.MapPost("/api/appointments", async (HttpRequest request) =>
{
    var date = await request.ReadFromJsonAsync<string>();
    date = date?.ToString();
    appointmentsOnSelectedDay = AppointmentCollection.GetAppointmentsForGivenDay(date);
    return appointmentsOnSelectedDay;
});*/

// app.MapPost("/api/appointments", async (HttpRequest request) =>
// {
//     var date = await request.ReadFromJsonAsync<string>();
//     date = date?.ToString();
//     appointmentsOnSelectedDay = db.nonbooked_appointments
//         .Where(d => d.Date == date)
//         //.OrderBy(t => t.Time)
//         .ToArray();
//     return appointmentsOnSelectedDay;
// });
//
// app.MapPost("/api/appointment", (ScheduledAppointment scheduledAppointment) =>
// {
//     return Results.Ok("Your appointment has been created");
// });

app.Run();
