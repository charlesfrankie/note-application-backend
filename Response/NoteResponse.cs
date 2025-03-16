using Microsoft.AspNetCore.Mvc;

namespace NoteApplication.Response
{
    public class NoteResponse : ControllerBase
    {
        public ActionResult JsonResponse(int statusCode, string message, Object data = null)
        {
            if (statusCode == 403)
            {
                return StatusCode(403, new { success = false, message = "Permission denied! You're not the owner of this note." });
            }
            else if (statusCode == 404)
            {
                return NotFound(new { success = false, message = "Note not found" });
            }
            else if (statusCode == 200)
            {
                return Ok(new { success = true, message, data });
            }
            else if (statusCode == 201)
            {
                return CreatedAtRoute("CreateNote", new { success = true, message });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Something went wrong. Please try again later." });
            }
        }
    }
}
