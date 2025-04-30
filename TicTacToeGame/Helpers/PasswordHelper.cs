using Microsoft.AspNetCore.Identity;
using TicTacToeGame.Models;

namespace TicTacToeGame.Helpers;

public class PasswordHelper
{
    public static PasswordHasher<AppUser> PasswordHasher { get; private set; }

    static PasswordHelper()
    {
        PasswordHasher = new PasswordHasher<AppUser>();
    }

    public static string HashPassword(AppUser user, string password)
    {
        return PasswordHasher.HashPassword(user, password);
    }

    public static bool VerifyPassword(AppUser user, string password, string hashedPassword)
    {
        var result = PasswordHasher.VerifyHashedPassword(user, hashedPassword, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}