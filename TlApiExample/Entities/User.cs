using System;
namespace TlApiExample.Entities
{
    public class User
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
