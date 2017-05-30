using System.Activities;
using System.ComponentModel;
using Active.Activities.Helpers;
using System.IO;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(CreateFolderDesigner))]
	public class CreateFolder : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path of the folder to create.")]
		[RequiredArgument]
		public InArgument<string> FolderName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Determines whether any output is sent to the ActivityConsole.")]
		[DefaultValue(false)] //NB : This only works because it's manually read in the designer
		public InArgument<bool> ShowOutput { get; set; }

		private ActivityConsole console = null;
		private bool showOutput = false;

		private void WriteLineConsole(string text)
		{
			if (showOutput)
			{
				console.WriteLine(text);
			}
		}

		protected override void Execute(CodeActivityContext context)
		{
			showOutput = ShowOutput.Get(context);
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			DirectoryInfo path = new DirectoryInfo(FolderName.Get(context));

			if (!path.Exists)
			{
				WriteLineConsole("Creating Folder : " + path.FullName);
				path.Create();
			}
			else
			{
				WriteLineConsole(string.Format("Folder '{0}' already exists. Skipping folder creation.", path.FullName));
			}
		}
	}
}
