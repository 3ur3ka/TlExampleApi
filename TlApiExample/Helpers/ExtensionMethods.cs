using System;
using TlApiExample.Entities;

namespace TlApiExample.Helpers
{
    public static class Extensions
    {
        public static User RemoveSensitiveFields(this User user)
        {
            user.PasswordHash = null;
            user.PasswordSalt = null;
            return user;
        }
    }
}