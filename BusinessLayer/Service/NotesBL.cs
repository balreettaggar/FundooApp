using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BusinessLayer.Interface;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BusinessLayer.Service;

public class NotesBL : INotesBL
{
    private sealed class CachedNote
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsArchieved { get; set; }
        public bool IsDeleted { get; set; }
        public int UserId { get; set; }
    }

    public INotesRL notesRL;
    private readonly IDistributedCache cache;

    public NotesBL(INotesRL notes, IDistributedCache cache)
    {
        notesRL = notes;
        this.cache = cache;
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
        await InvalidateNoteIdsAsync(userId);

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

        var noteIds = notes.Select(n => n.Id).ToList();

        await cache.SetStringAsync(
            NoteIdsKey(userId),
            JsonSerializer.Serialize(noteIds),
            CacheOptions);

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
        // 1. Check the user's note-ID index.
        var idsJson = await cache.GetStringAsync(
            NoteIdsKey(userId));

        var noteIds = idsJson == null
            ? new List<int>()
            : JsonSerializer.Deserialize<List<int>>(idsJson)
              ?? new List<int>();

        // 2. If the index contains this ID, check its note cache.
        if (noteIds.Contains(id))
        {
            var cachedJson = await cache.GetStringAsync(
                NoteKey(userId, id));

            if (cachedJson != null)
            {
                var cachedNote =
                    JsonSerializer.Deserialize<CachedNote>(cachedJson);

                if (cachedNote != null &&
                    cachedNote.Id == id &&
                    cachedNote.UserId == userId &&
                    !cachedNote.IsDeleted)
                {
                    return new ResponseModel<NoteEntity>(
                        true,
                        "Note retrieved from cache.",
                        FromCachedNote(cachedNote));
                }
            }
        }

        // 3. Cache miss, missing index, or stale index:
        // ask PostgreSQL, which verifies both ID and ownership.
        var note = await notesRL.ViewNotesById(id, userId);

        if (note == null)
        {
            return new ResponseModel<NoteEntity>(
                false, "Note not found.", null!);
        }

        // 4. Cache only the requested note.
        var json = JsonSerializer.Serialize(ToCachedNote(note));

        await cache.SetStringAsync(
            NoteKey(userId, id),
            json,
            CacheOptions);

        // 5. Update the index if it didn't contain this note ID.
        if (!noteIds.Contains(id))
        {
            noteIds.Add(id);

            await cache.SetStringAsync(
                NoteIdsKey(userId),
                JsonSerializer.Serialize(noteIds),
                CacheOptions);
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
        await InvalidateNoteAsync(userId, id);
        return new ResponseModel<NoteEntity>(
            true, "Note updated successfuly.", note);
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

    private static string NoteKey(int userId, int noteId) =>
    $"Fundoo:user:{userId}:note:{noteId}";

    private static string NoteIdsKey(int userId) =>
        $"Fundoo:user:{userId}:noteIds";

    private static readonly DistributedCacheEntryOptions CacheOptions =
        new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
    private static CachedNote ToCachedNote(NoteEntity note) =>
    new()
    {
        Id = note.Id,
        Description = note.Description,
        Title = note.Title,
        Color = note.Color,
        IsArchieved = note.IsArchieved,
        IsDeleted = note.isDeleted,
        UserId = note.UserId
    };

    private static NoteEntity FromCachedNote(CachedNote note) =>
        new()
        {
            Id = note.Id,
            Description = note.Description,
            Title = note.Title,
            Color = note.Color,
            IsArchieved = note.IsArchieved,
            isDeleted = note.IsDeleted,
            UserId = note.UserId,
            User = null!
        };

    private Task InvalidateNoteAsync(int userId, int noteId)
    {
        return cache.RemoveAsync(NoteKey(userId, noteId));
    }

    private Task InvalidateNoteIdsAsync(int userId)
    {
        return cache.RemoveAsync(NoteIdsKey(userId));
    }
}