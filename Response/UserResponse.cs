using Microsoft.AspNetCore.Mvc;

namespace NoteApplication.Response
{
    public class UserResponse : ControllerBase
    {
        public ActionResult JsonResponse(int statusCode, string message, Object user = null)
        {
            if (statusCode == 303)
            {
                return StatusCode(303, new { success = false, message = "This email address is already existed" });
            }
            else if (statusCode == 404)
            {
                return NotFound(new { success = false, message = "User not found" });
            }
            else if (statusCode == 200)
            {
                return Ok(new { success = true, message, user });
            }
            else if (statusCode == 201)
            {
                return CreatedAtRoute("RegisterUser", new { success = true, message });
            }
            else if (statusCode == 401)
            {
                return StatusCode(401, new { success = true, message = "Incorrect email or password" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Something went wrong. Please try again later." });
            }
        }
    }
}
