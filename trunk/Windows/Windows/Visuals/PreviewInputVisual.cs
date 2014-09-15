﻿using System.Drawing;
using System.Windows.Forms;
using Xyrus.Apophysis.Math;
using Xyrus.Apophysis.Windows.Input;
using Rectangle = System.Drawing.Rectangle;

namespace Xyrus.Apophysis.Windows.Visuals
{
	class PreviewInputVisual : ControlVisual<PictureBox>
	{
		private CameraInputOperation mOperation;
		private Size mImageSize;
		private bool mFitFrame;

		public PreviewInputVisual([NotNull] PictureBox control) : base(control)
		{
		}
		protected override void DisposeOverride(bool disposing)
		{
			mOperation = null;
		}

		public CameraInputOperation Operation
		{
			get { return mOperation; }
			set
			{
				mOperation = value;
				InvalidateControl();
			}
		}
		public Size ImageSize
		{
			get { return mImageSize; }
			set { mImageSize = value; }
		}
		public bool FitFrame
		{
			get { return mFitFrame; }
			set { mFitFrame = value; }
		}

		protected override void RegisterEvents(PictureBox control)
		{
			ImageSize = control.ClientSize;
		}
		protected override void UnregisterEvents(PictureBox control)
		{
		}

		protected override void OnControlPaint(Graphics graphics)
		{
			if (mImageSize.Width <= 0 || mImageSize.Height <= 0)
				return;

			var fractalSize = FitFrame ? AttachedControl.ClientSize : mImageSize.FitToFrame(AttachedControl.ClientSize);
			var fractalRect = new Rectangle(new Point(AttachedControl.ClientSize.Width / 2 - fractalSize.Width / 2, AttachedControl.ClientSize.Height / 2 - fractalSize.Height / 2), fractalSize);

			var pan = mOperation as PanOperation;
			if (pan != null)
			{
				var x0 = new Vector2(fractalRect.Left, fractalRect.Top);
				var x1 = new Vector2(fractalRect.Right, fractalRect.Top);
				var x2 = new Vector2(fractalRect.Right, fractalRect.Bottom);
				var x3 = new Vector2(fractalRect.Left, fractalRect.Bottom);

				var offset = System.Math.Pow(2, pan.Flame.Zoom)*pan.Flame.PixelsPerUnit*(pan.Offset - pan.Origin);

				x0 += offset;
				x1 += offset;
				x2 += offset;
				x3 += offset;

				DrawRectangle(graphics, x0, x1, x2, x3);
			}

			var rotate = mOperation as RotateCanvasOperation;
			if (rotate != null)
			{
				var x0 = new Vector2(fractalRect.Left, fractalRect.Top);
				var x1 = new Vector2(fractalRect.Right, fractalRect.Top);
				var x2 = new Vector2(fractalRect.Right, fractalRect.Bottom);
				var x3 = new Vector2(fractalRect.Left, fractalRect.Bottom);

				var o = new Vector2(AttachedControl.ClientSize.Width/2.0, AttachedControl.ClientSize.Height/2.0);

				var cos = System.Math.Cos(rotate.Angle - rotate.OriginalAngle);
				var sin = System.Math.Sin(rotate.Angle - rotate.OriginalAngle);

				x0 = new Vector2((x0.X - o.X) * cos + (x0.Y - o.Y) * sin + o.X, (x0.Y - o.Y) * cos - (x0.X - o.X) * sin + o.Y);
				x1 = new Vector2((x1.X - o.X) * cos + (x1.Y - o.Y) * sin + o.X, (x1.Y - o.Y) * cos - (x1.X - o.X) * sin + o.Y);
				x2 = new Vector2((x2.X - o.X) * cos + (x2.Y - o.Y) * sin + o.X, (x2.Y - o.Y) * cos - (x2.X - o.X) * sin + o.Y);
				x3 = new Vector2((x3.X - o.X) * cos + (x3.Y - o.Y) * sin + o.X, (x3.Y - o.Y) * cos - (x3.X - o.X) * sin + o.Y);

				DrawRectangle(graphics, x0, x1, x2, x3);
			}
		}

		private void DrawRectangle(Graphics graphics, Vector2 x0, Vector2 x1, Vector2 x2, Vector2 x3)
		{
			using (var pen = new Pen(Color.White, 1.0f))
			{
				pen.DashPattern = new[] { 6.0f, 4.0f };

				graphics.DrawLine(pen, x0.ToPoint(), x1.ToPoint());
				graphics.DrawLine(pen, x1.ToPoint(), x2.ToPoint());
				graphics.DrawLine(pen, x2.ToPoint(), x3.ToPoint());
				graphics.DrawLine(pen, x3.ToPoint(), x0.ToPoint());
			}
		}
	}
}