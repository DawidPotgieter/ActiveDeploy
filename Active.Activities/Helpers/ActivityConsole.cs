using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Activities;

namespace Active.Activities.Helpers
{
	public class ActivityConsole : TextWriter
	{
		public event EventHandler<ConsoleDataArrivedEventArgs> dataArrived;

		private IList<string> strings = new List<string>();
		public IList<string> Strings
		{
			get
			{
				return strings;
			}
		}

		public int LastShowIndex { get; set; }

		public override void WriteLine(string text)
		{
			strings.Add(text);
			if (dataArrived != null)
				dataArrived(this, new ConsoleDataArrivedEventArgs { Data = text });
		}

		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}

		public static ActivityConsole GetDefaultOrNew(CodeActivityContext context)
		{
			ActivityConsole console = context.GetExtension<ActivityConsole>();
			if (console == null) console = new ActivityConsole();
			return console;
		}
	}
}
