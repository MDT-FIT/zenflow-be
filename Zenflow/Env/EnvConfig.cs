namespace Zenflow.Env
{
    public static class EnvConfig
    {
        public static readonly string AuthDomain = Environment.GetEnvironmentVariable("AUTH_DOMAIN") ?? "domain";
        public static readonly string AuthClientId = Environment.GetEnvironmentVariable("AUTH_CLIENT_ID") ?? "id";
        public static readonly string AuthClientSecret = Environment.GetEnvironmentVariable("AUTH_CLIENT_SECRET") ?? "secret";
        public static readonly string AuthAudience = Environment.GetEnvironmentVariable("AUTH_AUDIENCE") ?? "audience";
        public static readonly string AuthConnection = Environment.GetEnvironmentVariable("AUTH_CONNECTION_NAME") ?? "Username-Password-Authentication";
        public static readonly string AuthToken = Environment.GetEnvironmentVariable("AUTH_JWT_TOKEN") ?? "your-secret-key-min-32-chars-long!";

        public static readonly string DbConectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? "string";

        public static readonly string TinkClientId = Environment.GetEnvironmentVariable("TINK_CLIENT_ID") ?? "id";
        public static readonly string TinkClientSecret = Environment.GetEnvironmentVariable("TINK_CLIENT_SECRET") ?? "secret";
        public static readonly string TinkApi = Environment.GetEnvironmentVariable("TINK_API_LINK") ?? "api";
        public static readonly string TinkJwt = Environment.GetEnvironmentVariable("TINK_JWT_TOKEN") ?? "other_bank_token";
    }
}
