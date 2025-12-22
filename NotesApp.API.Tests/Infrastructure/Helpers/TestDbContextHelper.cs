using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.DBContext;

namespace NotesApp.API.Tests.Infrastructure.Helpers;

public static class TestDbContextHelper
{
    public static NoteDBContext CreateInMemoryContext(string? databaseName = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NoteDBContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString());
        return new NoteDBContext(optionsBuilder.Options);
    }
}

