namespace Translator.Infrastructure.Database.Postgres.Configurations.Constants;

public partial class DatabaseConstants
{
    public class User
    {
        public const int USERNAME_MIN_LENGTH = 3;
        public const int USERNAME_MAX_LENGTH = 20;
        public const int SECRET_KEY_LENGTH = 16;
    }
    
}