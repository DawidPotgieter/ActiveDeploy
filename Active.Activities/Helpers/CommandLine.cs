using System;
using System.Diagnostics;

namespace Active.Activities.Helpers
{
	internal class CommandLineProgressEventArgs : EventArgs
	{
		public string Output { get; set; }
	}

	internal class CommandLine
	{
		public event EventHandler<CommandLineProgressEventArgs> ReportProgress;

		public int Execute(string fileName, string arguments, out string output)
		{
			Process processRunner = new Process();
			processRunner.StartInfo.FileName = fileName;
			processRunner.StartInfo.Arguments = arguments;
			processRunner.StartInfo.UseShellExecute = false;
			processRunner.StartInfo.RedirectStandardOutput = true;
			processRunner.StartInfo.RedirectStandardError = true;
			processRunner.StartInfo.RedirectStandardInput = true;
			processRunner.StartInfo.CreateNoWindow = true;

			processRunner.Start();

			string outPutLine;
			output = string.Empty;

			while ((outPutLine = processRunner.StandardOutput.ReadLine()) != null)
			{
				output += outPutLine + "\n";
				if (ReportProgress != null)
				{
					ReportProgress(this, new CommandLineProgressEventArgs { Output = outPutLine });
				}
			}

			while ((outPutLine = processRunner.StandardError.ReadLine()) != null)
			{
				output += outPutLine + "\n";
				if (ReportProgress != null)
				{
					ReportProgress(this, new CommandLineProgressEventArgs { Output = outPutLine });
				}
			}
			processRunner.WaitForExit();

			return processRunner.ExitCode;
		}
	}
}
