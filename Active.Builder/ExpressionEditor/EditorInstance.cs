using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Activities.Presentation.View;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Active.Builder.ExpressionEditor
{
	public class EditorInstance : IExpressionEditorInstance
	{

		private TextBox editor = null;
		private Popup isPopup = null;
		private string _startText = "";

		public EditorInstance()
		{
			editor = new TextBox();
			editor.KeyDown += EditorKeyDown;
			editor.TextChanged += EditorTextChanged;
			editor.PreviewKeyDown += EditorKeyPress;
			editor.LostFocus += EditorLostFocus;
		}

		public event EventHandler TextChanged;
		public event EventHandler Closing;
		public event EventHandler GotAggregateFocus;
		public event EventHandler LostAggregateFocus;

		internal TreeNodes IntellisenseList { get; set; }
		internal string HighlightWords { get; set; }
		internal Type ExpressionType { get; set; }
		internal Guid Guid { get; set; }

		public bool AcceptsReturn
		{
			get { return editor.AcceptsReturn; }
			set
			{
				if (editor.AcceptsReturn != value)
					editor.AcceptsReturn = value;
			}
		}

		public bool AcceptsTab
		{
			get { return editor.AcceptsTab; }
			set
			{
				if (editor.AcceptsTab != value)
					editor.AcceptsTab = value;
			}
		}

		public bool HasAggregateFocus
		{
			get { return true; }
		}

		public System.Windows.Controls.ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return editor.HorizontalScrollBarVisibility; }
			set { editor.HorizontalScrollBarVisibility = value; }
		}

		public System.Windows.Controls.Control HostControl
		{
			get { return editor; }
		}

		public int MaxLines
		{
			get { return editor.MaxLines; }
			set
			{
				if (editor.MaxLines != value)
					editor.MaxLines = value;
			}
		}

		public int MinLines
		{
			get { return editor.MinLines; }
			set
			{
				if (editor.MinLines != value)
					editor.MinLines = value;
			}
		}

		public string Text
		{
			get { return editor.Text; }
			set { editor.Text = value; }
		}

		public System.Windows.Controls.ScrollBarVisibility VerticalScrollBarVisibility
		{
			get { return editor.VerticalScrollBarVisibility; }
			set { editor.VerticalScrollBarVisibility = value; }
		}
		public bool CanCompleteWord()
		{
			return true;
		}

		public bool CanCopy()
		{
			return true;
		}

		public bool CanCut()
		{
			return true;
		}

		public bool CanDecreaseFilterLevel()
		{
			return false;
		}

		public bool CanGlobalIntellisense()
		{
			return false;
		}

		public bool CanIncreaseFilterLevel()
		{
			return false;
		}

		public bool CanParameterInfo()
		{
			return false;
		}

		public bool CanPaste()
		{
			return true;
		}

		public bool CanQuickInfo()
		{
			return false;
		}

		public bool CanRedo()
		{
			return editor.CanRedo;
		}

		public bool CanUndo()
		{
			return editor.CanUndo;
		}

		public void ClearSelection()
		{
			return;
		}

		public void Close()
		{
			return;
		}

		public bool CompleteWord()
		{
			return true;
		}

		public bool Copy()
		{
			return true;
		}

		public bool Cut()
		{
			return true;
		}

		public bool DecreaseFilterLevel()
		{
			return false;
		}

		public void Focus()
		{
			editor.Focus();
		}

		public string GetCommittedText()
		{
			return editor.Text;
		}

		public bool GlobalIntellisense()
		{
			return false;
		}

		public bool IncreaseFilterLevel()
		{
			return false;
		}

		public bool ParameterInfo()
		{
			return false;
		}

		public bool Paste()
		{
			return true;
		}

		public bool QuickInfo()
		{
			return false;
		}

		public bool Redo()
		{
			return true;
		}

		public bool Undo()
		{
			return true;
		}

		private void EditorTextChanged(object sender, EventArgs e)
		{
			this.Text = editor.Text;

			if ((isPopup == null))
				return;

			string inpText = this.GetInputingText();
			TreeNodes targetNode = this.SearchNodes(IntellisenseList, inpText);
			if (targetNode == null)
				targetNode = IntellisenseList;

			string targetText = inpText.ToLower();
			if (targetText.EndsWith("."))
			{
				targetText = targetText.Substring(0, targetText.Length - 1);
			}
			var searchList = targetNode.Nodes.Where(x => x.GetFullPath().ToLower().Contains(targetText));

			if (IsVarOrArg(targetText))
			{
				TreeNodes itemNode = this.SearchNodes(IntellisenseList, targetText);
				if (itemNode != null)
				{
					TreeNodes itemTypeNode = this.SearchNodes(IntellisenseList, itemNode.SystemType.FullName);
					if ((itemTypeNode != null) && (itemTypeNode.Nodes.Count > 0))
						searchList = searchList.Union(itemTypeNode.Nodes);
				}
			}

			if ((searchList == null) || (searchList.Count() <= 0))
			{
				this.UnInitializePopup();
				return;
			}

			isPopup.DataContext = null;
			isPopup.DataContext = searchList;

		}

		private bool IsVarOrArg(string inputText)
		{
			bool result = false;

			var searchList = IntellisenseList.Nodes.Where(x => x.Name.ToLower() == inputText.ToLower() && x.SystemType != null);

			if ((searchList != null) && (searchList.Count() > 0))
			{
				result = true;
			}

			return result;
		}

		private string GetInputingText()
		{
			var inpText = editor.Text;
			if (inpText.ToLower().StartsWith("new"))
			{
				inpText = inpText.Substring(3).Trim();
			}
			return inpText;
		}


		private void EditorKeyDown(object sender, KeyEventArgs e)
		{
			if ((!this.AcceptsReturn && e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None) || (!this.AcceptsTab && e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.None))
			{
				e.Handled = true;

				TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
				var uIElement = this.GetFocusedElement();
				if (uIElement != null)
					uIElement.MoveFocus(request);
				return;
			}
		}

		private void EditorKeyPress(object sender, KeyEventArgs e)
		{
			if ((e.Key == Key.Up))
			{
				if ((isPopup == null) || (!isPopup.IsOpen))
					return;
				isPopup.SelectedIndex -= 1;
				e.Handled = true;
			}

			if ((e.Key == Key.Down))
			{
				if ((isPopup == null) || (!isPopup.IsOpen))
					return;
				isPopup.SelectedIndex += 1;
				e.Handled = true;
			}

			if (e.Key == Key.Escape)
			{
				if (isPopup != null)
					isPopup.IsOpen = false;
				_startText = "";
				e.Handled = true;
				return;
			}

			if ((e.Key == Key.Decimal || e.Key == Key.OemPeriod || (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)))
			{
				if ((isPopup != null) && (isPopup.IsOpen))
					return;
				var searchWord = editor.Text;
				if ((e.Key == Key.Decimal || e.Key == Key.OemPeriod))
				{
					searchWord += ".";
				}
				_startText = GetCommitedWord(searchWord);

				if (searchWord.EndsWith("."))
					searchWord = searchWord.Substring(0, searchWord.Length - 1);
				if (searchWord.ToLower().StartsWith("new "))
					searchWord = searchWord.Substring(4);
				var targetNodes = this.SearchNodes(IntellisenseList, searchWord);
				if (targetNodes == null)
					targetNodes = IntellisenseList;
				var isSource = targetNodes.Nodes.ToList();
				this.InitializePopup(isSource);
				isPopup.IsOpen = true;
				if ((e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control))
					e.Handled = true;
				return;
			}

			if ((e.Key == Key.Home))
			{
				if ((isPopup == null) || (!isPopup.IsOpen))
					return;
				isPopup.SelectedIndex = 0;
				e.Handled = true;
			}

			if ((e.Key == Key.End))
			{
				if ((isPopup == null) || (!isPopup.IsOpen))
					return;
				isPopup.SelectedIndex = isPopup.ItemsCount - 1;
				e.Handled = true;
			}

			if ((e.Handled == false) && ((e.Key == Key.Enter) || (e.Key == Key.Space) || (e.Key == Key.Tab)))
			{
				if ((isPopup != null) && (isPopup.IsOpen))
				{
					var isSource = isPopup.SelectedItem;
					if (isSource == null)
						return;
					this.CommitIntellisenseItem(isSource);
					e.Handled = true;
				}
			}
		}

		private void EditorLostFocus(object sender, EventArgs e)
		{
			var popupItem = this.GetFocusedElement() as ListBoxItem;
			if (popupItem == null)
			{
				if ((isPopup != null) && (isPopup.IsOpen))
				{
					this.UnInitializePopup();
				}
				if (LostAggregateFocus != null)
				{
					LostAggregateFocus(sender, e);
				}
			}

		}

		private void ListKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				editor.Focus();
				this.UnInitializePopup();
				return;
			}

			if ((e.Key == Key.Enter) || (e.Key == Key.Space) || (e.Key == Key.Tab))
			{
				editor.Focus();

				var isSource = isPopup.SelectedItem;
				if (isSource == null)
					return;
				var isText = isSource.GetFullPath();
				editor.Text = _startText + isText;
				editor.SelectionStart = editor.Text.Length;
				this.UnInitializePopup();
			}
		}

		private void ListItemDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = sender as ListBoxItem;
			if (item == null)
				return;
			var nodes = item.DataContext as TreeNodes;
			if (nodes == null)
				return;
			editor.Focus();
			this.CommitIntellisenseItem(nodes);
		}

		private void CommitIntellisenseItem(TreeNodes selectedNodes)
		{
			var inputText = _startText;
			inputText += selectedNodes.Name;

			editor.Text = inputText;
			editor.SelectionStart = editor.Text.Length;
			editor.UpdateLayout();
			this.UnInitializePopup();
		}

		private string GetCommitedWord(string inputedText)
		{
			var spacePos = inputedText.LastIndexOf(" ");
			var parenthesisPos = inputedText.LastIndexOf("(");
			var dotPos = inputedText.LastIndexOf(".");
			if (spacePos == -1 && parenthesisPos == -1 && dotPos == -1)
				return inputedText;

			if ((spacePos > parenthesisPos) && (dotPos == -1))
			{
				return inputedText.Substring(0, spacePos + 1);
			}
			else if ((dotPos == -1))
			{
				return inputedText.Substring(0, parenthesisPos + 1);
			}
			else
			{
				return inputedText.Substring(0, dotPos + 1);
			}
		}

		private UIElement GetFocusedElement()
		{
			return Keyboard.FocusedElement as UIElement;
		}

		private void InitializePopup(List<TreeNodes> isSource)
		{
			if ((isPopup != null) && (isPopup.IsOpen))
				this.UnInitializePopup();

			isPopup = new Popup
			{
				DataContext = isSource,
				PlacementTarget = editor,
				Placement = PlacementMode.Bottom
			};

			isPopup.ListBoxKeyDown += ListKeyDown;
			isPopup.ListBoxItemDoubleClick += ListItemDoubleClick;
		}

		private void UnInitializePopup()
		{
			if (isPopup == null)
				return;

			isPopup.ListBoxKeyDown -= ListKeyDown;
			isPopup.ListBoxItemDoubleClick -= ListItemDoubleClick;

			if (isPopup.IsOpen)
				isPopup.IsOpen = false;
			_startText = "";
			isPopup = null;
		}

		private TreeNodes SearchNodes(TreeNodes targetNodes, string namePath)
		{
			var targetPath = namePath.Split('.');
			bool validPath = false;
			TreeNodes existsNodes = null;
			foreach (var childNode in targetNodes.Nodes)
			{
				if (childNode.Name.ToLower() != targetPath[0].ToLower())
					continue;
				validPath = true;
				existsNodes = childNode;
				break; // TODO: might not be correct. Was : Exit For
			}
			if (!validPath)
				return targetNodes;

			var nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
			if (nextPath.StartsWith("."))
				nextPath = nextPath.Substring(1, nextPath.Length - 1);
			if (nextPath == null || string.IsNullOrEmpty(nextPath.Trim()))
				return existsNodes;
			return this.SearchNodes(existsNodes, nextPath);
		}
	}
}