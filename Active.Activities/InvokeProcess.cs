using System;
using System.Activities;
using System.Diagnostics;
using System.ComponentModel;
using Active.Activities.Helpers;
using Active.Activities.ActivityDesigners;

namespace Active.Activities
{
	[Designer(typeof(InvokeProcessDesigner))]
	public sealed class InvokeProcess: CodeActivity
	{
		[RequiredArgument]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The full path of the executable file to invoke.")]
		[Category("Process")]
		public InArgument<string> FileName { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("Optional arguments to pass on the command line.")]
		[Category("Process")]
		public InArgument<string> Arguments { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The working directory of the executable file.")]
		[Category("Process")]
		public InArgument<string> WorkingDirectory { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The verb to pass to the new process.")]
		[Category("Process")]
		public InArgument<string> Verb { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to True, the output from this process will not be shown on the WPF console.")]
		[Category("Behaviour")]
		public InArgument<bool> SuppressConsoleOutput { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("For commands that are contained inside the command interpreter (i.e. dir), this must be set to True.")]
		[Category("Behaviour")]
		public InArgument<bool> IsNative { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to True, a non 0 ExitCode will throw an InvalidOperationException with the standard error output stream as exception message.")]
		[Category("Behaviour")]
		public InArgument<bool> ThrowOnExitCode { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, it will invoke this process and show it's own new window.")]
		[Category("Behaviour")]
		public InArgument<bool> ShowWindow { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("If set to true, it will open the file with the program registered to handle the file (i.e. URL will open in default browser)")]
		[Category("Behaviour")]
		public InArgument<bool> UseShellExecute { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The console output.  If ThrowOnExitCode is set to true, this will not be populated.")]
		[Category("Output")]
		public OutArgument<string> Output { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Description("The process exit code.")]
		[Category("Output")]
		public OutArgument<int> ExitCode { get; set; }

		private ActivityConsole console = null;
		private bool hideConsoleOutput = false;

		protected override void Execute(CodeActivityContext context)
		{
			console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();
			hideConsoleOutput = SuppressConsoleOutput.Get(context);
			ExecuteInvoke(context);
		}

		private void WriteLineConsole(string text)
		{
			if (!hideConsoleOutput)
			{
				console.WriteLine(text);
			}
		}

		private void ExecuteInvoke(object contextObject)
		{
			CodeActivityContext context = contextObject as CodeActivityContext;

			string commandLine = FileName.Get(context) + " " + Arguments.Get(context);
			bool useShellExecute = UseShellExecute.Get<bool>(context);
			bool showWindow = ShowWindow.Get<bool>(context);
			string stdOut = string.Empty;
			string errOut = string.Empty;
			bool isNative = IsNative.Get(context);

			WriteLineConsole((useShellExecute ? "Open : " : "") + commandLine);

			Process processRunner = new Process();
			if (isNative)
			{
				processRunner.StartInfo.FileName = Environment.GetEnvironmentVariable("COMSPEC");
			}
			else
			{
				processRunner.StartInfo.FileName = FileName.Get(context);
				processRunner.StartInfo.Arguments = Arguments.Get(context);
			}


			processRunner.StartInfo.WorkingDirectory = WorkingDirectory.Get<string>(context);
			processRunner.StartInfo.UseShellExecute = useShellExecute;
			processRunner.StartInfo.CreateNoWindow = showWindow;
			processRunner.StartInfo.Verb = Verb.Get<string>(context);

			if (!useShellExecute && !showWindow)
			{
				processRunner.StartInfo.RedirectStandardOutput = true;
				processRunner.StartInfo.RedirectStandardError = true;
				processRunner.StartInfo.RedirectStandardInput = true;
			}

			try
			{
				processRunner.Start();
			}
			catch (Exception ex)
			{
				ExitCode.Set(context, 1);
				if (ThrowOnExitCode.Get<bool>(context))
				{
					throw new InvalidOperationException(ex.Message);
				}
				else
				{
					WriteLineConsole(ex.Message);
					Output.Set(context, ex.Message);
				}
				return;
			}

			if (!useShellExecute && !showWindow)
			{
				if (IsNative.Get(context))
				{
					processRunner.StandardInput.WriteLine("echo off");
					processRunner.StandardInput.WriteLine(commandLine);
					processRunner.StandardInput.WriteLine("exit");
				}

				string outPutLine;
				string outPut = string.Empty;
				bool blockedOutput = IsNative.Get(context);

				while ((outPutLine = processRunner.StandardOutput.ReadLine()) != null)
				{
					if ((blockedOutput) || (outPutLine == "exit"))
					{
						if (outPutLine == commandLine) blockedOutput = false;
						continue;
					}
					stdOut += outPutLine + "\n";
					WriteLineConsole(outPutLine);
				}

				while ((outPutLine = processRunner.StandardError.ReadLine()) != null)
				{
					if ((blockedOutput) || (outPutLine == "exit"))
					{
						if (outPutLine == commandLine) blockedOutput = false;
						continue;
					}
					errOut += outPutLine + "\n";
					WriteLineConsole(outPutLine);
				}
				processRunner.WaitForExit();

				ExitCode.Set(context, processRunner.ExitCode);

				bool success = (processRunner.ExitCode == 0);

				if (success)
				{
					Output.Set(context, stdOut);
				}
				else
				{
					if (!ThrowOnExitCode.Get<bool>(context))
					{
						Output.Set(context, errOut);
					}
				}

				if (processRunner != null && !processRunner.HasExited)
				{
					processRunner.Kill();
					processRunner = null;
				}

				if (!success && ThrowOnExitCode.Get<bool>(context))
				{
					throw new InvalidOperationException(errOut);
				}
			}
		}
	}
}
