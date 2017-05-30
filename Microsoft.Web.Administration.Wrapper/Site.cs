using System.Collections.Generic;

namespace Microsoft.Web.Administration.Wrapper
{
	public class Site : NamedObject
	{
		private Administration.Site site;

		internal Site(Administration.Site site)
			: base(site.Name)
		{
			this.site = site;
		}

		public long Id { get { return site.Id; } }

		public override string Name { get { return site.Name; } }

		private List<Application> applications = null;
		public ReadOnlyStringIndexedCollection<Application> Applications
		{
			get
			{
				if (applications == null)
				{
					applications = new List<Application>();
					foreach (Administration.Application application in site.Applications)
					{
						applications.Add(new Application(application, application.Path));
					}
				}
				return new ReadOnlyStringIndexedCollection<Application>(applications);
			}
		}
	}
}
