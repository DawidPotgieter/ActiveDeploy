﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Active.Activities.Helpers;
using System.Activities;
using Microsoft.VisualBasic.Activities;

namespace Active.Activities.ActivityDesigners
{
	// Interaction logic for MSDeployDesigner.xaml
	public partial class MSDeployDesigner
	{
		public MSDeployDesigner()
		{
			InitializeComponent();
			//Keep this here, can be useful at some stage
			//if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			//{
			//  this.Width = double.NaN;
			//  this.Height = double.NaN;
			//}
			string versionHelp = "MSDeploy version to use.  Note that the .cmd file is dependant on which version of Visual Studio/TFS generated the package.\n\n";
			versionHelp += "CMD files generated by 2010 SP1 has a bug that causes the MSDeploy.exe (v2) path not to be found. This used to be fixed by selecting deprecated 'UseVersion2x', but you do not need to specify that any more.\n";
			versionHelp += "CMD files generated by 2012 has a bug that removes '=' from any params passed to MSDeploy.exe, so for these, you have to use the 'Exe' execution method if you specify the IIS Web Application name.\n";
			versionHelp += "If you're deploying to the root of a site, you can either use blank or '/' as the IIS Web Application name.\n";
			versionHelp += "For remote deployments, use the full .axd path for computer name i.e. 'https://server:8172/msdeploy.axd?site=mysitename'\n";

			lblVersionHelp.Content = versionHelp;

			VersionHelpContent.Background = new SolidColorBrush(SystemColors.InfoColor);
		}

		private void btnPickCmdFile_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "Command Files (*.cmd)|*.cmd|All Files|*.*", "CmdFileName", false);
		}

		private void btnSelectZipFileName_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "Zip Files (*.zip)|*.zip|All Files|*.*", "ZipPackageFilename", false);
		}

		private void btnSelectParamFileName_Click(object sender, RoutedEventArgs e)
		{
			Helpers.DesignerHelper.PickFile(ModelItem, "XML Files (*.xml)|*.xml|All Files|*.*", "ParamFilename", false);
		}

		private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
		{
			Activities.MSDeploy modelItem = (Activities.MSDeploy)ModelItem.GetCurrentValue();

			bool? defaultBoolValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "UseVersion2x");

			if (modelItem.UseVersion2x == null && defaultBoolValue != null)
				chkUserVersion2X.IsChecked = defaultBoolValue.Value;

			defaultBoolValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "DoNotDeleteFiles");

			if (modelItem.DoNotDeleteFiles == null && defaultBoolValue != null)
				chkDoNotDeleteFiles.IsChecked = defaultBoolValue.Value;

			defaultBoolValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "AllowUntrustedSSLConnection");

			if (modelItem.AllowUntrustedSSLConnection == null && defaultBoolValue != null)
				chkAllowUntrusted.IsChecked = defaultBoolValue.Value;

			defaultBoolValue = (bool?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "Test");

			if (modelItem.Test == null && defaultBoolValue != null)
				chkTest.IsChecked = defaultBoolValue.Value;

			MSDeployExecutionType? executionTypeDefaultValue = (MSDeployExecutionType?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "ExecutionType");

			if (modelItem.ExecutionType == null)
			{
				if (executionTypeDefaultValue != null)
				{
					if (executionTypeDefaultValue.Value == MSDeployExecutionType.Cmd)
					{
						rdoCmd.IsChecked = true;
					}
					else
					{
						rdoExe.IsChecked = true;
					}
				}
			}
			else
			{
				MSDeployExecutionType? executionType = GetModelItemMSDeployExecutionTypeValue(ModelItem, "ExecutionType");
				if (executionType == MSDeployExecutionType.Cmd)
				{
					rdoCmd.IsChecked = true;
				}
				else
				{
					rdoExe.IsChecked = true;
				}
			}

			int? versionDefaultValue = (int?)DesignerHelper.GetDefaultValueAttributeValue(modelItem, "UseVersion");

			if (modelItem.UseVersion == null)
			{
				if (versionDefaultValue != null)
				{
					cmbVersion.SelectedIndex = versionDefaultValue.Value - 1;
				}
			}
			else
			{
				cmbVersion.SelectedIndex = DesignerHelper.GetInArgumentModelItemIntValue(ModelItem, "UseVersion").Value - 1;
			}
		}

		private void btnSelectIISApplication_Click(object sender, RoutedEventArgs e)
		{
			CustomDialogs.SelectIISApplication selectIISVirtualFolderWindow = new CustomDialogs.SelectIISApplication();
			var result = selectIISVirtualFolderWindow.ShowDialog();
			if (result.HasValue && result.Value)
			{
				Helpers.DesignerHelper.SetModelItemExpressionTextValue(ModelItem, "IISWebApplication", "\"" + selectIISVirtualFolderWindow.VirtualDirectoryPath + "\"");
			}
		}

		private void cmbVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DesignerHelper.SetInArgumentModelItemIntValue(ModelItem, "UseVersion", cmbVersion.SelectedIndex + 1);
		}

		public void SetModelItemMSDeployExecutionTypeValue(System.Activities.Presentation.Model.ModelItem modelItem, string modelItemPropertyName, MSDeployExecutionType value)
		{
			var newExpression = new Microsoft.VisualBasic.Activities.VisualBasicValue<MSDeployExecutionType>(typeof(MSDeployExecutionType).FullName + "." + value.ToString());
			var newValue = new InArgument<MSDeployExecutionType>(newExpression);
			modelItem.Properties[modelItemPropertyName].SetValue(newValue);
		}

		public MSDeployExecutionType? GetModelItemMSDeployExecutionTypeValue(System.Activities.Presentation.Model.ModelItem modelItem, string modelItemPropertyName)
		{
			if (modelItem.Properties[modelItemPropertyName] != null && modelItem.Properties[modelItemPropertyName].Value != null)
			{
				var currentValue = modelItem.Properties[modelItemPropertyName].Value.GetCurrentValue();
				if (currentValue is InArgument<MSDeployExecutionType> && ((InArgument<MSDeployExecutionType>)currentValue).Expression is VisualBasicValue<MSDeployExecutionType>)
				{
					string expressionText = ((VisualBasicValue<MSDeployExecutionType>)((InArgument<MSDeployExecutionType>)currentValue).Expression).ExpressionText;
					//Enum.Parse doesn't like this
					if (expressionText == string.Format("{0}.{1}", typeof(MSDeployExecutionType).FullName, MSDeployExecutionType.Cmd.ToString()))
					{
						return MSDeployExecutionType.Cmd;
					}
					else if (expressionText == string.Format("{0}.{1}", typeof(MSDeployExecutionType).FullName, MSDeployExecutionType.Exe.ToString()))
					{
						return MSDeployExecutionType.Exe;
					}
				}
			}
			return null;
		}

		private void rdoCmd_Checked(object sender, RoutedEventArgs e)
		{
			SetModelItemMSDeployExecutionTypeValue(ModelItem, "ExecutionType", MSDeployExecutionType.Cmd);
			CmdSettings.Height = new GridLength();
			ExeSettings.Height = new GridLength(0);
		}

		private void rdoExe_Checked(object sender, RoutedEventArgs e)
		{
			SetModelItemMSDeployExecutionTypeValue(ModelItem, "ExecutionType", MSDeployExecutionType.Exe);
			ExeSettings.Height = new GridLength();
			CmdSettings.Height = new GridLength(0);
		}

		private void btnConvertCmdPathToRelative_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.ConvertExpressionTextBoxExpressionToRelativePathExpression(txtCmdFileName);
		}

		private void btnConvertZipPathToRelative_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.ConvertExpressionTextBoxExpressionToRelativePathExpression(txtZipFileName);
		}

		private void btnConvertParamsPathToRelative_Click(object sender, RoutedEventArgs e)
		{
			DesignerHelper.ConvertExpressionTextBoxExpressionToRelativePathExpression(txtParamsFileName);
		}

		private void imgVersionHelp_MouseEnter(object sender, MouseEventArgs e)
		{
			VersionHelp.PlacementTarget = imgVersionHelp;
			VersionHelp.IsOpen = true;
		}

		private void imgVersionHelp_MouseLeave(object sender, MouseEventArgs e)
		{
			VersionHelp.IsOpen = false;
		}

		private void chkUserVersion2X_Checked(object sender, RoutedEventArgs e)
		{
			cmbVersion.SelectedIndex = 1;
		}
	}
}