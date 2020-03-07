using System.Collections.Generic;
using Newtonsoft.Json;
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

        // Simple but convoluted way to convert POCO into Dictionary
        public static Dictionary<string, string> ToDict<T>(this T target)
            => target is null
                ? new Dictionary<string, string>()
                : JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        JsonConvert.SerializeObject(target)
                  );
    }
}