using System.Activities;
using System.ComponentModel;
using Active.Activities.ActivityDesigners;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Active.Activities.Azure
{
	[Designer(typeof(AzureQueryBlobsDesigner))]
	public sealed class QueryBlobs : BlobStorageBase
	{
		private Activity child;
		public Activity Child
		{
			get
			{
				return child;
			}
			set
			{
				child = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The path to search.")]
		[Category("Arguments")]
		public InArgument<string> Path { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The prefix to search for.")]
		[Category("Arguments")]
		public InArgument<string> Prefix { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, will throw an exception if an error occurs. Defaults to true.")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> ThrowOnError { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Contains the result of the query.")]
		[Category("Output")]
		public OutArgument<List<CloudBlob>> Result { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			CloudBlobContainer blobContainer = new CloudBlobContainer(GetBlobStorageUri(context, Container.Get(context), Path.Get(context)), GetCredentials(context));
			var result = new List<CloudBlob>();
			string errorMessage = "Unknown error.";
			try
			{
				result = blobContainer.ListBlobs(Prefix.Get(context), true, BlobListingDetails.Metadata | BlobListingDetails.Copy).Select(b => (CloudBlob)b).ToList();
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}
			if (ThrowOnError.Get(context))
				throw new ArgumentException(string.Format("Could not query blob container '{0}' : {1}", Container.Get(context), errorMessage));
			Result.Set(context, result);
		}
	}
}
