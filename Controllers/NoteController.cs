using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteApplication.Models.Entities;
using NoteApplication.Repositories;
using NoteApplication.Response;
using System.Security.Claims;

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

        [HttpGet("")]
        public ActionResult<IEnumerable<Note>> GetNotes([FromQuery(Name = "per_page")] int? perPage, [FromQuery(Name = "page")] int? page, [FromQuery(Name = "order_by")] string? order_by)
        {  
            perPage = perPage ?? 10;
            page = page ?? 1;
            order_by = !string.IsNullOrEmpty(order_by) ? order_by.Replace("-", " ") : "Id DESC";
            var result = _noteRepository.GetAllNotes(page.Value, perPage.Value, order_by);
            
            return Ok(new { message = "Get notes successfully", result });
        }
        
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<Note>> GetNoteById(int id)
        {
            var note = _noteRepository.GetNoteById(id);
            return Ok(note);
        }

        [Authorize]
        [HttpPost("create", Name = "CreateNote")]
        public ActionResult<Note> AddNote([FromBody] Note newNote)
        {
            if (newNote == null || string.IsNullOrEmpty(newNote.Title))
            {
                return BadRequest("Note title is required.");
            }

            // Get the UserId from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User not found in token.");
            }
            // Assign UserId to the note
            newNote.UserId = int.Parse(userIdClaim.Value);

            int statusCode = _noteRepository.AddNote(newNote);
            return new NoteResponse().JsonResponse(statusCode, "Note created successfully!");
        }

        [Authorize]
        [HttpPut("update/{id:int}")]
        public ActionResult<IEnumerable<Note>> UpdateNote(int id, [FromBody] Note updatedNote)
        {
            if (updatedNote == null || string.IsNullOrEmpty(updatedNote.Title))
            {
                return BadRequest("Note title is required.");
            }

            // Get the UserId from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User not found in token.");
            }
            // Assign UserId to the note
            updatedNote.UserId = int.Parse(userIdClaim.Value);

            int statusCode = _noteRepository.UpdateNote(id, updatedNote);

            return new NoteResponse().JsonResponse(statusCode, "Note update successfully!");
        }

        [Authorize]
        [HttpDelete("delete/{id:int}")]
        public ActionResult<IEnumerable<Note>> DeleteNote(int id)
        {
            // Get the UserId from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User not found in token.");
            }
            // Assign UserId to the note
            int UserId = int.Parse(userIdClaim.Value);

            int statusCode = _noteRepository.DeleteNote(id, UserId);

            return new NoteResponse().JsonResponse(statusCode, "Note deleted successfully!");
        }
    }
}
