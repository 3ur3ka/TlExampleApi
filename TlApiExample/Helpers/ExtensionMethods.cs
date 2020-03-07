using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    // This helper function allows us to run async methods in a safe synchronous way
    public static class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        public static void RunSync(Func<Task> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
    }
}