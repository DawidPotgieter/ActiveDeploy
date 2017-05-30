using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.ComponentModel;
using Active.Builder.DesignerDataTypes;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using Active.Activities.Resources;
using Active.Activities.XamlProviders;
using Active.Builder.ExpressionEditor;
using System.Runtime.Versioning;
using System.Windows.Media;

namespace Active.Builder
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private WorkflowDesigner wd;
		private string xamlFile = AppDomain.CurrentDomain.BaseDirectory + "Workflow.xaml";
		private Process processRunner;
		private string lastSavedXaml = string.Empty;
		private string password = null;
		private bool busyGettingPassword = false;
		private bool mustSave = false;
		private bool isLoadingXaml = false;
		private ExpressionEditor.Intellisense intellisense;
		private System.Threading.Tasks.Task intellisenseLoadingTask;

		public bool Encrypt { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			intellisenseLoadingTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				intellisense = new ExpressionEditor.Intellisense();
			});
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				RegisterMetadata();
				AddCustomActivityIcons();
				AddNativeActivityIcons();
				AddToolBox();
				
				if (File.Exists(xamlFile))
					LoadDesignerAndWorkflow(xamlFile);
				else
					LoadDesignerAndWorkflow();

				StatusBarText.Text = "Loading Intellisense...";
				StatusBarText.Foreground = new SolidColorBrush(Colors.Green);
				StatusBarText.FontSize += 3;
				intellisenseLoadingTask.GetAwaiter().OnCompleted(() =>
				{
					EditorService editorService = new EditorService
					{
						IntellisenseData = intellisense.IntellisenseList,
						EditorKeyWord = intellisense.CreateKeywords(),
					};
					wd.Context.Services.Publish<System.Activities.Presentation.View.IExpressionEditorService>(editorService);
					StatusBarText.Text = "";
					StatusBarText.Foreground = new SolidColorBrush(Colors.Black);
					StatusBarText.FontSize -= 3;
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + (ex.InnerException != null ? ex.InnerException.Message : string.Empty));
				Application.Current.Shutdown();
			}
		}

		private bool IsDirty()
		{
			wd.Flush();
			return lastSavedXaml != wd.Text || mustSave;
		}

		private void LoadDesignerAndWorkflow(string fileName = null)
		{
			isLoadingXaml = true;
			Encrypt = false;
			mustSave = false;

			if (wd != null)
			{
				MainLayout.Children.Remove(this.wd.View);
				MainLayout.Children.Remove(this.wd.PropertyInspectorView);
				this.wd.ModelChanged -= wd_ModelChanged;
				this.wd = null;
			}
			//Create an instance of WorkflowDesigner class.
			this.wd = new WorkflowDesigner();

			DesignerConfigurationService configService = this.wd.Context.Services.GetService<DesignerConfigurationService>();
			configService.TargetFrameworkName = new FrameworkName(".NETFramework", new Version(4, 5));
			configService.LoadingFromUntrustedSourceEnabled = true;

			this.wd.ModelChanged += new EventHandler(wd_ModelChanged);

			//Place the designer canvas in the middle column of the grid.
			Grid.SetRow(this.wd.View, 1);
			Grid.SetColumn(this.wd.View, 2);

			AddPropertyInspector();

			if (string.IsNullOrEmpty(fileName))
			{
				this.wd.Load(new Sequence());
				wd.Flush();
				lastSavedXaml = wd.Text;
				mustSave = true;
			}
			else
			{
				password = null;
				string errorMessage = null;
				if (XamlFileProviderFactory.IsXamlFileEncrypted(fileName))
				{
					if (!GetPassword())
					{
						errorMessage = string.Format("The file '{0}' is protected and requires the correct password to open.", xamlFile);
					}
					else
					{
						Encrypt = true;
					}
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					try
					{
						IXamlFileProvider provider = XamlFileProviderFactory.GetXamlFileProvider(fileName, password);
						provider.LoadXamlFile(fileName, password);
						this.wd.Load(provider.XamlDocument);
						wd.Flush();
						lastSavedXaml = wd.Text;
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
						if (errorMessage == "The encrypted string was not in a valid format.")
							errorMessage = "The password you specified was incorrect.";
						errorMessage = string.Format("The following error occurred while trying to open '{0}' : \n\n{1}", xamlFile, errorMessage);
					}
				}
				if (!string.IsNullOrEmpty(errorMessage))
				{
					xamlFile = string.Empty;
					ShowErrorMessage(errorMessage, "Failed to load file");
					this.wd.Load(new Sequence());
					password = null;
					mustSave = true;
					Encrypt = false;
				}
			}

			busyGettingPassword = true;
			Protect.GetBindingExpression(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty).UpdateTarget();
			busyGettingPassword = false;

			//Add the designer canvas to the grid.
			MainLayout.Children.Add(this.wd.View);

			StatusBarText.Text = "";
			ToggleDocumentHasChangesVisual(mustSave);

			isLoadingXaml = false;
		}

		void wd_ModelChanged(object sender, EventArgs e)
		{
			//Keep in mind that some of the activities changes their layout after loading, so loading a a file with for instance MSDeploy activity will always make this think it's unsaved.
			if (!isLoadingXaml)
			{
				if (IsDirty())
				{
					ToggleDocumentHasChangesVisual(true);
				}
				else
				{
					ToggleDocumentHasChangesVisual(false);
				}
			}
		}

		private void ToggleDocumentHasChangesVisual(bool hasChanges)
		{
			if (hasChanges)
			{
				SaveFile.IsChecked = true;
			}
			else
			{
				SaveFile.IsChecked = false;
			}
			RefreshTitle();
		}

		private void RefreshTitle()
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			FileVersionInfo version = FileVersionInfo.GetVersionInfo(assembly.Location);
			this.Title = string.Format("Active Builder [{1}] - {0} {2}", string.IsNullOrEmpty(xamlFile) ? "New" : xamlFile, version.FileVersion, (IsDirty() ? "*" : ""));
		}

		private void ShowErrorMessage(string message, string title = null)
		{
			if (string.IsNullOrEmpty(title)) title = "Error";
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void RegisterMetadata()
		{
			DesignerMetadata dm = new DesignerMetadata();
			dm.Register();
		}

		private ToolboxControl GetToolboxControl()
		{
			ToolboxControl ctrl = new ToolboxControl();

			foreach (var categoryDefinition in CustomActivityDefinitions.Categories)
			{
				var category = new ToolboxCategory(categoryDefinition.Name);
				foreach (var toolDefinition in categoryDefinition.ActivityTypes)
				{
					ToolboxItemWrapper tool;
					if (string.IsNullOrEmpty(toolDefinition.DisplayName))
						tool = new ToolboxItemWrapper(toolDefinition.ActivityType);
					else
						tool = new ToolboxItemWrapper(toolDefinition.ActivityType, toolDefinition.DisplayName);
					category.Add(tool);
				}
				ctrl.Categories.Add(category);
			}

			foreach (var categoryDefinition in NativeActivityDefintions.Categories)
			{
				var category = new ToolboxCategory(categoryDefinition.Name);
				foreach (var toolDefinition in categoryDefinition.ActivityTypes)
				{
					ToolboxItemWrapper tool;
					if (string.IsNullOrEmpty(toolDefinition.DisplayName))
						tool = new ToolboxItemWrapper(toolDefinition.ActivityType);
					else
						tool = new ToolboxItemWrapper(toolDefinition.ActivityType, toolDefinition.DisplayName);
					category.Add(tool);
				}

				ctrl.Categories.Add(category);
			}

			//After converting the native activity icons to local resources, this is not needed anymore.  Keeping the code here because I know I'm going to want this as referenced code
			//Collapse all categories.  This speeds up loading significantly.
			//ctrl.CategoryItemStyle = 
			//  new System.Windows.Style(typeof(TreeViewItem))
			//  {
			//    Setters = { new Setter(TreeViewItem.IsExpandedProperty, false) }
			//  };

			return ctrl;
		}

		private void AddToolBox()
		{
			ToolboxControl tc = GetToolboxControl();
			Grid.SetRow(tc, 1);
			Grid.SetColumn(tc, 0);
			MainLayout.Children.Add(tc);
		}

		private void AddPropertyInspector()
		{
			Grid.SetRow(wd.PropertyInspectorView, 1);
			Grid.SetColumn(wd.PropertyInspectorView, 4);
			MainLayout.Children.Add(wd.PropertyInspectorView);
		}

		private void AddCustomActivityIcons()
		{
			foreach (var categoryDefinition in CustomActivityDefinitions.Categories)
			{
				foreach (var activityType in categoryDefinition.ActivityTypes)
				{
					Stream designerIconStream = ResourceManager.GetActivityDesignerIcon(activityType.ActivityType.Name);
					if (designerIconStream != null)
					{
						DesignerHelper.AddToolboxIcon(
							activityType.ActivityType,
							new System.Drawing.Bitmap(designerIconStream));
						designerIconStream = null;
					}
				}
			}
		}

		private void AddNativeActivityIcons()
		{
			foreach (var categoryDefinition in NativeActivityDefintions.Categories)
			{
				foreach (var activityType in categoryDefinition.ActivityTypes)
				{
					Stream designerIconStream = ResourceManager.GetActivityDesignerIcon(activityType.ActivityType.Name.Split('`')[0]);
					if (designerIconStream != null)
					{
						DesignerHelper.AddToolboxIcon(
							activityType.ActivityType,
							new System.Drawing.Bitmap(designerIconStream));
						designerIconStream = null;
					}
				}
			}
		}

		private void New_Click(object sender, RoutedEventArgs e)
		{
			bool createNew = false;

			if (QuerySave())
			{
				createNew = true;
			}

			if (createNew)
			{
				this.IsEnabled = false;
				mustSave = true;
				Encrypt = false;
				xamlFile = string.Empty;
				LoadDesignerAndWorkflow();
				this.IsEnabled = true;
			}
			ToggleDocumentHasChangesVisual(true);
		}

		private void SaveFile_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(xamlFile))
			{
				SaveFileAs_Click(sender, e);
			}
			else
			{
				this.IsEnabled = false;
				ExecuteSaveFile();
				this.IsEnabled = true;
			}
		}

		private void ExecuteSaveFile(bool background = true)
		{
			if (string.IsNullOrEmpty(xamlFile))
			{
				ExecuteSaveFileAs();
				return;
			}
			StatusBarText.Text = "Saving...";
			wd.Flush();
			lastSavedXaml = wd.Text;
			ToggleDocumentHasChangesVisual(false);
			mustSave = false;
			IXamlFileProvider provider = XamlFileProviderFactory.GetNewXamlFileProvider(!string.IsNullOrEmpty(password));
			if (background)
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
				{
					provider.WriteXamlFile(xamlFile, wd.Text, password);
					StatusBarText.Text = "Done.";
					this.IsEnabled = true;
				}));
			}
			else
			{
				provider.WriteXamlFile(xamlFile, wd.Text, password);
				StatusBarText.Text = "Done.";
			}
			RefreshTitle();
		}

		private void SaveFileAs_Click(object sender, RoutedEventArgs e)
		{
			ExecuteSaveFileAs();
		}

		private void ExecuteSaveFileAs()
		{
			this.IsEnabled = false;
			Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

			saveFileDialog.FileName = xamlFile;
			saveFileDialog.DefaultExt = ".xaml";
			saveFileDialog.Filter = "XAML documents (.xaml)|*.xaml";

			bool? result = saveFileDialog.ShowDialog();

			if (result == true)
			{
				xamlFile = saveFileDialog.FileName;
				ExecuteSaveFile();
			}
			else
			{
				this.IsEnabled = true;
			}
		}

		private void OpenFile_Click(object sender, RoutedEventArgs e)
		{
			this.IsEnabled = false;
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

			openFileDialog.DefaultExt = ".xaml";
			openFileDialog.Filter = "XAML documents (.xaml)|*.xaml";

			bool? result = openFileDialog.ShowDialog();

			if (result == true)
			{
				if (!QuerySave())
				{
					this.IsEnabled = true;
					return;
				}
				xamlFile = openFileDialog.FileName;
				ExecuteOpenFile();
			}
			else
			{
				this.IsEnabled = true;
			}
		}

		private void ExecuteOpenFile()
		{
			StatusBarText.Text = "Loading...";
			this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
			{
				LoadDesignerAndWorkflow(xamlFile);
				StatusBarText.Text = "Done Loading.";
				this.IsEnabled = true;
			}));
		}

		private void RunFile_Click(object sender, RoutedEventArgs e)
		{
			ExecuteRunFile();
		}

		private void ExecuteRunFile()
		{
			ExecuteSaveFile(false);
			if (processRunner != null && !processRunner.HasExited)
				processRunner.Kill();
			processRunner = new Process();
			processRunner.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "Active.Run.exe";
			processRunner.StartInfo.Arguments = string.Format("\"-file:{0}\" -start -nolog", xamlFile);
			processRunner.Start();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (processRunner != null && !processRunner.HasExited)
			{
				processRunner.Kill();
				processRunner = null;
			}
			e.Cancel = !QuerySave();
		}

		/// <summary>
		/// Returns false if operation needs to be cancelled.
		/// </summary>
		/// <returns></returns>
		private bool QuerySave()
		{
			if (IsDirty())
			{
				var msgboxResult = MessageBox.Show("Do you want to save the current workflow?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
				switch (msgboxResult)
				{
					case MessageBoxResult.Yes:
						ExecuteSaveFile(false);
						break;
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
						return false;
				}
			}
			return true;
		}

		private void GridSplitter_MouseEnter(object sender, MouseEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.SizeWE;
		}

		private void GridSplitter_MouseLeave(object sender, MouseEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		private void UnProtectFile()
		{
			password = null;
		}

		private void ProtectFile()
		{
			GetPassword();
		}

		private bool GetPassword()
		{
			bool returnValue = false;
			if (!busyGettingPassword)
			{
				busyGettingPassword = true;
				Password passwordWindow = new Password();
				passwordWindow.Owner = this;
				passwordWindow.ToolTip = string.Format("Requesting password for : '{0}'", xamlFile);
				bool? success = passwordWindow.ShowDialog();
				if (success.HasValue && success.Value == true)
				{
					password = passwordWindow.PasswordValue;
					returnValue = true;
				}
				else
				{
					UnProtectFile();
				}
				passwordWindow = null;
				if (!isLoadingXaml)
					mustSave = true;
			}
			busyGettingPassword = false;
			return returnValue;
		}

		private void Protect_Checked(object sender, RoutedEventArgs e)
		{
			ProtectFile();
			ToggleDocumentHasChangesVisual(true);
		}

		private void Protect_Unchecked(object sender, RoutedEventArgs e)
		{
			UnProtectFile();
			ToggleDocumentHasChangesVisual(true);
		}

		private void Package_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
