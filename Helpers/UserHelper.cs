namespace StarWarsApi.Helpers;

public static class UserHelper
{
    public static bool TryGetUserId(HttpContext context, out int userId)
    {
        if (context.Request.Headers.TryGetValue("X-User-Id", out var idStr)
            && int.TryParse(idStr, out userId))
        {
            return true;
        }

        userId = 0;
        return false;
    }
}
