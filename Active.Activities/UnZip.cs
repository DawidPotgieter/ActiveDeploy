using System.Collections.Generic;
using System.ComponentModel;
using System.Activities;
using Active.Activities.Helpers;
using System.IO;
using System.IO.Compression;

namespace Active.Activities
{
	public class UnZip : CodeActivity
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The path and filename of the zip file to unzip.")]
		[Category("UnZip")]
		[RequiredArgument]
		public InArgument<string> InputFile { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The parent directory to unzip the file to.")]
		[Category("UnZip")]
		[RequiredArgument]
		public InArgument<string> OutputDirectory { get; set; }

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

			string inputFile = InputFile.Get(context);
			string outputDirectory = OutputDirectory.Get(context);

			using (ZipStorer zipStorer = ZipStorer.Open(inputFile, FileAccess.Read))
			{
				List<ZipStorer.ZipFileEntry> directoryRepository = zipStorer.ReadCentralDir();
				foreach (ZipStorer.ZipFileEntry entry in directoryRepository)
				{
					string fileName = Path.GetFileName(entry.FilenameInZip);
					string directoryName = Path.GetDirectoryName(entry.FilenameInZip);
					string fullPath = Path.Combine(outputDirectory, directoryName, fileName);
					ExtractFile(zipStorer, entry, fullPath);
				}
			}
		}

		private void ExtractFile(ZipStorer zipStorer, ZipStorer.ZipFileEntry zipFileEntry, string writeToPath)
		{
			zipStorer.ExtractFile(zipFileEntry, writeToPath);
			WriteLineConsole(string.Format("Extacted : {0}", writeToPath));
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
