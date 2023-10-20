namespace ChatApplicationApi.ModelClasses
{
    public class Auth0UserInfo
    {
        public string Sub { get; set; }

        public string Name { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; } = null;

        public string? Picture { get; set; } = null;

        public string? NickName { get; set; } = null;

        public string? Error { get; set; } = null;
    }
}
