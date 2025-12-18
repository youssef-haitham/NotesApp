using System.Text.Json.Serialization;

namespace NotesApp.API.Modules.Notes.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Colors
    {
        YELLOW,
        BLUE,
        GREY
    }
}