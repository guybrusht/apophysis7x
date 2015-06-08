﻿using System;

namespace Xyrus.Apophysis.Windows.Controllers
{
	class SettingsEditorController : Controller<Forms.Settings>
	{
		private SettingsController mParent;

		public SettingsEditorController([NotNull] Forms.Settings view, [NotNull] SettingsController parent)
			: base(view)
		{
			if (parent == null) throw new ArgumentNullException("parent");
			mParent = parent;
		}
		protected override void DisposeOverride(bool disposing)
		{
			mParent = null;
		}

		protected override void AttachView()
		{
		}
		protected override void DetachView()
		{
		}

		public void Update()
		{
			View.CameraEditUseScaleCheckBox.Checked = ApophysisSettings.Editor.CameraEditUseScale;
		}
		public void WriteSettings()
		{
			ApophysisSettings.Editor.CameraEditUseScale = View.CameraEditUseScaleCheckBox.Checked;
		}
	}
}