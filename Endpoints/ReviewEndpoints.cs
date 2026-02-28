using KimmyEsthi.Db;
using KimmyEsthi.Reviews;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KimmyEsthi.Endpoints;

public static class ReviewEndpoints
{
    public static void Map(WebApplication app)
    {
        var reviews = app.MapGroup("reviews");

        reviews.MapGet("", async (KimmyEsthiDbContext db) =>
        {
            return Results.Ok(await db.Reviews.ToArrayAsync());
        });

        reviews.MapPost("", async (Review review, KimmyEsthiDbContext db) =>
        {
            await db.Reviews.AddAsync(review);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
