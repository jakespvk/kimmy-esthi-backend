using System.Linq;
using KimmyEsthi.ConsentForm;
using KimmyEsthi.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class ConsentFormEndpoints
{
    public static void Map(WebApplication app)
    {
        var consentForm = app.MapGroup("consentForm");

        consentForm.MapGet("/statements", async (KimmyEsthiDbContext db) =>
        {
            return Results.Ok(await db.ConsentFormStatements.Where(x => x.IsActive).ToArrayAsync());
        });

        consentForm.MapPost("", async ([FromBody] ConsentForm consentForm, KimmyEsthiDbContext db) =>
        {
            var client = await db.Clients
                .FindAsync(consentForm.ClientId);
            if (client is null)
            {
                return Results.NotFound();
            }
            client.ConsentForm = consentForm;
            return Results.Ok(await db.SaveChangesAsync());
        });

        consentForm.MapPut("/statements", async ([FromBody] ConsentFormStatement statement, KimmyEsthiDbContext db) =>
        {
            db.ConsentFormStatements.Update(statement);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        consentForm.MapPost("/statements", async ([FromBody] ConsentFormStatement statement, KimmyEsthiDbContext db) =>
        {
            await db.AddAsync(statement);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
