using System.Text.RegularExpressions;

namespace SAClient.Classes
{
	public static class Utility
	{
		public static bool Matches(this string str, string pattern)
		{
			Regex regex = new Regex(pattern);
			return regex.Match(str).Success;
		}
	}
}
