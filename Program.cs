var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

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

using var db = new AppointmentContext();
var appointmentsOnSelectedDay = db.nonbooked_appointments
    .Where(d => d.Date == "11/19/2024")
    .ToArray();
//AppointmentCollection.Init();

//IEnumerable<Appointment> appointmentsOnSelectedDay = AppointmentCollection.GetAppointmentsForGivenDay("11/1/2024");

/*app.MapGet("/api/appointments", () =>
{
    return appointmentsOnSelectedDay;
});*/

app.MapGet("/api/appointments", () =>
{
    return appointmentsOnSelectedDay;
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

app.MapPost("/api/appointments", async (HttpRequest request) =>
{
    var date = await request.ReadFromJsonAsync<string>();
    date = date?.ToString();
    appointmentsOnSelectedDay = db.nonbooked_appointments
        .Where(d => d.Date == date)
        //.OrderBy(t => t.Time)
        .ToArray();
    return appointmentsOnSelectedDay;
});

app.MapPost("/api/appointment", (ScheduledAppointment scheduledAppointment) =>
{
    return Results.Ok("Your appointment has been created");
});

app.Run();
