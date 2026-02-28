using DotNetEnv;
using KimmyEsthi.Db;
using KimmyEsthi.Email;
using KimmyEsthi.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Env.Load();

const string KimmyEsthiCorsPolicy = "KimmyEsthiCorsPolicy";
var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddDbContext<KimmyEsthiDbContext>(opt => opt.UseSqlite("Data Source=Db/db1.db;"));
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: KimmyEsthiCorsPolicy,
        policy =>
        {
            policy.WithOrigins(["http://localhost:3000", "https://sunsetkimcare.com"]).AllowAnyMethod().AllowAnyHeader();
        }
    );
});
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

app.UseCors(KimmyEsthiCorsPolicy);

AppointmentEndpoints.Map(app);
PromotionAppointmentEndpoints.Map(app);
AdminEndpoints.Map(app);
ConsentFormEndpoints.Map(app);
ServicesEndpoints.Map(app);
TestEmailEndpoint.Map(app);
ReviewEndpoints.Map(app);

app.Run();
