using System.Text.Json.Serialization;

namespace NotesApp.API.Modules.Auth.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role
    {
        User = 0,
        Admin = 1
    }
}

