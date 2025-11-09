namespace Zenflow.Helpers
{
    public static class CookieConfig
    {
        public static CookieOptions Default(double? expires = null)
        {

            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires.HasValue ? DateTimeOffset.UtcNow.AddSeconds(expires.Value) : DateTimeOffset.UtcNow.AddHours(1),
            };
        }
    }
}
