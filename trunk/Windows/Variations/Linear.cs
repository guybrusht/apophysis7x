﻿using Xyrus.Apophysis.Calculation;

namespace Xyrus.Apophysis.Variations
{
	class Linear : Variation
	{
		private bool m15C;

		public override void Prepare(IterationData data)
		{
			m15C = VariationsIn15CStyle;
		}

		public override void Calculate(IterationData data)
		{
			data.PostX += Weight * data.PreX;
			data.PostY += Weight * data.PreY;

			if (m15C)
			{
				data.PostZ += Weight * data.PreZ;
			}
		}
	}
}
