namespace Zenflow.Helpers
{
    public static class EnvConfig
    {
        public static readonly string AuthDomain =
            Environment.GetEnvironmentVariable("AUTH_DOMAIN") ?? "domain";
        public static readonly string AuthClientId =
            Environment.GetEnvironmentVariable("AUTH_CLIENT_ID") ?? "id";
        public static readonly string AuthClientSecret =
            Environment.GetEnvironmentVariable("AUTH_CLIENT_SECRET") ?? "secret";
        public static readonly string AuthAudience =
            Environment.GetEnvironmentVariable("AUTH_AUDIENCE") ?? "audience";
        public static readonly string AuthConnection =
            Environment.GetEnvironmentVariable("AUTH_CONNECTION_NAME")
            ?? "Username-Password-Authentication";
        public static readonly string AuthGrantType =
            Environment.GetEnvironmentVariable("AUTH_GRANT_TYPE") ?? "grant my wish";
        public static readonly string AuthJwt =
            Environment.GetEnvironmentVariable("AUTH_JWT_TOKEN") ?? "access_token";
        public static readonly string AuthRefreshJwt =
            Environment.GetEnvironmentVariable("AUTH_REFRESH_JWT_TOKEN") ?? "refresh_token";

        public static readonly Uri AuthConnectUri = new Uri(
            $"https://{AuthDomain}/dbconnections/signup"
        );
        public static readonly Uri AuthTokenUri = new Uri($"https://{AuthDomain}/oauth/token");
        public static readonly Uri AuthUserInfoUri = new Uri($"https://{AuthDomain}/userinfo");

        public static readonly string DbConectionString =
            Environment.GetEnvironmentVariable("DB_CONNECTION") ?? "string";

        public static readonly string TinkClientId =
            Environment.GetEnvironmentVariable("TINK_CLIENT_ID") ?? "id";
        public static readonly string TinkClientSecret =
            Environment.GetEnvironmentVariable("TINK_CLIENT_SECRET") ?? "secret";
        public static readonly string TinkApi =
            Environment.GetEnvironmentVariable("TINK_API_LINK") ?? "api";
        public static readonly string TinkJwt =
            Environment.GetEnvironmentVariable("TINK_JWT_TOKEN") ?? "other_bank_token";
        public static readonly string TinkClientJwt =
    Environment.GetEnvironmentVariable("TINK_CLIENT_JWT_TOKEN") ?? "other_client_bank_token";
        public static readonly string TinkGrantType =
            Environment.GetEnvironmentVariable("TINK_GRANT_TYPE") ?? "grant my wish";
        public static readonly string TinkActorClientId = Environment.GetEnvironmentVariable("TINK_ACTOR_CLIENT_ID") ?? "actor_client_id";


        public static readonly Uri TinkTokentUri = new Uri($"{TinkApi}/api/v1/oauth/token");
        public static readonly Uri TinkUserId = new Uri($"{TinkApi}/api/v1/user");
        public static readonly Uri TinkCreateUser = new Uri($"{TinkApi}/api/v1/user/create");
        public static readonly Uri TinkUserAuthDelegateCode = new Uri($"{TinkApi}/api/v1/oauth/authorization-grant/delegate");
        public static readonly Uri TinkUserAuthCode = new Uri($"{TinkApi}/api/v1/oauth/authorization-grant");
        public static readonly Uri TinkListAccountUri = new Uri($"{TinkApi}/data/v2/accounts");
        public static readonly Uri TinkListTransactionUri = new Uri($"{TinkApi}/api/v1/search");
        public static readonly Uri TinkRedirectUri = new Uri("http://localhost:5173/connect-bank");

        public static Uri TinkGetBalanceUri(string accountId)
        {
            return new Uri($"{TinkApi}/api/v1/accounts/{accountId}/balances");
        }

        public static Uri TinkAccountCheck(string accountId)
        {
            return new Uri($"{TinkApi}/api/v1/account-verification-reports/{accountId}");
        }
    }
}
