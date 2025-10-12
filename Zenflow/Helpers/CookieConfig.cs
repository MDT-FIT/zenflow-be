namespace FintechStatsPlatform.Helpers
{
    public static class CookieConfig
    {
        public static CookieOptions Default => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        };
    }
}
