using System;
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

        consentForm.MapDelete("/statements/{id}", async (int id, KimmyEsthiDbContext db) =>
        {
            var statementToDelete = await db.ConsentFormStatements.FindAsync(id);
            if (statementToDelete == null)
            {
                return Results.NotFound();
            }
            db.ConsentFormStatements.Remove(statementToDelete);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        consentForm.MapPost("/clientInfo", async ([FromBody] ConsentFormClientInfo clientInfo, KimmyEsthiDbContext db) =>
        {
            var client = new Client
            {
                PreferredName = clientInfo.FullName,
                Email = clientInfo.Email,
                PhoneNumber = clientInfo.PhoneNumber,
                DOB = clientInfo.DOB,
                Gender = clientInfo.Gender,
            };

            var entity = await db.Clients.AddAsync(client);
            await db.SaveChangesAsync();
            return Results.Ok(entity.Entity.ClientId);
        });

        consentForm.MapPost("/skincareHistory", async ([FromBody] SkincareHistoryRequest request, KimmyEsthiDbContext db) =>
        {
            var client = await db.Clients.FindAsync(request.ClientId);
            if (client is null)
            {
                return Results.NotFound("Client not found");
            }

            var questionnaire = request.SkincareHistoryQuestionnaire;

            var existing = await db.SkincareHistoryQuestionnaires.Where(x => x.ClientId == questionnaire.ClientId).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.EverReceivedFacial = questionnaire.EverReceivedFacial;
                existing.LastFacialDate = questionnaire.LastFacialDate;
                existing.Retinol = questionnaire.Retinol;
                existing.ChemPeel = questionnaire.ChemPeel;
                existing.LastChemPeelDate = questionnaire.LastChemPeelDate;
                existing.HairRemoval = questionnaire.HairRemoval;
                existing.MedicalConditions = questionnaire.MedicalConditions;
                existing.Allergies = questionnaire.Allergies;
                existing.Botox = questionnaire.Botox;
                existing.NegativeReaction = questionnaire.NegativeReaction;
                existing.SkinType = questionnaire.SkinType;
                await db.SaveChangesAsync();
                return Results.Ok();
            }

            questionnaire.ClientId = request.ClientId;
            await db.SkincareHistoryQuestionnaires.AddAsync(questionnaire);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        consentForm.MapPost("/emergencyContact", async ([FromBody] EmergencyContactRequest request, KimmyEsthiDbContext db) =>
        {
            var client = await db.Clients.FindAsync(request.ClientId);
            if (client is null)
            {
                return Results.NotFound("Client not found");
            }
            var emergencyContact = new EmergencyContact
            {
                ClientId = request.ClientId,
                Name = request.EmergencyContact.Name,
                Phone = request.EmergencyContact.Phone,
                Relationship = request.EmergencyContact.Relationship
            };
            await db.EmergencyContacts.AddAsync(emergencyContact);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        consentForm.MapPost("/consentAndAcknowledgement", async ([FromBody] ConsentAndAcknowledgementRequest request, KimmyEsthiDbContext db) =>
        {
            var client = await db.Clients.FindAsync(request.ClientId);
            if (client is null)
            {
                return Results.NotFound("Client not found");
            }
            var consentAndAcknowledgement = new ConsentAndAcknowledgement
            {
                ClientId = request.ClientId,
                Signature = request.Signature
            };
            await db.ConsentAndAcknowledgements.AddAsync(consentAndAcknowledgement);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        consentForm.MapPost("/products", async ([FromBody] ProductsUsedRequest request, KimmyEsthiDbContext db) =>
        {
            var client = await db.Clients.FindAsync(request.ClientId);
            if (client is null)
            {
                return Results.NotFound("Client not found");
            }
            var productsUsed = new ProductsUsed
            {
                ClientId = request.ClientId,
                Products = request.Products
            };
            await db.ProductsUsed.AddAsync(productsUsed);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }
}

public class SkincareHistoryRequest
{
    public Guid ClientId { get; set; }
    public required SkincareHistoryQuestionnaire SkincareHistoryQuestionnaire { get; set; }
}

public class EmergencyContactRequest
{
    public Guid ClientId { get; set; }
    public required EmergencyContactDetails EmergencyContact { get; set; }
}

public class EmergencyContactDetails
{
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required string Relationship { get; set; }
}

public class ConsentAndAcknowledgementRequest
{
    public Guid ClientId { get; set; }
    public required string Signature { get; set; }
}

public class ProductsUsedRequest
{
    public Guid ClientId { get; set; }
    public required string Products { get; set; }
}
