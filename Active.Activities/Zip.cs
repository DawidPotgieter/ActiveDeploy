using System.Collections.Generic;
using System.ComponentModel;
using System.Activities;
using Active.Activities.Helpers;
using System.IO;
using System.IO.Compression;

namespace Active.Activities
{
	public class Zip : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The path and filename of the zip file to create.")]
		[Category("Zip")]
		[RequiredArgument]
		public InArgument<string> OutputFile { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("A list of directory and/or filenames to add to the zip file. (New List(Of String) From { \"File.txt\" })")]
		[Category("Zip")]
		[RequiredArgument]
		public InArgument<List<string>> IncludePaths { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("A comment to store inside the zip file.")]
		[Category("Zip")]
		public InArgument<string> Comment { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to True, the output from this process will not be shown on the WPF console. Defaults to False.")]
		[Category("Behaviour")]
		public InArgument<bool> SuppressConsoleOutput { get; set; }

		private ActivityConsole console = null;
		private bool hideConsoleOutput = false;

		protected override void Execute(CodeActivityContext context)
		{
			console = ActivityConsole.GetDefaultOrNew(context);
			hideConsoleOutput = SuppressConsoleOutput.Get(context);

			List<string> includePaths = IncludePaths.Get(context);
			string outputFile = OutputFile.Get(context);
			string comment = Comment.Get(context) ?? string.Empty;

			try
			{
				using (ZipStorer zipStorer = ZipStorer.Create(outputFile, comment))
				{
					foreach (string includePath in includePaths)
					{
						FileAttributes attr = File.GetAttributes(includePath);
						DirectoryInfo directoryInfo = new DirectoryInfo(includePath);

						if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
						{
							AddDirectory(zipStorer, directoryInfo, string.Empty, 0);
						}
						else
						{
							AddFile(zipStorer, includePath, directoryInfo.Name, string.Empty, 0);
						}
					}
				}
			}
			catch
			{
				try
				{
					File.Delete(outputFile);
				}
				catch { }
				throw;
			}
		}

		private void AddFile(ZipStorer zipStorer, string includePath, string filename, string relativePath, int indent)
		{
			zipStorer.AddFile(ZipStorer.Compression.Deflate, includePath, (!string.IsNullOrEmpty(relativePath) ? string.Format("{0}\\{1}", relativePath, filename) : filename), string.Empty);
			WriteLineConsole(string.Format("{0}Added : {1}", new string(' ', indent * 2), filename));
		}

		private void AddDirectory(ZipStorer zipStorer, DirectoryInfo directoryInfo, string relativePath, int indent)
		{
			WriteLineConsole(string.Format("{0}Processing Directory : {1}", new string(' ', indent * 2), directoryInfo.Name));
			indent++;
			foreach (var file in directoryInfo.GetFiles())
			{
				AddFile(zipStorer, file.FullName, file.Name, (!string.IsNullOrEmpty(relativePath) ? string.Format("{0}\\{1}", relativePath, directoryInfo.Name) : directoryInfo.Name), indent);
			}
			foreach (var directory in directoryInfo.GetDirectories())
			{
				AddDirectory(zipStorer, directory, (!string.IsNullOrEmpty(relativePath) ? string.Format("{0}\\{1}", relativePath, directoryInfo.Name) : directoryInfo.Name), indent);
			}
		}

		private void WriteLineConsole(string text)
		{
			if (!hideConsoleOutput)
			{
				console.WriteLine(text);
			}
		}
	}
}
