using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApplicationApi.DBModels
{
    public class Message
    {
        public long Id { get; set; }

        public byte[] Content { get; set; }

        public string Type { get; set; } = "PlainText";

        public string UserIdWhoSend { get; set; }

        [ForeignKey("UserIdWhoSend")]
        public User UserWhoSend { get; set; }

        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        public long ChatId { get; set; }

        [ForeignKey("ChatId")]
        public Chat Chat { get; set; }
    }
}
