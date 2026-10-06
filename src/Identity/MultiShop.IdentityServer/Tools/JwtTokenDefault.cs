namespace MultiShop.IdentityServer.Tools
{
    public class JwtTokenDefault
    {
        public const string ValidAudience = "http://localhost";

        public const string ValidIssuer = "http://localhost";

        public static string Key { get; set; } = string.Empty;

        public const int Expire = 60;
    }
}
