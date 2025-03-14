using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NoteApplication.Data;
using NoteApplication.Models.Entities;

namespace NoteApplication.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public IEnumerable<User> GetAllUsers()
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                return db.Query<User>("SELECT * FROM Users").ToList();
            }
        }

        public bool CreateUser(User newUser)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                // Check if the user already exists
                var existingUser = db.QueryFirstOrDefault<User>(
                    "SELECT * FROM Users WHERE Email = @Email",
                    new { Email = newUser.email }
                );
                if (existingUser != null)
                {
                    return false;
                }

                // Hash password before storing
                newUser.password = HashPassword(newUser.password);

                string queryString = "INSERT INTO Users (name, email, password, created_at) VALUES (@name, @email, @password, GETDATE())";
                int rowsAffected = db.Execute(queryString, newUser);

                return rowsAffected > 0;
            }
        }

        public User? GetUser(User user)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                var ExisitingUser = db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE email = @Email", new { Email = user.email });

                if(ExisitingUser != null && VerifyPassword(user.password, ExisitingUser.password))
                {
                    return ExisitingUser;
                }

                return null;
            }
        }
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Helper method to verify hashed password
        private static bool VerifyPassword(string password, string storedHash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == storedHash;
        }
    }
}
