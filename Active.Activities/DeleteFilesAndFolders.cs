using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Activities;
using Active.Activities.Helpers;
using System.IO;

namespace Active.Activities
{
	[Description("Writes the specified message to the ActivityConsole.")]
	public sealed class DeleteFilesAndFolders : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[RequiredArgument]
		[Description("Path where the file(s) are located. See http://msdn.microsoft.com/en-us/library/dd413233(v=vs.100).aspx for valid path expressions.")]
		public InArgument<string> Path { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[RequiredArgument]
		[Description("See http://msdn.microsoft.com/en-us/library/dd413233(v=vs.100).aspx for valid filter expressions.")]
		public InArgument<string> Filter { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, it will also enumerate subfolders of 'Path' with the specified 'Filter' and delete those recursively.  Defaults to 'false'.")]
		public InArgument<bool> IncludeFolders { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Determines whether the delete output is sent to the ActivityConsole.")]
		public InArgument<bool> ShowDetails { get; set; }

		private ActivityConsole console = null;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();
			bool showDetails = ShowDetails.Get(context);
			string path = Path.Get(context);
			string filter = Filter.Get(context);

			List<string> fileNames = Directory.EnumerateFiles(path, filter).ToList();
			foreach (string fileName in fileNames)
			{
				File.Delete(fileName);
				if (showDetails)
				{
					console.WriteLine(string.Format("Deleted file '{0}'.", fileName));
				}
			}

			if (IncludeFolders.Get(context))
			{
				List<string> directoryNames = Directory.EnumerateDirectories(path, filter).ToList();
				foreach (string directoryName in directoryNames)
				{
					Directory.Delete(directoryName, true);
					if (showDetails)
					{
						console.WriteLine(string.Format("Deleted folder '{0}' and everything in it.", directoryName));
					}
				}
			}
		}
	}
}
