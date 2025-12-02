using System;
using System.Text.RegularExpressions;
using ADManagementApp.Models;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Centralized validation service for all AD operations.
    /// Prevents invalid data from reaching Active Directory.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates a username against AD requirements.
        /// </summary>
        /// <param name="username">The username to validate (1-20 alphanumeric characters with -_.)</param>
        /// <returns>Tuple with validation result and error message if invalid</returns>
        (bool IsValid, string ErrorMessage) ValidateUsername(string username);

        /// <summary>
        /// Validates a password against complexity requirements.
        /// </summary>
        /// <param name="password">The password to validate (8-128 characters, uppercase, lowercase, digit, special)</param>
        /// <returns>Tuple with validation result and error message if invalid</returns>
        (bool IsValid, string ErrorMessage) ValidatePassword(string password);

        /// <summary>
        /// Validates an email address format.
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <returns>Tuple with validation result and error message if invalid</returns>
        (bool IsValid, string ErrorMessage) ValidateEmail(string email);

        /// <summary>
        /// Validates a complete ADUser object for creation or update.
        /// </summary>
        /// <param name="user">The user object to validate</param>
        /// <returns>Tuple with validation result and error message if invalid</returns>
        (bool IsValid, string ErrorMessage) ValidateUser(ADUser user);

        /// <summary>
        /// Validates a complete ADGroup object for creation or update.
        /// </summary>
        /// <param name="group">The group object to validate</param>
        /// <returns>Tuple with validation result and error message if invalid</returns>
        (bool IsValid, string ErrorMessage) ValidateGroup(ADGroup group);
    }

    /// <summary>
    /// Implementation of IValidationService with comprehensive validation rules.
    /// </summary>
    public class ValidationService : IValidationService
    {
        // Username: 1-20 characters, alphanumeric, dash, underscore, period
        private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9._-]{1,20}$", RegexOptions.Compiled);

        // Email: Standard email format
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        // Password complexity requirements
        private const int MinPasswordLength = 8;
        private const int MaxPasswordLength = 128;

        public (bool IsValid, string ErrorMessage) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username cannot be empty");

            if (username.Length > 20)
                return (false, "Username must be 20 characters or less");

            if (!UsernameRegex.IsMatch(username))
                return (false, "Username can only contain letters, numbers, hyphens, underscores, and periods");

            // Check for reserved names
            string[] reservedNames = { "admin", "administrator", "root", "system", "guest" };
            if (Array.Exists(reservedNames, name => name.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return (false, $"'{username}' is a reserved name and cannot be used");

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty");

            if (password.Length < MinPasswordLength)
                return (false, $"Password must be at least {MinPasswordLength} characters long");

            if (password.Length > MaxPasswordLength)
                return (false, $"Password must be less than {MaxPasswordLength} characters");

            // Check complexity requirements
            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                    hasUpper = true;
                else if (char.IsLower(c))
                    hasLower = true;
                else if (char.IsDigit(c))
                    hasDigit = true;
                else if (!char.IsLetterOrDigit(c))
                    hasSpecial = true;
            }

            int complexityMet = 0;
            if (hasUpper)
                complexityMet++;
            if (hasLower)
                complexityMet++;
            if (hasDigit)
                complexityMet++;
            if (hasSpecial)
                complexityMet++;

            if (complexityMet < 3)
                return (false, "Password must contain at least 3 of: uppercase, lowercase, numbers, special characters");

            // Check for common weak passwords
            string[] weakPasswords = { "password", "12345678", "qwerty", "admin123" };
            if (Array.Exists(weakPasswords, weak => password.Contains(weak, StringComparison.OrdinalIgnoreCase)))
                return (false, "Password is too common and easily guessable");

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (true, string.Empty); // Email is optional

            if (!EmailRegex.IsMatch(email))
                return (false, "Invalid email format");

            if (email.Length > 256)
                return (false, "Email address is too long");

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateUser(ADUser user)
        {
            // Validate username
            var usernameValidation = ValidateUsername(user.SamAccountName);
            if (!usernameValidation.IsValid)
                return usernameValidation;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.GivenName))
                return (false, "First name is required");

            if (string.IsNullOrWhiteSpace(user.Surname))
                return (false, "Last name is required");

            if (string.IsNullOrWhiteSpace(user.DisplayName))
                return (false, "Display name is required");

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(user.EmailAddress))
            {
                var emailValidation = ValidateEmail(user.EmailAddress);
                if (!emailValidation.IsValid)
                    return emailValidation;
            }

            // Validate field lengths
            if (user.DisplayName.Length > 256)
                return (false, "Display name is too long (max 256 characters)");

            if (user.Description?.Length > 1024)
                return (false, "Description is too long (max 1024 characters)");

            if (user.Department?.Length > 64)
                return (false, "Department name is too long (max 64 characters)");

            if (user.Title?.Length > 128)
                return (false, "Title is too long (max 128 characters)");

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorMessage) ValidateGroup(ADGroup group)
        {
            // Validate group name
            if (string.IsNullOrWhiteSpace(group.Name))
                return (false, "Group name is required");

            if (group.Name.Length > 256)
                return (false, "Group name is too long (max 256 characters)");

            // Validate SAM account name
            var samValidation = ValidateUsername(group.SamAccountName);
            if (!samValidation.IsValid)
                return (false, $"SAM Account Name: {samValidation.ErrorMessage}");

            // Validate description
            if (group.Description?.Length > 1024)
                return (false, "Description is too long (max 1024 characters)");

            // Check for invalid characters in group name
            char[] invalidChars = { '/', '\\', '[', ']', ':', ';', '|', '=', ',', '+', '*', '?', '<', '>' };
            if (group.Name.IndexOfAny(invalidChars) >= 0)
                return (false, "Group name contains invalid characters");

            return (true, string.Empty);
        }
    }
}
