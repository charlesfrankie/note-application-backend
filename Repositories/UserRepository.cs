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

        public int CreateUser(User newUser)
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
                    return 303;
                }

                // Hash password before storing
                newUser.password = HashPassword(newUser.password);

                string queryString = "INSERT INTO Users (name, email, password, created_at) VALUES (@name, @email, @password, GETDATE())";
                int rowsAffected = db.Execute(queryString, newUser);

                return rowsAffected > 0 ? 201 : 0;
            }
        }

        public (int statusCode, User user) GetUser(User user)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                var foundUser = db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE email = @Email", new { Email = user.email });
                if (foundUser == null)
                {
                    return (404, null);
                }

                if(foundUser != null && VerifyPassword(user.password, foundUser.password))
                {
                    return (200, foundUser);
                }

                return (401, null);
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
