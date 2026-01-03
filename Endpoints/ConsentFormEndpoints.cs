using KimmyEsthi.Client;
using KimmyEsthi.ConsentForm;
using KimmyEsthi.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public static class ConsentFormEndpoints
{
    public static void MapConsentFormEndpoints(this IEndpointRouteBuilder consentForm)
    {
        consentForm.MapPost("", async ([FromBody] ConsentForm consentForm, KimmyEsthiDbContext db) =>
        {
            var client = await db.Clients
                .FindAsync(consentForm.ClientId);
            if (client is not Client)
            {
                return Results.NotFound();
            }
            client.ConsentForm = consentForm;
            return Results.Ok(await db.SaveChangesAsync());
        });
    }
}
