namespace RepositoryLayer.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepositoryLayer.Entity;

public interface INotesRL
{
    Task<NoteEntity> AddNotes(NoteEntity note);
    Task<List<NoteEntity>> ViewNotes(int userId);
    Task<NoteEntity?> ViewNotesById(int id, int userId);
    Task<NoteEntity?> EditNote(NoteEntity updatedNote, int userId);
    Task<bool> DeleteNote(int id, int userId);
    Task<bool> ToggleArchive(int id, int userId);
    Task<bool> ToggleTrash(int id, int userId);
}