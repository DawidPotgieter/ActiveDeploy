using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Microsoft.Web.Administration.Wrapper
{
	public class ReadOnlyStringIndexedCollection<T> : ReadOnlyCollection<T> where T : NamedObject
	{
		internal ReadOnlyStringIndexedCollection(IList<T> list)
			: base(list)
		{
		}

		public T this[string name] { get { return this.First(i => ((NamedObject)i).Name == name); } }
	}
}
