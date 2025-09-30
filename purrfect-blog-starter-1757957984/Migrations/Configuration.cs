using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using purrfect_blog_starter_1757957984.Models;

namespace purrfect_blog_starter_1757957984.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<purrfect_blog_starter_1757957984.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(purrfect_blog_starter_1757957984.Models.ApplicationDbContext context)
        {
            string Hash(string password)
            {
                using (var sha = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(password);
                    var hash = sha.ComputeHash(bytes);
                    return Convert.ToBase64String(hash);
                }
            }

            // Add or update by Username to avoid duplicates if Seed runs again
            context.Users.AddOrUpdate(u => u.Username,
                new User { Username = "siamese",   Email = "siamese@example.com",   PasswordHash = Hash("Password1!") },
                new User { Username = "russianblue",     Email = "russianblue@example.com",     PasswordHash = Hash("Password2!") },
                new User { Username = "mainecoon", Email = "mainecoon@example.com", PasswordHash = Hash("Password3!") }
            );
        }
    }
}
