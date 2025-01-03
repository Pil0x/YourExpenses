using System.Security.Claims;

namespace YourExpenses.Helpers
{
    public static class UserHelpers
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static bool IsUserAuthorized(this ClaimsPrincipal user)
        {
            return user.GetUserId() != null;
        }
    }
}
