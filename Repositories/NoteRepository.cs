using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NoteApplication.Data;
using NoteApplication.Models.Entities;
using static Azure.Core.HttpHeader;

namespace NoteApplication.Repositories
{
    public class NoteRepository
    {
        private readonly ApplicationDBContext _context;

        public NoteRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public object GetAllNotes(int pageNumber, int pageSize, string orderBy)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                var allowedColumns = new HashSet<string> { "Id ASC", "Id DESC", "title asc", "title desc", "created_at asc", "created_at desc" };

                if (!allowedColumns.Contains(orderBy))
                {
                    orderBy = "Id DESC"; // Default sorting column if invalid input
                }

                string query = $@"
                    SELECT * FROM Note
                    ORDER BY {orderBy}
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM Note;";

                var parameters = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };

                using (var multi = db.QueryMultiple(query, parameters))
                {
                    var notes = multi.Read<Note>().ToList();
                    var totalCount = multi.Read<int>().FirstOrDefault();
                    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    return new { Notes = notes, TotalCount = totalCount, TotalPage = totalPages };
                }
            }
        }

        public Note? GetNoteById(int id)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                return db.QueryFirstOrDefault<Note>(
                    "SELECT * FROM Note WHERE Id = @Id",
                    new { Id = id }
                );
            }
        }

        public int AddNote(Note newNote)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                string queryString = "INSERT INTO Note (Title, Content, UserId, created_at) VALUES (@Title, @Content, @UserId, GETDATE())";
                int rowsAffected = db.Execute(queryString, newNote);
                return rowsAffected > 0 ? 200 : 0;
            }
        }

        public int UpdateNote(int id, Note updatedNote)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                // Check if the note exists
                var existingNote = db.QueryFirstOrDefault<Note>("SELECT * FROM Note WHERE Id = @Id", new { Id = id });
                if (existingNote == null)
                {
                    return 404;
                }

                if (existingNote.UserId != updatedNote.UserId)
                {
                    return 403;
                }

                updatedNote.Updated_at = DateTime.Now;
                string queryString = "UPDATE Note SET Title = @Title, Content = @Content, Updated_at = @Updated_at WHERE Id = @Id";

                int rowsAffected = db.Execute(queryString, new { updatedNote.Title, updatedNote.Content, updatedNote.Updated_at, Id = id });
                return rowsAffected > 0 ? 201 : 0;
            }
        }

        public int DeleteNote(int id, int UserId)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                var note = db.QueryFirstOrDefault<Note>("SELECT * FROM Note WHERE Id = @Id", new { Id = id });
                if (note == null)
                {
                    return 404;
                }

                if (note.UserId != UserId)
                {
                    return 403;
                }

                string queryString = "DELETE FROM Note WHERE Id = @Id";
                int rowsAffected = db.Execute(queryString, new { Id = id });
                return rowsAffected > 0 ? 200 : 0;
            }
        }
    }
}