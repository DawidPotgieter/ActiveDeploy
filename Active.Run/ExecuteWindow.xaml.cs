using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Activities;
using System.IO;
using System.Activities.Presentation;
using System.Activities.Core.Presentation;
using Active.Activities.Helpers;
using Active.Activities.XamlProviders;

namespace Active.Run
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class ExecuteWindow : Window
	{
		private const string WindowTitle = "Active Run";
		private WorkflowDesigner wd;
		private WorkflowApplication wa;
		private bool isExecuting = false;
		private bool shutdownAfterExecute = false;
		private Activity workflow = null;

		public ExecuteWindow()
		{
			InitializeComponent();
		}

		public void AddConsoleText(string text, bool newLine = true)
		{
			ConsoleWindow.Text += text + (newLine ? Environment.NewLine : string.Empty);
			ConsoleScrollViewer.ScrollToBottom();
		}

		private void WriteToConsole(object param)
		{
			App.Current.Dispatcher.BeginInvoke((Action)(() =>
			{
				AddConsoleText((string)param);
			}));
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			wa.Cancel();
		}

		private void Start_Click(object sender, RoutedEventArgs e)
		{
			StartWorkflow();
		}

		private void Abort_Click(object sender, RoutedEventArgs e)
		{
			wa.Abort();
		}

		public WorkflowApplication StartWorkflow(bool debug = false, bool shutdown = false)
		{
			try
			{
				shutdownAfterExecute = shutdown;
				SetStatusRunning();

				LoadWorkflow();

				if (workflow != null)
				{
					wa = WorkflowHelper.ExecuteWorkflow(
						workflow,
						WorkflowCompleted,
						WorkflowUnhandledException,
						WorkflowAborted,
						debug);
				}
				return wa;
			}
			catch (Exception ex)
			{
				SetStatusStopped(null);
				AddConsoleText(ex.Message + Environment.NewLine);
				AddConsoleText("** Workflow was NOT executed. **");
				if (wa != null)
				{
					wa.Abort();
				}
				CheckShutDown();
				WriteLogFile();
			}
			return null;
		}

		private void CheckShutDown(object param = null)
		{
			if (shutdownAfterExecute)
			{
				if (base.CheckAccess())
				{
					App.Current.Shutdown(0);
				}
				else
				{
					base.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
						new System.Threading.WaitCallback(CheckShutDown), param);
				}
			}
		}

		private void SetStatusStopped(object param = null)
		{
			isExecuting = false;
			if (base.CheckAccess())
			{
				this.Title = WindowTitle;
				Start.IsEnabled = !isExecuting;
				Cancel.IsEnabled = isExecuting;
				Abort.IsEnabled = isExecuting;
				CopyText.IsEnabled = !isExecuting;
				string output = param as string;
				if (!string.IsNullOrEmpty(output))
				{
					AddConsoleText(output);
				}
				WriteLogFile();
			}
			else
			{
				base.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
				new System.Threading.WaitCallback(SetStatusStopped), param);
			}
		}

		private void SetStatusRunning()
		{
			isExecuting = true;
			this.Title = string.Format("{0} - Executing...", WindowTitle);
			ConsoleWindow.Text = "";
			Start.IsEnabled = !isExecuting;
			Cancel.IsEnabled = isExecuting;
			Abort.IsEnabled = isExecuting;
			CopyText.IsEnabled = !isExecuting;
		}

		private void WorkflowCompleted(WorkflowApplicationCompletedEventArgs e)
		{
			switch (e.CompletionState)
			{
				case ActivityInstanceState.Canceled:
					SetStatusStopped(Environment.NewLine + "== Execution Cancelled ==");
					break;
				case ActivityInstanceState.Closed:
					SetStatusStopped(Environment.NewLine + "== Execution Complete ==");
					break;
				case ActivityInstanceState.Faulted:
					WriteToConsole(Environment.NewLine + "** Terminated **");
					SetStatusStopped(Environment.NewLine + (e.TerminationException != null ? ExceptionManager.GetExceptionMessage(e.TerminationException) : string.Empty));
					break;
			}
			wa = null;
			CheckShutDown();
		}

		private UnhandledExceptionAction WorkflowUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
		{
			WriteToConsole(Environment.NewLine + "** Unhandled Exception **");
			SetStatusStopped(Environment.NewLine + (e.UnhandledException != null ? ExceptionManager.GetExceptionMessage(e.UnhandledException) : string.Empty));
			return UnhandledExceptionAction.Cancel;
		}

		private void WorkflowAborted(WorkflowApplicationAbortedEventArgs e)
		{
			WriteToConsole(Environment.NewLine + "** Aborted **");
			SetStatusStopped(Environment.NewLine + (e.Reason != null ? ExceptionManager.GetExceptionMessage(e.Reason) : string.Empty));
			CheckShutDown();
		}

		private void LoadWorkflow()
		{
			if (workflow == null)
			{
				string password = null;
				string errorMessage = null;
				if (XamlFileProviderFactory.IsXamlFileEncrypted(App.XamlFile))
				{
					password = GetPassword();
					if (string.IsNullOrEmpty(password))
					{
						errorMessage = string.Format("The file '{0}' is protected and requires the correct password to open.", App.XamlFile);
					}
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					try
					{
						IXamlFileProvider provider = XamlFileProviderFactory.GetXamlFileProvider(App.XamlFile, password);
						provider.LoadXamlFile(App.XamlFile, password);
						workflow = provider.XamlDocument;
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
						if (errorMessage == "The encrypted string was not in a valid format.")
							errorMessage = "The password you specified was incorrect.";
						errorMessage = string.Format("The following error occurred while trying to open '{0}' : \n\n{1}", App.XamlFile, errorMessage);
					}
				}
				if (!string.IsNullOrEmpty(errorMessage))
				{
					ShowErrorMessage(errorMessage, "Failed to load file");
					password = null;
					this.Close();
				}
			}
		}

		private void AddDesigner()
		{
			ScrollViewer designerScrollViewer = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };

			this.wd = new WorkflowDesigner();

			Grid.SetRow(designerScrollViewer, 0);
			Grid.SetColumn(designerScrollViewer, 0);

			LoadWorkflow();
			if (workflow != null)
			{
				this.wd.Load(workflow);
				wd.Flush();
				designerScrollViewer.Content = wd.View;
				wd.View.IsEnabled = false;
				designerScrollViewer.ScrollToTop();
				MainLayout.Children.Add(designerScrollViewer);
			}
		}

		private void ShowErrorMessage(string message, string title = null)
		{
			if (string.IsNullOrEmpty(title)) title = "Error";
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private string GetPassword()
		{
			string returnValue = null;
			Password passwordWindow = new Password();
			passwordWindow.Owner = this;
			bool? success = passwordWindow.ShowDialog();
			if (success.HasValue && success.Value == true)
			{
				returnValue = passwordWindow.PasswordValue;
			}
			passwordWindow = null;
			return returnValue;
		}

		private void RegisterMetadata()
		{
			DesignerMetadata dm = new DesignerMetadata();
			dm.Register();
		}

		private void GridSplitter_MouseEnter(object sender, MouseEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.SizeNS;
		}

		private void GridSplitter_MouseLeave(object sender, MouseEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (App.ShowWorkflow)
			{
				RegisterMetadata();
				AddDesigner();
			}
			else
			{
				MainLayout.RowDefinitions[0].Height = new GridLength(0);
				MainLayout.RowDefinitions[1].Height = new GridLength(0);
			}
			if (App.StartWorkflow)
			{
				this.Start.IsEnabled = false;
				this.InvalidateVisual();
				App.Current.Dispatcher.BeginInvoke((Action)(() =>
				{
				  StartWorkflow(App.Debug, App.ShutDown);
				}));
			}
		}

		private void CopyText_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(ConsoleWindow.Text);
		}

		private void WriteLogFile()
		{
			if (App.CreateLogFile)
			{
				try
				{
					File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("Log_{0}.txt", DateTime.Now.ToString("dd_MM_yyyy hh_mm_ss"))), ConsoleWindow.Text);
				}
				catch (Exception ex)
				{
					ShowErrorMessage(ex.Message, "Failed to write logfile");
				}
			}
		}
	}
}
