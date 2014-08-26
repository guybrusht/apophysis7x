﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Xyrus.Apophysis.Windows.Drawing
{
	public abstract class ControlPainter : ControlEventInterceptor
	{
		protected ControlPainter([NotNull] Control control) : base(control)
		{
		}
		protected abstract void OnControlPaint(Graphics graphics);

		public void Paint()
		{
			using (var graphics = AttachedControl.CreateGraphics())
			{
				OnControlPaint(graphics);
			}
		}
		public void Paint([NotNull] Graphics graphics)
		{
			if (graphics == null) throw new ArgumentNullException("graphics");
			OnControlPaint(graphics);
		}
	}
}