using KimmyEsthi.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
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

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

app.MapAppointmentEndpoints();

var admin = app.MapGroup("admin");
admin.MapAdminEndpoints();

app.Run();
