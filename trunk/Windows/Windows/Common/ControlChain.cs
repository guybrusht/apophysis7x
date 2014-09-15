using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Xyrus.Apophysis.Windows
{
	abstract class ControlChain<TControl, TChainItem> : ControlEventInterceptor where TChainItem : ChainItem<TControl> where TControl: Control
	{
		class PriorizedChainItem
		{
			[NotNull]
			public TChainItem Handler;
			public int Priority;
		}

		private List<PriorizedChainItem> mChain;

		protected ControlChain([NotNull] Control control) : base(control)
		{
			mChain = new List<PriorizedChainItem>();
		}
		protected override void DisposeOverride(bool disposing)
		{
			if (mChain != null)
			{
				foreach (var item in mChain)
					item.Handler.Dispose();

				mChain.Clear();
				mChain = null;
			}
		}

		protected IEnumerable<TChainItem> GetChainItems()
		{
			if (mChain == null)
				return new TChainItem[0];

			return mChain.OrderBy(x => x.Priority).Select(x => x.Handler);
		}

		public void Add([NotNull] TChainItem handler, int priority = 1)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			if (priority < 1) throw new ArgumentOutOfRangeException("priority");

			mChain.Add(new PriorizedChainItem { Handler = handler, Priority = priority });
		}
		public void Remove([NotNull] TChainItem painter)
		{
			if (painter == null) throw new ArgumentNullException("painter");

			if (mChain == null)
				return;

			var itemsToRemove = mChain.Where(x => ReferenceEquals(painter, x.Handler));
			foreach (var item in itemsToRemove)
			{
				mChain.Remove(item);
			}
		}
		public void Clear()
		{
			mChain.Clear();
		}
	}

	abstract class ControlChain<T> : ControlChain<Control, T> where T : ChainItem
	{
		protected ControlChain([NotNull] Control control) : base(control)
		{
		}
	}
}