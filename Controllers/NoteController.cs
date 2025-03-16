using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteApplication.Models.Entities;
using NoteApplication.Repositories;
using NoteApplication.Response;
using NoteApplication.Controllers.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace NoteApplication.Controllers
{
    [ApiController]
    [Route("api/notes")]
    public class NoteController : ControllerBase
    {
        private readonly NoteRepository _noteRepository;
        public NoteController(NoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
            
        }

        private int getAuthUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpGet("")]
        public ActionResult<IEnumerable<Note>> GetNotes([FromQuery] NoteRequest queryParams)
        {
            queryParams.PerPage = queryParams.PerPage ?? 10;
            queryParams.Page = queryParams.Page ?? 1;
            queryParams.Search = queryParams.Search ?? null;
            queryParams.OrderBy = !string.IsNullOrEmpty(queryParams.OrderBy) ? queryParams.OrderBy.Replace("-", " ") : "Id DESC";
            
            if(!string.IsNullOrEmpty(queryParams.FilterBy) && queryParams.FilterBy == "my_post")
            {
                queryParams.FilterBy = $"UserId = {getAuthUserId()}";
            } else
            {
                queryParams.FilterBy = null;
            }

            var result = _noteRepository.GetAllNotes(queryParams);
            return Ok(new { message = "Get notes successfully", result });
        }

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<Note>> GetNoteById(int id)
        {
            int UserId = getAuthUserId();
            if (UserId == 0)
            {
                return Unauthorized("User not found in token.");
            }

            var (result, note) = _noteRepository.GetNoteById(id, UserId);
            return new NoteResponse().JsonResponse(result, "Get note successfully!", note);
        }

        [Authorize]
        [HttpPost("create", Name = "CreateNote")]
        public ActionResult<Note> AddNote([FromBody] Note newNote)
        {
            if (newNote == null || string.IsNullOrEmpty(newNote.Title))
            {
                return BadRequest("Note title is required.");
            }

            int UserId = getAuthUserId();
            if (UserId == 0)
            {
                return Unauthorized("User not found in token.");
            }
            newNote.UserId = UserId;

            var (result, note) = _noteRepository.AddNote(newNote);
            return new NoteResponse().JsonResponse(result, "Note created successfully!", note);
        }

        [Authorize]
        [HttpPut("update/{id:int}")]
        public ActionResult<IEnumerable<Note>> UpdateNote(int id, [FromBody] Note updatedNote)
        {
            if (updatedNote == null || string.IsNullOrEmpty(updatedNote.Title))
            {
                return BadRequest("Note title is required.");
            }

            int UserId = getAuthUserId();
            if (UserId == 0)
            {
                return Unauthorized("User not found in token.");
            }
            updatedNote.UserId = UserId;

            int statusCode = _noteRepository.UpdateNote(id, updatedNote);
            return new NoteResponse().JsonResponse(statusCode, "Note update successfully!");
        }

        [Authorize]
        [HttpDelete("delete/{id:int}")]
        public ActionResult<IEnumerable<Note>> DeleteNote(int id)
        {
            int UserId = getAuthUserId();
            if (UserId == 0)
            {
                return Unauthorized("User not found in token.");
            }

            int statusCode = _noteRepository.DeleteNote(id, UserId);
            return new NoteResponse().JsonResponse(statusCode, "Note deleted successfully!");
        }
    }
}
