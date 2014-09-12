using System;
using Xyrus.Apophysis.Windows.Forms;

namespace Xyrus.Apophysis.Windows.Controllers
{
	class MainToolbarController : Controller<Main>
	{
		private MainController mParent;

		public MainToolbarController([NotNull] Main view, [NotNull] MainController parent) : base(view)
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
			UpdateButtonStates();
		}
		protected override void DetachView()
		{
		}

		public void UpdateButtonStates()
		{
			View.UndoButton.Enabled = mParent.UndoController.CanUndo;
			View.RedoButton.Enabled = mParent.UndoController.CanRedo;
		}
	}
}