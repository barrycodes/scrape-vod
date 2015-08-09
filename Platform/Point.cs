using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Platform
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Point
	{
		public int X;
		public int Y;

		public Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public Point(System.Drawing.Point pt) : this(pt.X, pt.Y) { }

		public static implicit operator System.Drawing.Point(Point p)
		{
			return new System.Drawing.Point(p.X, p.Y);
		}

		public static implicit operator Point(System.Drawing.Point p)
		{
			return new Point(p.X, p.Y);
		}

		public System.Drawing.Point ToPoint()
		{
			return new System.Drawing.Point(X, Y);
		}

		public static Point FromPoint(System.Drawing.Point p)
		{
			return new Point { X = p.X, Y = p.Y };
		}
	}
}
