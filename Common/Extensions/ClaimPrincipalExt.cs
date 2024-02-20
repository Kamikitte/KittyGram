using System.Security.Claims;

namespace Common.Extensions;

public static class ClaimPrincipalExt
{
	public static T? GetClaimValue<T>(this ClaimsPrincipal user, string claim)
	{
		var value = user.Claims.FirstOrDefault(x => x.Type == claim)?.Value;
		return value != null ? value.Convert<T>() : default;
	}
}