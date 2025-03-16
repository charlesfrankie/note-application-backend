using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NoteApplication.Data;
using NoteApplication.Models.Entities;
using NoteApplication.Controllers.Requests;
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

        public object GetAllNotes(NoteRequest queryParams)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                var allowedColumns = new HashSet<string> { "Id ASC", "Id DESC", "title asc", "title desc", "created_at asc", "created_at desc" };

                if (!allowedColumns.Contains(queryParams.OrderBy))
                {
                    queryParams.OrderBy = "Id DESC"; // Default sorting column if invalid input
                }

                // Construct WHERE clause for filtering
                var conditions = new List<string>();
                var parameters = new DynamicParameters();
                
                if (!string.IsNullOrWhiteSpace(queryParams.FilterBy))
                {
                    conditions.Add(queryParams.FilterBy.Trim());
                }

                if (!string.IsNullOrWhiteSpace(queryParams.Search))
                {
                    conditions.Add("(title LIKE @Search OR content LIKE @Search)");
                    parameters.Add("Search", $"%{queryParams.Search.Trim()}%");
                }

                string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
                string query = $@"
                    SELECT * FROM Note
                    {whereClause}
                    ORDER BY {queryParams.OrderBy}
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(*) FROM Note {whereClause};";

                parameters.Add("Offset", (queryParams.Page - 1) * queryParams.PerPage);
                parameters.Add("PageSize", queryParams.PerPage);

                using (var multi = db.QueryMultiple(query, parameters))
                {
                    var notes = multi.Read<Note>().ToList();
                    var totalCount = multi.Read<int>().FirstOrDefault();
                    var totalPages = (int)Math.Ceiling((double)totalCount / (queryParams.PerPage ?? 1));

                    return new { Notes = notes, TotalCount = totalCount, TotalPage = totalPages };
                }
            }
        }

        public (int statusCode, Note note) GetNoteById(int id, int UserId)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                var note = db.QueryFirstOrDefault<Note>(
                    "SELECT * FROM Note WHERE Id = @Id",
                    new { Id = id }
                );
                if (note == null)
                {
                    return (404, note);
                }

                if (note.UserId != UserId)
                {
                    return ( 403, note );
                }

                return ( 200, note );
            }
        }

        public (int statusCode, Note note) AddNote(Note newNote)
        {
            using (IDbConnection db = _context.Database.GetDbConnection())
            {
                string queryString = @"INSERT INTO Note (Title, Content, UserId, created_at) 
                                        OUTPUT INSERTED.Id
                                        VALUES (@Title, @Content, @UserId, GETDATE())";
                int createdId = db.QuerySingle<int>(queryString, newNote);
                var note = db.QueryFirstOrDefault<Note>(
                    "SELECT * FROM Note WHERE Id = @Id",
                    new { Id = createdId }
                );
                if (note == null)
                {
                    return (404, note);
                }

                return (200, note);
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