using System;
using System.Text;

namespace Active.Activities.Helpers
{
	public class ExceptionManager
	{
		public static string GetExceptionMessage(Exception ex, int indent = 0)
		{
			string nl = Environment.NewLine;
			string indentString = new string('\t', indent);

			StringBuilder sb = new StringBuilder();
			sb.Append(indentString + "Message : " + ex.Message ?? "" + nl + nl);
			sb.Append(indentString + ex.StackTrace ?? "" + nl + nl);
			if (ex.Data != null && ex.Data.Count > 0)
				sb.Append(indentString + "Data : " + GetDataAsString(ex.Data) + nl + nl);
			if (ex.InnerException != null)
				sb.Append(nl + indentString + "InnerException : " + nl + GetExceptionMessage(ex.InnerException, ++indent) + nl);
			return sb.ToString();
		}

		private static string GetDataAsString(System.Collections.IDictionary iDictionary)
		{
			try
			{
				StringBuilder sb = new StringBuilder();
				foreach (System.Collections.DictionaryEntry item in iDictionary)
				{
					sb.Append(item.Key.ToString() + " : " + item.Value.ToString() + Environment.NewLine);
				}
				return sb.ToString();
			}
			catch (Exception ex)
			{
				return string.Format("Oops. GetDataAsString threw '{0}' when trying to read the Data field of the exception.", ex.Message);
			}
		}
	}
}
