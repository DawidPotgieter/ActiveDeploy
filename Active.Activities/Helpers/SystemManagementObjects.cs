using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;
using System.DirectoryServices.ActiveDirectory;

namespace Active.Activities.Helpers
{
	public class SystemManagementObjects
	{
		private static Regex[] excludeUserFilters = new Regex[] { new Regex(@"\$$", RegexOptions.Singleline), new Regex(@"^mcx\d-", RegexOptions.Singleline | RegexOptions.IgnoreCase) };
		private static Regex[] excludeGroupFilters = new Regex[0];

		public static string[] GetUsernames(string domain, bool includeDomainInUsername = true)
		{
			List<string> usernames = new List<string>();
			try
			{
				SelectQuery query = new SelectQuery("Win32_UserAccount", string.Format("Domain='{0}'", domain));
				ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
				foreach (ManagementObject user in searcher.Get())
				{
					string username = user["Name"] as string;
					if (!string.IsNullOrEmpty(username) && !IsMatch(username, excludeUserFilters))
					{
						usernames.Add((includeDomainInUsername ? domain + "\\" : "") + username);
					}
				}
			}
			catch { }
			return usernames.ToArray();
		}

		public static string[] GetGroupNames(string domain, bool includeDomainInGroupName = true)
		{
			List<string> groupNames = new List<string>();
			try
			{
				SelectQuery query = new SelectQuery("Win32_Group", string.Format("Domain='{0}'", domain));
				ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
				foreach (ManagementObject group in searcher.Get())
				{
					string groupName = group["Name"] as string;
					if (!string.IsNullOrEmpty(groupName) && !IsMatch(groupName, excludeUserFilters))
					{
						groupNames.Add((includeDomainInGroupName ? domain + "\\" : "") + groupName);
					}
				}
			}
			catch { }
			return groupNames.ToArray();
		}

		public static string[] GetDomainNames()
		{
			List<string> domainNames = new List<string>();
			try
			{
				using (var forest = Forest.GetCurrentForest())
				{
					foreach (Domain domain in forest.Domains)
					{
						domainNames.Add(domain.Name);
						domain.Dispose();
					}
				}
			}
			catch { }
			return domainNames.ToArray();
		}

		private static bool IsMatch(string value, Regex[] expressions)
		{
			foreach (Regex regex in expressions)
			{
				if (regex.IsMatch(value))
				{
					return true;
				}
			}
			return false;
		}
	}
}
