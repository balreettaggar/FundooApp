using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BusinessLayer.Interface;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service;

public class NotesBL : INotesBL
{
    public INotesRL notesRL;

    public NotesBL(INotesRL notes)
    {
        notesRL = notes;
    }
    public async Task<ResponseModel<NoteEntity>> AddNotes(NotesCreationModel model, int userId)
    {
        var note = new NoteEntity
        {
            Title = model.Title,
            Description = model.Description,
            Color = model.Colour,
            UserId = userId
        };

        var createdNote = await notesRL.AddNotes(note);

        return new ResponseModel<NoteEntity>
        {
            IsSuccess = true,
            Message = "Note created successfully.",
            Data = createdNote
        };
    }

    public async Task<ResponseModel<List<NoteEntity>>> ViewNotes(int userId)
    {
        var notes = await notesRL.ViewNotes(userId);

        return new ResponseModel<List<NoteEntity>>(
            true,
            notes.Count == 0
                ? "No notes found."
                : "Notes retrieved successfully.",
            notes
        );
    }

    public async Task<ResponseModel<NoteEntity>> ViewNotesById(
        int id, int userId)
    {
        var note = await notesRL.ViewNotesById(id, userId);

        if (note == null)
        {
            return new ResponseModel<NoteEntity>(
                false, "Note not found.", null!);
        }

        return new ResponseModel<NoteEntity>(
            true, "Note retrieved successfully.", note);
    }

    public async Task<ResponseModel<NoteEntity>> EditNote(
        int id, EditNotesModel model, int userId)
    {
        var updatedNote = new NoteEntity
        {
            Id = id,
            Title = model.Title,
            Description = model.Description,
            Color = model.Colour
        };

        var note = await notesRL.EditNote(updatedNote, userId);

        if (note == null)
        {
            return new ResponseModel<NoteEntity>(
                false, "Note not found.", null!);
        }

        return new ResponseModel<NoteEntity>(
            true, "Note updated successfully.", note);
    }

    public async Task<ResponseModel<bool>> DeleteNote(
        int id, int userId)
    {
        var deleted = await notesRL.DeleteNote(id, userId);

        if (!deleted)
        {
            return new ResponseModel<bool>(
                false, "Note not found.", false);
        }

        return new ResponseModel<bool>(
            true, "Note moved to trash successfully.", true);
    }

    public async Task<ResponseModel<bool>> ToggleArchive(
        int id, int userId)
    {
        var updated = await notesRL.ToggleArchive(id, userId);

        if (!updated)
        {
            return new ResponseModel<bool>(
                false, "Note not found.", false);
        }

        return new ResponseModel<bool>(
            true, "Archive status updated successfully.", true);
    }

    public async Task<ResponseModel<bool>> ToggleTrash(
        int id, int userId)
    {
        var updated = await notesRL.ToggleTrash(id, userId);

        if (!updated)
        {
            return new ResponseModel<bool>(
                false, "Note not found.", false);
        }

        return new ResponseModel<bool>(
            true, "Trash status updated successfully.", true);
    }
}