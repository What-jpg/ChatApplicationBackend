namespace ChatApplicationApi.DBModels
{
    public class Chat
    {
        public long Id { get; set; }

        public List<User> Users { get; set; } = new List<User> { };
    }
}
