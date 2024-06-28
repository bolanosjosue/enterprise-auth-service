using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using BCrypt.Net;

namespace AuthService.Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var adminUser = User.Create(
            "admin@authservice.com",
            BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            "System Administrator",
            UserRole.Admin
        );

        var managerUser = User.Create(
            "manager@authservice.com",
            BCrypt.Net.BCrypt.HashPassword("Manager123!"),
            "John Manager",
            UserRole.Manager
        );

        var regularUser = User.Create(
            "user@authservice.com",
            BCrypt.Net.BCrypt.HashPassword("User123!"),
            "Jane User",
            UserRole.User
        );

        var guestUser = User.Create(
            "guest@authservice.com",
            BCrypt.Net.BCrypt.HashPassword("Guest123!"),
            "Guest User",
            UserRole.Guest
        );

        context.Users.AddRange(adminUser, managerUser, regularUser, guestUser);

        await context.SaveChangesAsync();
    }
}