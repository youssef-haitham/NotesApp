using System.Text.Json.Serialization;

namespace NotesApp.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Colors
    {
        YELLOW,
        BLUE,
        GREY
    }
}