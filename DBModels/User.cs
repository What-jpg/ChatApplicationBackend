using System.Diagnostics.CodeAnalysis;

namespace ChatApplicationApi.DBModels
{
    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string? Picture { get; set; }

        public List<Chat> Chats { get; set; } = new List<Chat> {};
    }
}
