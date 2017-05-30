using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Active.Activities.Azure
{
	public abstract class StorageBase : CodeActivity
	{
		protected const string blobStorageUriTemplate = "https://{0}.blob.core.windows.net/";
		protected const string blobStorageSecondaryUriTemplate = "https://{0}-secondary.blob.core.windows.net/";

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Azure storage account name.")]
		[Category("Security")]
		[RequiredArgument]
		public InArgument<string> Account { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Azure storage account key.")]
		[Category("Security")]
		[RequiredArgument]
		public InArgument<string> AccountKey { get; set; }

		protected StorageUri GetBlobStorageUri(CodeActivityContext context, string container, string path)
		{
			return new StorageUri(
				new Uri(string.Format(blobStorageUriTemplate, Account.Get(context)) + container + "/" + path ?? ""),
				new Uri(string.Format(blobStorageSecondaryUriTemplate, Account.Get(context)) + container + "/" + path ?? ""));
		}

		protected StorageCredentials GetCredentials(CodeActivityContext context)
		{
			return new StorageCredentials(Account.Get(context), AccountKey.Get(context));
		}
	}
}
