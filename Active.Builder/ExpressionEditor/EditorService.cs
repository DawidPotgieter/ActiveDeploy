using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Activities.Presentation.View;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Hosting;
using System.Linq;
using System.Windows.Controls;

namespace Active.Builder.ExpressionEditor
{
	public class EditorService : IExpressionEditorService
	{

		internal TreeNodes IntellisenseData { get; set; }
		internal string EditorKeyWord { get; set; }

		private Dictionary<string, EditorInstance> editorInstances = new Dictionary<string, EditorInstance>();

		public void CloseExpressionEditors()
		{
			foreach (var editor in editorInstances.Values)
			{
				editor.LostAggregateFocus -= LostFocus;
			}
			editorInstances.Clear();
		}

		public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, System.Collections.Generic.List<ModelItem> variables, string text)
		{
			return CreateExpressionEditorPrivate(assemblies, importedNamespaces, variables, text, null, new System.Windows.Size());
		}

		public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, System.Collections.Generic.List<ModelItem> variables, string text, System.Type expressionType)
		{
			return CreateExpressionEditorPrivate(assemblies, importedNamespaces, variables, text, expressionType, new System.Windows.Size());
		}

		public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, System.Collections.Generic.List<ModelItem> variables, string text, System.Type expressionType, System.Windows.Size initialSize)
		{
			return CreateExpressionEditorPrivate(assemblies, importedNamespaces, variables, text, expressionType, initialSize);
		}

		public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, System.Collections.Generic.List<ModelItem> variables, string text, System.Windows.Size initialSize)
		{
			return CreateExpressionEditorPrivate(assemblies, importedNamespaces, variables, text, null, initialSize);
		}

		public void UpdateContext(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces)
		{
		}

		private object _intellisenseLock = new object();
		private TreeNodes CreateUpdatedIntellisense(List<ModelItem> vars)
		{
			TreeNodes result = IntellisenseData;
			lock (_intellisenseLock)
			{
				foreach (var vs in vars)
				{
					ModelProperty vsProp = vs.Properties["Name"];
					if (vsProp == null)
						continue;
					string varName = (string)vsProp.ComputedValue;
					IEnumerable<TreeNodes> res = result.Nodes.Where(x => x.Name == varName);

					if (res.FirstOrDefault() == null)
					{
						Type sysType = null;
						ModelProperty sysTypeProp = vs.Properties["Type"];
						if (sysTypeProp != null)
						{
							sysType = (Type)sysTypeProp.ComputedValue;
						}
						TreeNodes newVar = new TreeNodes
						{
							Name = varName,
							ItemType = TreeNodes.NodeTypes.Primitive,
							SystemType = sysType,
							Description = ""
						};
						result.Nodes.Add(newVar);
					}
				}
			}
			return result;
		}

		private IExpressionEditorInstance CreateExpressionEditorPrivate(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, System.Collections.Generic.List<ModelItem> variables, string text, System.Type expressionType, System.Windows.Size initialSize)
		{
			EditorInstance editor = new EditorInstance
			{
				IntellisenseList = this.CreateUpdatedIntellisense(variables),
				HighlightWords = this.EditorKeyWord,
				ExpressionType = expressionType,
				Guid = Guid.NewGuid(),
				Text = text,
			};
			editor.LostAggregateFocus += LostFocus;

			editorInstances.Add(editor.Guid.ToString(), editor);
			return editor;
		}

		private void LostFocus(object sender, EventArgs e)
		{
			dynamic edt = sender as TextBox;
			if (edt != null)
				DesignerView.CommitCommand.Execute(edt.Text);
		}
	}
}
