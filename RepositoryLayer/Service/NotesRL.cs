using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Models;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Service;

public class NotesRL : INotesRL
{
    //AddNotes
    //ViewNotes
    //ViewNotesById
    //EditNote
    //DeleteNote
    //ArchUnarchieved
    //TrashUnTrash
    public FundooContext context { get; set; }

    public NotesRL(FundooContext context)
    {
        this.context = context;
    }

    public async Task<NoteEntity> AddNotes(NoteEntity note)
    {
        context.Notes.Add(note);
        await context.SaveChangesAsync();

        return note;
    }

    public async Task<List<NoteEntity>> ViewNotes(int userId)
    {
        return await context.Notes
            .Where(n => n.UserId == userId)
            .ToListAsync();
    }



    public async Task<NoteEntity?> ViewNotesById(int id, int userId)
    {
        return await context.Notes
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId &&
                !n.isDeleted);
    }

    public async Task<NoteEntity?> EditNote(
        NoteEntity updatedNote, int userId)
    {
        var note = await context.Notes
            .FirstOrDefaultAsync(n =>
                n.Id == updatedNote.Id &&
                n.UserId == userId &&
                !n.isDeleted);

        if (note == null)
            return null;

        note.Title = updatedNote.Title;
        note.Description = updatedNote.Description;
        note.Color = updatedNote.Color;

        await context.SaveChangesAsync();

        return note;
    }

    public async Task<bool> DeleteNote(int id, int userId)
    {
        var note = await context.Notes
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId &&
                !n.isDeleted);

        if (note == null)
            return false;

        // Soft delete: move note to trash
        note.isDeleted = true;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleArchive(int id, int userId)
    {
        var note = await context.Notes
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId &&
                !n.isDeleted);

        if (note == null)
            return false;

        note.IsArchieved = !note.IsArchieved;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleTrash(int id, int userId)
    {
        var note = await context.Notes
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UserId == userId);

        if (note == null)
            return false;

        note.isDeleted = !note.isDeleted;

        await context.SaveChangesAsync();
        return true;
    }



}
