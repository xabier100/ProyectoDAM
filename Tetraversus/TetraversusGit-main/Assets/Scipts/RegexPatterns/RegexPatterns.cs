namespace Scipts.RegexPatterns
{
    public class RegexPatterns
    {
        public const string Username = @"^[a-zA-Z0-9_-]{6,16}$";
        public const string Password = @".{6,}";
        public const string Email = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
    }
}