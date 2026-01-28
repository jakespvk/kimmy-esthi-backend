using System.Linq;
using KimmyEsthi.Db;
using KimmyEsthi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KimmyEsthi.Endpoints;

public static class ServicesEndpoints
{
    public static void Map(WebApplication app)
    {
        var services = app.MapGroup("services");

        services.MapGet("", async (KimmyEsthiDbContext db) =>
        {
            return Results.Ok(await db.Services.Where(x => x.IsActive).ToArrayAsync());
        });

        services.MapGet("/search", async ([FromQuery] ServiceType serviceType, KimmyEsthiDbContext db) =>
        {
            return Results.Ok(await db.Services.Where(x => x.IsActive && x.ServiceType == serviceType).ToArrayAsync());
        });

        services.MapPost("", async ([FromBody] Service[] newServiceRequests, KimmyEsthiDbContext db) =>
        {
            await db.Services.AddRangeAsync(newServiceRequests);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
