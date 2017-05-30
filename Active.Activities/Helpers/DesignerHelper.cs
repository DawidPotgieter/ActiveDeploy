using System;
using System.IO;
using System.Activities;
using System.Activities.Presentation.Model;
using System.ComponentModel;
using Microsoft.VisualBasic.Activities;
using System.Windows.Forms;

namespace Active.Activities.Helpers
{
	public class DesignerHelper
	{
		public static string ConvertFullPathToRelativePathExpressionText(string expressionText)
		{
			string relativePathExpressionText = expressionText;
			expressionText = expressionText.Trim('"');
			string currentPath = System.AppDomain.CurrentDomain.BaseDirectory;

			try
			{
				if (Path.GetPathRoot(expressionText) == Path.GetPathRoot(currentPath))  //If they're not on the same volume, it can't be converted to virtual path
				{
					string fileName = Path.GetFileName(expressionText);
					string[] fullPathParts = expressionText.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
					string[] currentPathParts = currentPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
					int index = 0;
					relativePathExpressionText = string.Empty;
					while (fullPathParts[index] == currentPathParts[index])
					{
						index++;
					}
					int virtualIndex = index;
					while (virtualIndex < currentPathParts.Length)
					{
						relativePathExpressionText += "..\\";
						virtualIndex++;
					}
					while (index < fullPathParts.Length && (fullPathParts[index] != fileName))
					{
						relativePathExpressionText += fullPathParts[index] + "\\";
						index++;
					}
					relativePathExpressionText = string.Format("System.AppDomain.CurrentDomain.BaseDirectory + \"{0}{1}\"", relativePathExpressionText, fileName);
				}
			}
			catch { }
			
			return relativePathExpressionText;
		}

		public static void ConvertExpressionTextBoxExpressionToRelativePathExpression(System.Activities.Presentation.View.ExpressionTextBox textBox)
		{
			if (textBox.Expression != null)
			{
				if (textBox.Expression.Properties["ExpressionText"] != null)
				{
					string currentValue = textBox.Expression.Properties["ExpressionText"].Value.ToString();
					textBox.Expression.Properties["ExpressionText"].SetValue(ConvertFullPathToRelativePathExpressionText(currentValue));
				}
			}
		}

		public static void SetModelItemExpressionTextValue(ModelItem modelItem, string modelItemPropertyName, string value)
		{
			if (value == null)
			{
				modelItem.Properties[modelItemPropertyName].SetValue(null);
			}
			else
			{
				var newExpression = new Microsoft.VisualBasic.Activities.VisualBasicValue<string>(value);
				var newValue = new InArgument<string>(newExpression);
				modelItem.Properties[modelItemPropertyName].SetValue(newValue);
			}
		}

		public static string GetModelItemExpressionTextValue(ModelItem modelItem, string modelItemPropertyName)
		{
			if (modelItem.Properties[modelItemPropertyName] != null && modelItem.Properties[modelItemPropertyName].Value != null)
			{
				var currentValue = modelItem.Properties[modelItemPropertyName].Value.GetCurrentValue();
				if (currentValue is InArgument<string> && ((InArgument<string>)currentValue).Expression is VisualBasicValue<string>)
				{
					return ((VisualBasicValue<string>)((InArgument<string>)currentValue).Expression).ExpressionText;
				}
			}
			return null;
		}

		public static void SetInArgumentModelItemIntValue(ModelItem modelItem, string modelItemPropertyName, int? value)
		{
			if (value == null)
			{
				modelItem.Properties[modelItemPropertyName].SetValue(null);
			}
			else
			{
				var newExpression = new System.Activities.Expressions.Literal<int>(value.Value);
				var newValue = new InArgument<int>(newExpression);
				modelItem.Properties[modelItemPropertyName].SetValue(newValue);
			}
		}

		public static int? GetInArgumentModelItemIntValue(ModelItem modelItem, string modelItemPropertyName)
		{
			if (modelItem.Properties[modelItemPropertyName] != null && modelItem.Properties[modelItemPropertyName].Value != null)
			{
				var currentValue = modelItem.Properties[modelItemPropertyName].Value.GetCurrentValue();
				if (currentValue is InArgument<int> && ((InArgument<int>)currentValue).Expression is System.Activities.Expressions.Literal<int>)
				{
					return ((System.Activities.Expressions.Literal<int>)(((InArgument<int>)(currentValue)).Expression)).Value;
				}
			}
			return null;
		}

		public static void PickFile(ModelItem modelItem, string filter, string modelItemPropertyName, bool convertToRelativePath = true)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Filter = filter;
			var result = openFileDialog.ShowDialog();
			if (result.HasValue && result.Value)
			{
				var newExpression = new Microsoft.VisualBasic.Activities.VisualBasicValue<string>("\"" + openFileDialog.FileName + "\"");
				if (convertToRelativePath)
				{
					newExpression.ExpressionText = DesignerHelper.ConvertFullPathToRelativePathExpressionText(newExpression.ExpressionText);
				}
				var newValue = new InArgument<string>(newExpression);
				modelItem.Properties[modelItemPropertyName].SetValue(newValue);
			}
		}

		public static void PickFolder(ModelItem modelItem, string modelItemPropertyName, bool convertToRelativePath = true)
		{
			FolderBrowserDialog browseFolderDialog = new FolderBrowserDialog();
			browseFolderDialog.ShowNewFolderButton = false;
			var result = browseFolderDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				var newExpression = new Microsoft.VisualBasic.Activities.VisualBasicValue<string>("\"" + browseFolderDialog.SelectedPath + "\"");
				if (convertToRelativePath)
				{
					newExpression.ExpressionText = DesignerHelper.ConvertFullPathToRelativePathExpressionText(newExpression.ExpressionText);
				}
				var newValue = new InArgument<string>(newExpression);
				modelItem.Properties[modelItemPropertyName].SetValue(newValue);
			}
		}

		public static object GetDefaultValueAttributeValue(object container, string propertyName)
		{
			try
			{
				AttributeCollection attributes = TypeDescriptor.GetProperties(container)[propertyName].Attributes;
				DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
				return defaultValueAttribute.Value;
			}
			catch { }
			return null;
		}
	}
}
