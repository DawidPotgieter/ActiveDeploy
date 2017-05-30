using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace Active.Builder.ExpressionEditor
{
	/// <summary>
	/// Syntax Highlight TextBox
	/// </summary>
	/// <remarks>http://blogs.microsoft.co.il/blogs/tamir/archive/2006/12/14/RichTextBox-syntax-highlighting.aspx</remarks>
	public class HilightTextBox : RichTextBox
	{
		private Regex keyWords;
		private Regex stringMatch = new Regex("\\\"(.*?)\\\"");

		private List<WordTag> wordTagList = new List<WordTag>();
		private char[] chrs = { '.', ')',	'(', '[',	']', '>', '<', ':', ';', '\r', '\n', '\t', };
		private List<char> specials = new List<char>();

		internal string HighlightWords
		{
			set
			{
				if (value == null)
				{
					keyWords = null;
				}
				else
				{
					keyWords = new Regex(value);
				}
			}
		}
		internal TreeNodes IntellisenceList { get; set; }

		public int MaxLines { get; set; }
		public int MinLines { get; set; }

		public string Text { get; set; }

		public int SelectionStart { get; set; }
		public HilightTextBox()
		{
			specials.AddRange(chrs);
		}

		private void OnTextChangedPrivate(object sender, TextChangedEventArgs e)
		{
			if (this.Document == null)
				return;

			TextRange docRange = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
			docRange.ClearAllProperties();

			TextPointer navigator = this.Document.ContentStart;
			while ((navigator.CompareTo(this.Document.ContentEnd) < 0))
			{
				TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
				if ((context == TextPointerContext.ElementStart) && (navigator.Parent is System.Windows.Documents.Run))
				{
					CheckWordsInRun(navigator.Parent);
				}
				navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
			}
			FormatText();
		}

		private void FormatText()
		{
			this.TextChanged -= OnTextChangedPrivate;

			System.Threading.Tasks.Parallel.ForEach(wordTagList, target =>
			{
				TextRange range = new TextRange(target.StartPosition, target.EndPosition);
				range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));
			});
			wordTagList.Clear();

			this.TextChanged += OnTextChangedPrivate;
		}

		private void CheckWordsInRun(DependencyObject runSection)
		{
			string text = ((System.Windows.Documents.Run)runSection).Text;

			int stIndex = 0;
			int edIndex = 0;
			for (int index = 0; index <= text.Length; index++)
			{
				if (!(char.IsWhiteSpace(text[index]) || (specials.Contains(text[index]))))
				{
					if ((index > 0) && (!char.IsWhiteSpace(text[index - 1]) || specials.Contains(text[index - 1])))
					{
						edIndex = index - 1;
						string word = text.Substring(stIndex, edIndex - stIndex + 1);
					}
				}
			}
		}

		internal struct WordTag
		{
			public TextPointer StartPosition;
			public TextPointer EndPosition;
			public string Word;
		}
	}
}