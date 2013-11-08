using System.IO;
using System.Security.AccessControl;

namespace SqlServerRunnerNet.Utils
{
	public static class FileSystemUtils
	{
		public static bool CanReadDirectory(string path)
		{
			var listAllow = false;
			var listDeny = false;

			try
			{
				var accessControlList = Directory.GetAccessControl(path);
				var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

				foreach (FileSystemAccessRule rule in accessRules)
				{
					if ((FileSystemRights.ListDirectory& rule.FileSystemRights) != FileSystemRights.ListDirectory) continue;

					if (rule.AccessControlType == AccessControlType.Allow)
						listAllow = true;
					else if (rule.AccessControlType == AccessControlType.Deny)
						listDeny = true;
				}

				return listAllow && !listDeny;
			}
			catch
			{
				return false;
			}
		}
	}
}
