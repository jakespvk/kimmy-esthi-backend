using System.Linq;
using KimmyEsthi.Clients;
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
            return Results.Ok(await db.ConsentFormStatements.ToArrayAsync());
        });

        consentForm.MapGet("/statements/active", async (KimmyEsthiDbContext db) =>
        {
            return Results.Ok(await db.ConsentFormStatements.Where(x => x.IsActive).ToArrayAsync());
        });

        consentForm.MapPost("", async ([FromBody] ConsentForm consentForm, KimmyEsthiDbContext db) =>
        {
            var client = await db.Clients
                .FindAsync(consentForm.ClientId);
            if (client is null)
            {
                client = new Client { PreferredName = consentForm.PrintedName };
            }
            client.ConsentForm = consentForm;
            return Results.Ok(await db.SaveChangesAsync());
        });

        consentForm.MapPut("/statements", async ([FromBody] ConsentFormStatement statement, KimmyEsthiDbContext db) =>
        {
            var cfs = await db.ConsentFormStatements.FindAsync(statement.Id);
            if (cfs is null)
            {
                return Results.NotFound();
            }
            cfs.Statement = statement.Statement;
            cfs.IsActive = statement.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        consentForm.MapPost("/statements", async ([FromBody] ConsentFormStatement statement, KimmyEsthiDbContext db) =>
        {
            var entityEntry = await db.AddAsync(statement);
            await db.SaveChangesAsync();
            return Results.Ok(entityEntry.Entity.Id);
        });
    }
}
