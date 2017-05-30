using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Active.Builder.ExpressionEditor
{
	internal class ComparerName : IComparer<TreeNodes>
	{
		public int Compare(TreeNodes x, TreeNodes y)
		{
			if (x == null)
			{
				if (y == null)
				{
					return 0;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (y == null)
				{
					return 1;
				}
				else
				{
					return x.Name.CompareTo(y.Name);
				}
			}
		}
	}
}