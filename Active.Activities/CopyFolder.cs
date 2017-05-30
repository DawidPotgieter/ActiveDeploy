using System.Collections.Generic;
using System.Activities;
using System.ComponentModel;
using Active.Activities.ActivityDesigners;
using System.IO;
using Active.Activities.Helpers;
using System.Text.RegularExpressions;

namespace Active.Activities
{
	[Designer(typeof(CopyFolderDesigner))]
	public class CopyFolder : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Source folder to copy.")]
		[RequiredArgument]
		public InArgument<string> Source { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Target folder to copy to.")]
		[RequiredArgument]
		public InArgument<string> Target { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Determines whether the copy output is sent to the ActivityConsole.")]
		public InArgument<bool> ShowDetails { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("A list of regular expressions that will be used to exclude files copied." +
			"\n e.g New List(Of System.Text.RegularExpressions.Regex) From { New System.Text.RegularExpressions.Regex(\"\\.dll$\")}")]
		public InArgument<List<Regex>> ExcludeFileFilters { get; set; }

		private ActivityConsole console = null;
		private bool showDetails = false;
		private List<Regex> excludeFileFilters;

		private void WriteLineConsole(string text)
		{
			if (showDetails)
			{
				console.WriteLine(text);
			}
		}

		protected override void Execute(CodeActivityContext context)
		{
			showDetails = ShowDetails.Get(context);
			excludeFileFilters = ExcludeFileFilters.Get(context);
			if (excludeFileFilters == null) excludeFileFilters = new List<Regex>();

			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();

			DirectoryInfo sourcePath = new DirectoryInfo(Source.Get(context));
			DirectoryInfo targetPath = new DirectoryInfo(Target.Get(context));

			if (!sourcePath.Exists)
				throw new IOException("The source path does not exist.");

			CopyDirectory(sourcePath, targetPath);
		}

		private void CopyDirectory(DirectoryInfo source, DirectoryInfo destination, int indent = 0)
		{
			if (!destination.Exists)
			{
				WriteLineConsole(new string(' ', indent *2) + "Creating Folder : " + destination.FullName);
				destination.Create();
			}

			// Copy all files.
			FileInfo[] files = source.GetFiles();
			foreach (FileInfo file in files)
			{
				bool copy = true;
				foreach (Regex excludeFileFilter in excludeFileFilters)
				{
					if (excludeFileFilter.IsMatch(file.Name))
					{
						copy = false;
						break;
					}
				}

				if (copy)
				{
					WriteLineConsole(new string(' ', indent * 2) + "Copying File : " + file.FullName + " to " + destination.FullName);
					file.CopyTo(Path.Combine(destination.FullName,
							file.Name), true);
				}
				else
				{
					WriteLineConsole(new string(' ', indent * 2) + "Excluded File : " + file.FullName);
				}
			}

			// Process subdirectories.
			DirectoryInfo[] dirs = source.GetDirectories();
			foreach (DirectoryInfo dir in dirs)
			{
				bool copy = true;
				foreach (Regex excludeFileFilter in excludeFileFilters)
				{
					if (excludeFileFilter.IsMatch(dir.FullName))
					{
						copy = false;
						break;
					}
				}

				if (copy)
				{
					// Get destination directory.
					string destinationDir = Path.Combine(destination.FullName, dir.Name);
					// Call CopyDirectory() recursively.
					CopyDirectory(dir, new DirectoryInfo(destinationDir), ++indent);
				}
			}
		}
	}
}
