using System.ComponentModel.DataAnnotations;
using SafeVault.Models;

namespace SafeVault.Services;

public static class RegistrationValidator
{
    public static bool IsValid(string username, string email, string password)
    {
        var model = new RegisterViewModel
        {
            Username = username,
            Email = email,
            Password = password
        };
        var context = new ValidationContext(model);
        return Validator.TryValidateObject(model, context, null, validateAllProperties: true);
    }
}