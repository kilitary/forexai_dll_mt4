using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace forexAI
{
	public static class Extensions
	{
		public static int WordCount(this String str)
		{
			return str.Split(new char[] { ' ', '.', '?' },
							 StringSplitOptions.RemoveEmptyEntries).Length;
		}
	}
}