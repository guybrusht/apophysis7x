using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Xyrus.Apophysis.Models;
using Xyrus.Apophysis.Windows.Controls;
using Xyrus.Apophysis.Windows.Forms;

namespace Xyrus.Apophysis.Windows.Controllers
{
	public class EditorController : WindowController<Editor>
	{
		private static readonly double[] mMoveOffsetOptions = { 1.0, 0.5, 0.25, 0.05, 0.025, 0.01 };
		private static readonly double[] mSnapAngleOptions = { 5.0, 15.0, 30.0, 45.0, 60.0, 90.0, 120.0, 180.0 };
		private static readonly double[] mScaleRatioOptions = { 110.0, 125.0, 150.0, 175.0, 200.0 };

		private Lock mInitialize = new Lock();
		private Flame mFlame;

		protected override void DisposeOverride(bool disposing)
		{
			if (disposing)
			{
				if (mInitialize != null)
				{
					mInitialize.Dispose();
					mInitialize = null;
				}

				if (mFlame != null)
				{
					mFlame.Iterators.ContentChanged -= OnIteratorCollectionChanged;
					mFlame = null;
				}
			}
		}

		protected override void AttachView()
		{
			View.IteratorWeightDragPanel.ValueChanged += OnWeightChanged;
			View.IteratorNameTextBox.TextChanged += OnNameChanged;
			View.IteratorSelectionComboBox.SelectedIndexChanged += OnIteratorSelectedFromComboBox;
			View.IteratorCanvas.SelectionChanged += OnIteratorSelectedFromCanvas;

			View.IteratorColorDragPanel.ValueChanged += OnColorChanged;
			View.IteratorColorSpeedDragPanel.ValueChanged += OnColorSpeedChanged;
			View.IteratorOpacityDragPanel.ValueChanged += OnOpacityChanged;
			View.IteratorDirectColorDragPanel.ValueChanged += OnDirectColorChanged;
			View.IteratorColorScrollBar.ValueChanged += OnColorChanged;

			View.KeyHandler = OnKeyDown;

			View.IteratorCanvas.Settings = new EditorSettings
			{
				MoveAmount = ApophysisSettings.EditorMoveDistance,
				AngleSnap = ApophysisSettings.EditorRotateAngle,
				ScaleSnap = ApophysisSettings.EditorScaleRatio
			};

			View.IteratorRotate90CCW.Click += OnIteratorRotateClick;
			View.IteratorRotate90CW.Click += OnIteratorRotateClick;
			View.IteratorRotateCCW.Click += OnIteratorRotateClick;
			View.IteratorRotateCW.Click += OnIteratorRotateClick;

			View.IteratorSnapAngle.Items.AddRange(mSnapAngleOptions.OfType<object>().ToArray());
			View.IteratorSnapAngle.Text = View.IteratorCanvas.Settings.AngleSnap.ToString(InputController.Culture);
			View.IteratorSnapAngle.TextChanged += OnSnapAngleChanged;

			SetFlame(new Flame());
		}
		protected override void DetachView()
		{
			View.IteratorWeightDragPanel.ValueChanged -= OnWeightChanged;
			View.IteratorNameTextBox.TextChanged -= OnNameChanged;
			View.IteratorSelectionComboBox.SelectedIndexChanged -= OnIteratorSelectedFromComboBox;
			View.IteratorCanvas.SelectionChanged -= OnIteratorSelectedFromCanvas;

			View.IteratorColorDragPanel.ValueChanged -= OnColorChanged;
			View.IteratorColorSpeedDragPanel.ValueChanged -= OnColorSpeedChanged;
			View.IteratorOpacityDragPanel.ValueChanged -= OnOpacityChanged;
			View.IteratorDirectColorDragPanel.ValueChanged -= OnDirectColorChanged;
			View.IteratorColorScrollBar.ValueChanged -= OnColorChanged;

			View.IteratorRotate90CCW.Click -= OnIteratorRotateClick;
			View.IteratorRotate90CW.Click -= OnIteratorRotateClick;
			View.IteratorRotateCCW.Click -= OnIteratorRotateClick;
			View.IteratorRotateCW.Click -= OnIteratorRotateClick;

			View.IteratorSnapAngle.TextChanged -= OnSnapAngleChanged;

			View.KeyHandler = null;
		}

		public void SetFlame([NotNull] Flame flame)
		{
			if (flame == null) throw new ArgumentNullException("flame");

			if (mFlame != null)
			{
				mFlame.Iterators.ContentChanged -= OnIteratorCollectionChanged;
			}

			mFlame = flame;
			mFlame.Iterators.ContentChanged += OnIteratorCollectionChanged;

			using (mInitialize.Enter())
			{
				View.IteratorCanvas.Iterators = mFlame.Iterators;
				
				BuildIteratorComboBox();
				SelectIterator(mFlame.Iterators.First());
			}
		}
		public void ReadFlameFromClipboard()
		{
			var clipboard = Clipboard.GetText();
			if (!string.IsNullOrEmpty(clipboard))
			{
				try
				{
					var flame = new Flame();
					
					flame.ReadXml(XElement.Parse(clipboard, LoadOptions.None));
					SetFlame(flame);
				}
				catch (ApophysisException exception)
				{
					MessageBox.Show(exception.Message, View.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				catch (XmlException exception)
				{
					MessageBox.Show(exception.Message, View.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void BuildIteratorComboBox()
		{
			View.IteratorSelectionComboBox.Items.Clear();
			View.IteratorSelectionComboBox.Items.AddRange(mFlame
				.Iterators
				.Select(GetIteratorDisplayName)
				.OfType<object>()
				.ToArray());
		}
		private void SelectIterator([CanBeNull] Iterator iterator)
		{
			View.IteratorCanvas.SelectedIterator = iterator;

			using (mInitialize.Enter())
			{
				View.IteratorSelectionComboBox.SelectedIndex = iterator == null ? -1 : iterator.Index;
				View.IteratorNameTextBox.Text = iterator == null ? null : iterator.Name;
				View.IteratorWeightDragPanel.Value = iterator == null ? 0.5 : iterator.Weight;
				View.IteratorColorDragPanel.Value = iterator == null ? 0.0 : iterator.Color;
				View.IteratorColorSpeedDragPanel.Value = iterator == null ? 0.0 : iterator.ColorSpeed;
				View.IteratorOpacityDragPanel.Value = iterator == null ? 1.0 : iterator.Opacity;
				View.IteratorDirectColorDragPanel.Value = iterator == null ? 1.0 : iterator.DirectColor;
				View.IteratorColorScrollBar.Value = iterator == null ? 0 : (int)(iterator.Color * 1000);
			}
		}

		private string GetIteratorDisplayName(Iterator iterator)
		{
			if (iterator == null)
				return null;

			var name = string.IsNullOrEmpty(iterator.Name)
				? (iterator.Index + 1).ToString(CultureInfo.CurrentCulture)
				: string.Format("{0} - {1}", iterator.Index + 1, iterator.Name);

			return name;
		}

		private void OnNameChanged(object sender, EventArgs e)
		{
			var iterator = View.IteratorCanvas.SelectedIterator;
			if (mInitialize.IsBusy || iterator == null)
				return;

			iterator.Name = View.IteratorNameTextBox.Text;

			using (mInitialize.Enter())
			{
				View.IteratorSelectionComboBox.Items.RemoveAt(iterator.Index); 
				View.IteratorSelectionComboBox.Items.Insert(iterator.Index, GetIteratorDisplayName(iterator));
				View.IteratorSelectionComboBox.SelectedIndex = iterator.Index;
			}
		}
		private void OnWeightChanged(object sender, EventArgs e)
		{
			var iterator = View.IteratorCanvas.SelectedIterator;
			if (mInitialize.IsBusy || iterator == null)
				return;

			iterator.Weight = View.IteratorWeightDragPanel.Value;
		}
		private void OnColorChanged(object sender, EventArgs e)
		{
			var iterator = View.IteratorCanvas.SelectedIterator;
			if (mInitialize.IsBusy || iterator == null)
				return;

			if (ReferenceEquals(sender, View.IteratorColorDragPanel))
			{
				iterator.Color = View.IteratorColorDragPanel.Value;
				using (mInitialize.Enter())
				{
					View.IteratorColorScrollBar.Value = (int)(iterator.Color * 1000);
				}
			}
			else if (ReferenceEquals(sender, View.IteratorColorScrollBar))
			{
				iterator.Color = View.IteratorColorScrollBar.Value / 1000.0;
				using (mInitialize.Enter())
				{
					View.IteratorColorDragPanel.Value = iterator.Color;
				}
			}
		}
		private void OnColorSpeedChanged(object sender, EventArgs e)
		{
			var iterator = View.IteratorCanvas.SelectedIterator;
			if (mInitialize.IsBusy || iterator == null)
				return;

			iterator.ColorSpeed = View.IteratorColorSpeedDragPanel.Value;
		}
		private void OnOpacityChanged(object sender, EventArgs e)
		{
			var iterator = View.IteratorCanvas.SelectedIterator;
			if (mInitialize.IsBusy || iterator == null)
				return;

			iterator.Opacity = View.IteratorOpacityDragPanel.Value;
		}
		private void OnDirectColorChanged(object sender, EventArgs e)
		{
			var iterator = View.IteratorCanvas.SelectedIterator;
			if (mInitialize.IsBusy || iterator == null)
				return;

			iterator.DirectColor = View.IteratorDirectColorDragPanel.Value;
		}

		private void OnIteratorSelectedFromCanvas(object sender, EventArgs e)
		{
			var iterator = View.IteratorCanvas.SelectedIterator;
			if (mInitialize.IsBusy)
				return;

			SelectIterator(iterator);
		}
		private void OnIteratorSelectedFromComboBox(object sender, EventArgs e)
		{
			var iterator = View.IteratorSelectionComboBox.SelectedIndex < 0 || mFlame == null ? null : 
				mFlame.Iterators[View.IteratorSelectionComboBox.SelectedIndex];

			if (mInitialize.IsBusy)
				return;

			SelectIterator(iterator);
		}
		private void OnIteratorCollectionChanged(object sender, EventArgs e)
		{
			BuildIteratorComboBox();
		}

		private void OnIteratorRotateClick(object sender, EventArgs e)
		{
			double angle;

			if (ReferenceEquals(sender, View.IteratorRotate90CCW))
			{
				angle = -90;
			}
			else if (ReferenceEquals(sender, View.IteratorRotate90CW))
			{
				angle = 90;
			}
			else if (ReferenceEquals(sender, View.IteratorRotateCCW))
			{
				angle = -View.IteratorCanvas.Settings.AngleSnap;
			}
			else if (ReferenceEquals(sender, View.IteratorRotateCW))
			{
				angle = View.IteratorCanvas.Settings.AngleSnap;
			}
			else
			{
				return;
			}

			while (angle < 0)
			{
				angle = 360 + angle;
			}

			View.IteratorCanvas.Commands.RotateSelected(-angle * System.Math.PI / 180.0);
		}
		private void OnSnapAngleChanged(object sender, EventArgs e)
		{
			double value;
			if (!double.TryParse(View.IteratorSnapAngle.Text, NumberStyles.Float, InputController.Culture, out value))
			{
				value = View.IteratorCanvas.Settings.AngleSnap;
			}

			View.IteratorCanvas.Settings.AngleSnap = value;
		}

		private void OnKeyDown(Keys keys)
		{
			if ((keys & Keys.Control) == Keys.Control && (keys & Keys.V) == Keys.V)
			{
				ReadFlameFromClipboard();
			}
		}
	}
}