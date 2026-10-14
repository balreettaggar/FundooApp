using Models;
using RepositoryLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface INotesBL
{
    Task<ResponseModel<NoteEntity>> AddNotes(NotesCreationModel model, int userId);

    Task<ResponseModel<List<NoteEntity>>> ViewNotes(int userId);

    Task<ResponseModel<NoteEntity>> ViewNotesById(int id, int userId);

    Task<ResponseModel<NoteEntity>> EditNote(int id, EditNotesModel model, int userId);

    Task<ResponseModel<bool>> DeleteNote(int id, int userId);

    Task<ResponseModel<bool>> ToggleArchive(int id, int userId);

    Task<ResponseModel<bool>> ToggleTrash(int id, int userId);
}
