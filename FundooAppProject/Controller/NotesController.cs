using System.Security.Claims;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using RepositoryLayer.Entity;

namespace Fundoo.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INotesBL notesBL;

    private int? CurrentUserId =>
        int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var userId)
            ? userId
            : null;

    public NotesController(INotesBL notesBL)
    {
        this.notesBL = notesBL;
    }

    [HttpPost]
    public async Task<IActionResult> AddNotes(NotesCreationModel model)
    {
        if (CurrentUserId is not int userId)
        {
            return Unauthorized(new ResponseModel<NoteEntity>(
                false,
                "Invalid user identity.",
                null!
            ));
        }

        var response = await notesBL.AddNotes(model, userId);

        if (!response.IsSuccess)
        {
            return BadRequest(response);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpGet]
    public async Task<IActionResult> ViewNotes()
    {
        if (CurrentUserId is not int userId)
        {
            return Unauthorized(new ResponseModel<NoteEntity>(
                false,
                "Invalid user identity.",
                null!
            ));
        }

        var response = await notesBL.ViewNotes(userId);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ViewNotesById(int id)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();

        var response =
            await notesBL.ViewNotesById(id, userId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> EditNote(
        int id, [FromBody] EditNotesModel model)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();

        var response =
            await notesBL.EditNote(id, model, userId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();

        var response =
            await notesBL.DeleteNote(id, userId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPatch("{id:int}/archive")]
    public async Task<IActionResult> ToggleArchive(int id)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();

        var response =
            await notesBL.ToggleArchive(id, userId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPatch("{id:int}/trash")]
    public async Task<IActionResult> ToggleTrash(int id)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();

        var response =
            await notesBL.ToggleTrash(id, userId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }
}