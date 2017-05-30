using System.Activities;
using System.ComponentModel;
using Active.Activities.ActivityDesigners;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace Active.Activities.Azure
{
	[Designer(typeof(AzureCreateBlobContainerDesigner))]
	public sealed class CreateBlobContainerIfNotExists : BlobStorageBase
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, will throw an exception if an error occurs. Defaults to true.")]
		[DefaultValue(true)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> ThrowOnError { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Contains the result of the CreateIfNotExists call.")]
		[Category("Output")]
		public OutArgument<bool> Success { get; set; }

		protected override void Execute(CodeActivityContext context)
		{
			CloudBlobContainer blobContainer = new CloudBlobContainer(GetBlobStorageUri(context, Container.Get(context), string.Empty), GetCredentials(context));
			bool result = false;
			string errorMessage = "Unknown error.";
			try
			{
				result = blobContainer.CreateIfNotExists();
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}
			if (ThrowOnError.Get(context))
				throw new ArgumentException(string.Format("Could not create blob container '{0}' : {1}", Container.Get(context), errorMessage));
			Success.Set(context, result);
		}
	}
}
