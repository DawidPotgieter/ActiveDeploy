namespace Microsoft.Web.Administration.Wrapper
{
	public class NamedObject
	{
		internal NamedObject(string name)
		{
			Name = name;
		}

		public virtual string Name { get; internal set; }
	}
}
