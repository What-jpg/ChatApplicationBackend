using ChatApplicationApi.DBModels;

namespace ChatApplicationApi.ModelClasses
{
    public class ClientMessage
    {
        public long Id { get; set; }

        public string Content { get; set; }

        public string Type { get; set; }

        public string UserIdWhoSend { get; set; }

        public User UserWhoSend { get; set; }

        public DateTime DateTime { get; set; }

        public long ChatId { get; set; }

        public Chat Chat { get; set; }
    }
}