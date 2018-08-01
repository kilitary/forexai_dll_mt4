using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace forexAI
{
	public static class Extensions
	{
		public static int WordCount(this string str) => str.Split(new char[] { ' ', '.', '?', ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
	}
}