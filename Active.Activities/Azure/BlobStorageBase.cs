using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Active.Activities.Azure
{
	public abstract class BlobStorageBase : StorageBase
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The name of the container to create.")]
		[Category("Arguments")]
		[RequiredArgument]
		public InArgument<string> Container { get; set; }
	}
}
