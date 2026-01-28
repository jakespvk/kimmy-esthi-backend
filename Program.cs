using DotNetEnv;
using KimmyEsthi.Db;
using KimmyEsthi.Email;
using KimmyEsthi.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Env.Load();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddDbContext<KimmyEsthiDbContext>(opt => opt.UseSqlite("Data Source=Db/db1.db;"));
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
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

AppointmentEndpoints.Map(app);
PromotionAppointmentEndpoints.Map(app);
AdminEndpoints.Map(app);
ConsentFormEndpoints.Map(app);
ServicesEndpoints.Map(app);
TestEmailEndpoint.Map(app);

app.Run();
