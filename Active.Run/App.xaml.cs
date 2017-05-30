using System;
using System.Windows;

namespace Active.Run
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static string XamlFile = AppDomain.CurrentDomain.BaseDirectory + "Workflow.xaml";
		public static bool Debug = false;
		public static bool ShutDown = false; 
		public static bool StartWorkflow = false;
		public static bool ShowWorkflow = false;
		public static bool CreateLogFile = true;
		
		private Command GetCommand(string arg)
		{
			switch (arg.ToLower())
			{
				case "-file": return Command.File;
				case "-start": return Command.Start;
				case "-debug": return Command.Debug;
				case "-shutdown": return Command.Shutdown;
				case "-showworkflow": return Command.ShowWorkflow;
				case "-nolog": return Command.NoLogFile;
			}
			return Command.Unknown;
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			foreach (var arg in e.Args)
			{
				var parts = arg.Split(':');
				for (int i = 2; i < parts.Length; i++)
				{
					parts[1] += ":" + parts[i];
				}
				Command command = GetCommand(parts[0]);
				switch (command)
				{
					case Command.Unknown:
						break;
					case Command.File:
						XamlFile = parts[1];
						break;
					case Command.Start:
						StartWorkflow = true;
						break;
					case Command.Debug:
						Debug = true;
						break;
					case Command.Shutdown:
						ShutDown = true;
						break;
					case Command.ShowWorkflow:
						ShowWorkflow = true;
						break;
					case Command.NoLogFile:
						CreateLogFile = false;
						break;
				}
			}
		}
	}
}
