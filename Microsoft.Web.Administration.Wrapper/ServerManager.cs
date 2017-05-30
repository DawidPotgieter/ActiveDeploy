using System;
using System.Collections.Generic;

namespace Microsoft.Web.Administration.Wrapper
{
	public class ServerManager : IDisposable
	{
		private Administration.ServerManager serverManager;

		public ServerManager()
		{
			serverManager = new Administration.ServerManager();
		}

		private List<Site> sites = null;
		public ReadOnlyStringIndexedCollection<Site> Sites
		{
			get
			{
				if (sites == null)
				{
					sites = new List<Site>();
					foreach (Administration.Site site in serverManager.Sites)
					{
						sites.Add(new Site(site));
					}
				}
				return new ReadOnlyStringIndexedCollection<Site>(sites);
			}
		}

		public void Dispose()
		{
			if (serverManager != null)
			{
				serverManager.Dispose();
				serverManager = null;
			}
		}
	}
}
