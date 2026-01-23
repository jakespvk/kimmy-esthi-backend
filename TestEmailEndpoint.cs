using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using KimmyEsthi.Email;

public static class TestEmailEndpoint
{
    public static void Map(WebApplication app)
    {
        var tests = app.MapGroup("tests");
        tests.MapPost("/test-email", async ([FromBody] TestEmailRequest request, EmailService emailService) =>
        {
            var result = await emailService.SendAppointmentRequestEmail(
                Guid.NewGuid(),
                Guid.NewGuid(),
                request.Email
            );

            return result ? Results.Ok("Email sent successfully") : Results.Problem("Failed to send email");
        });
    }
}

public class TestEmailRequest
{
    public string Email { get; set; } = string.Empty;
}
