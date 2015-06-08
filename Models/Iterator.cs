﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using Xyrus.Apophysis.Calculation;
using Xyrus.Apophysis.Messaging;
using Xyrus.Apophysis.Strings;
using Xyrus.Apophysis.Variations;

namespace Xyrus.Apophysis.Models
{
	[PublicAPI]
	public class Iterator
	{
		private Flame mFlame;

		private Matrix3x2 mPreAffine;
		private Matrix3x2 mPostAffine;

		private readonly VariationCollection mVariations;
		private static readonly string mDefaultVariationName = ApophysisSettings.Common.VariationsIn15CStyle
			? VariationRegistry.GetName<Linear>()
			: VariationRegistry.GetName<Linear3D>();

		private string mName;
		private float mWeight;
		private float mColor;
		private float mColorSpeed;
		private float mOpacity;
		private float mDirectColor;
		private int mGroupIndex;

		public Iterator([NotNull] Flame hostingFlame)
		{
			if (hostingFlame == null) throw new ArgumentNullException(@"hostingFlame");

			mVariations = new VariationCollection();
			mFlame = hostingFlame;

			Reset();
		}

		public void Reset()
		{
			mPreAffine = new Matrix3x2(1,0,0,1,0,0);
			mPostAffine = new Matrix3x2(1,0,0,1,0,0);

			mName = null;
			mWeight= 0.5f;
			mColor = 0.0f;
			mColorSpeed = 0.0f;
			mOpacity = 1.0f;
			mDirectColor = 1.0f;

			mVariations.ClearWeights();
			mVariations.SetWeight(mDefaultVariationName, 1.0f);
		}

		[NotNull]
		public Iterator Copy()
		{
			var copy = new Iterator(mFlame)
			{
				mName = mName,
				mWeight = mWeight,
				mColor = mColor,
				mColorSpeed = mColorSpeed,
				mOpacity = mOpacity,
				mDirectColor = mDirectColor,
				mGroupIndex = mGroupIndex,
				mPreAffine = mPreAffine,
				mPostAffine = mPostAffine
			};

			copy.mVariations.ClearWeights();
			foreach (var variation in mVariations.Where(x => System.Math.Abs(x.Weight) > float.Epsilon))
			{
				copy.mVariations.SetWeight(variation.Name, variation.Weight);

				var variables = variation.EnumerateVariables();
				foreach (var variable in variables)
				{
					var value = variation.GetVariable(variable);
					copy.mVariations.SetVariable(variable, value);
				}
			}

			return copy;
		}

		[NotNull]
		public Iterator Convert(int groupIndex)
		{
			return mFlame.Iterators.ConvertIterator(this, groupIndex);
		}

		[NotNull]
		internal Iterator Copy([NotNull] Flame flame)
		{
			if (flame == null) throw new ArgumentNullException(@"flame");
			var copy = Copy();
			copy.mFlame = flame;
			return copy;
		}

		public int Index
		{
			get { return mFlame.Iterators.IndexOf(this); }
		}
		public int GroupIndex
		{
			get { return mGroupIndex; }
			set { mGroupIndex = value; }
		}
		public int GroupItemIndex
		{
			get { return mFlame.Iterators.Where(x => Equals(x.GroupIndex, GroupIndex)).ToList().IndexOf(this); }
		}
		public bool IsSingleInGroup
		{
			get { return mFlame.Iterators.Count(x => Equals(x.GroupIndex, GroupIndex)) == 1; }
		}

		[CanBeNull]
		public string Name
		{
			get { return mName; }
			set
			{
				if (string.IsNullOrEmpty(value))
					mName = null;
				else if (string.IsNullOrEmpty(value.Trim()))
					mName = null;
				else mName = value;
			}
		}

		public float Weight
		{
			get { return mWeight; }
			set
			{
				if (value < float.Epsilon)
				{
					throw new ArgumentOutOfRangeException(@"value");
				}

				mWeight = value;
			}
		}
		public float Color
		{
			get { return mColor; }
			set
			{
				if (value < 0 || value > 1)
				{
					throw new ArgumentOutOfRangeException(@"value");
				}

				mColor = value;
			}
		}
		public float ColorSpeed
		{
			get { return mColorSpeed; }
			set
			{
				if (value < -1 || value > 1)
				{
					throw new ArgumentOutOfRangeException(@"value");
				}

				mColorSpeed = value;
			}
		}
		public float Opacity
		{
			get { return mOpacity; }
			set
			{
				if (value < 0 || value > 1)
				{
					throw new ArgumentOutOfRangeException(@"value");
				}

				mOpacity = value;
			}
		}
		public float DirectColor
		{
			get { return mDirectColor; }
			set
			{
				if (value < 0 || value > 1)
				{
					throw new ArgumentOutOfRangeException(@"value");
				}

				mDirectColor = value;
			}
		}

		public Matrix3x2 PreAffine
		{
			get { return mPreAffine; }
			set
			{
				if (value == null) throw new ArgumentNullException(@"value");
				mPreAffine = value;
			}
		}

		public Matrix3x2 PostAffine
		{
			get { return mPostAffine; }
			set
			{
				if (value == null) throw new ArgumentNullException(@"value");
				mPostAffine = value;
			}
		}

		[NotNull]
		public VariationCollection Variations
		{
			get { return mVariations; }
		}

		public void ReadXml([NotNull] XElement element)
		{
			if (element == null) throw new ArgumentNullException(@"element");

			var elementName = element.Name.ToString().ToLower();
			var elementNames = new[] { @"xform", @"finalxform" };

			string[] knownNames = { @"name", @"weight", @"color", @"symmetry", @"opacity", @"var_color", @"coefs", @"post", @"chaos", @"color_speed", @"linear3D" };

			int groupIndex;

			switch (elementName)
			{
				case @"xform":
					groupIndex = 0;
					break;
				case @"finalxform":
					groupIndex = 1;
					break;
				default:
					var expectedNameString = elementNames.Length == 1
						? elementNames[0]
						: string.Format(Common.OrConcatenation,
							string.Join(@", ", elementNames.Select(x => @"""" + x + @"""").Take(elementNames.Length - 2).ToArray()),
							@"""" + elementNames.Last() + @"""");
					throw new ApophysisException(string.Format(Messages.UnexpectedXmlTagError, expectedNameString, elementName));
			}

			GroupIndex = groupIndex;

			var nameAttribute = element.Attribute(XName.Get(@"name"));
			Name = nameAttribute == null ? null : nameAttribute.Value;

			var weightAttribute = element.Attribute(XName.Get(@"weight"));
			if (weightAttribute != null && groupIndex == 0)
			{
				var weight = weightAttribute.ParseFloat(0.5f);
				if (weight < float.Epsilon)
				{
					throw new ApophysisException(Messages.IteratorWeightRangeError);
				}

				Weight = weight;
			}

			var colorAttribute = element.Attribute(XName.Get(@"color"));
			if (colorAttribute != null && groupIndex == 0)
			{
				var color = colorAttribute.ParseFloat();
				if (color < 0 || color > 1)
				{
					throw new ApophysisException(Messages.IteratorColorRangeError);
				}

				Color = color;
			}

			var colorSpeedAttribute = element.Attribute(XName.Get(@"symmetry"));
			if (colorSpeedAttribute != null && groupIndex == 0)
			{
				var colorSpeed = colorSpeedAttribute.ParseFloat();
				if (colorSpeed < -1 || colorSpeed > 1)
				{
					throw new ApophysisException(Messages.IteratorColorSpeedRangeError);
				}

				ColorSpeed = colorSpeed;
			}
			else if (colorSpeedAttribute != null && groupIndex > 0)
			{
				ColorSpeed = 1;
			}

			var opacityAttribute = element.Attribute(XName.Get(@"opacity"));
			if (opacityAttribute != null && groupIndex == 0)
			{
				var opacity = opacityAttribute.ParseFloat();
				if (opacity < 0 || opacity > 1)
				{
					throw new ApophysisException(Messages.IteratorOpacityRangeError);
				}

				Opacity = opacity;
			}

			var directColorAttribute = element.Attribute(XName.Get(@"var_color"));
			if (directColorAttribute != null)
			{
				var directColor = directColorAttribute.ParseFloat();
				if (directColor < 0 || directColor > 1)
				{
					throw new ApophysisException(Messages.IteratorDirectColorRangeError);
				}

				DirectColor = directColor;
			}

			var coefsAttribute = element.Attribute(XName.Get(@"coefs"));
			if (coefsAttribute != null)
			{
				var vector = coefsAttribute.ParseCoefficients();
				PreAffine = new Matrix3x2(vector[0], -vector[1], -vector[2], vector[3], vector[4], vector[5]);
			}

			var postAttribute = element.Attribute(XName.Get(@"post"));
			if (postAttribute != null)
			{
				var vector = postAttribute.ParseCoefficients();
				PreAffine = new Matrix3x2(vector[0], -vector[1], -vector[2], vector[3], vector[4], vector[5]);
			}

			Variations.ClearWeights();

			var potentialVariations = element.Attributes().Where(x => !knownNames.Contains(x.Name.ToString().ToLower()));
			foreach (var attribute in potentialVariations)
			{
				var attributeName = attribute.Name.ToString();
				var attributeValue = attribute.ParseFloat();

				if (VariationRegistry.IsVariation(attributeName))
				{
					mVariations.SetWeight(attributeName, attributeValue);
				}
				else if (VariationRegistry.IsVariable(attributeName))
				{
					mVariations.SetVariable(attributeName, attributeValue);
				}
				else
				{
					var message = string.Format(Messages.IteratorUnknownAttributeError, attributeName);

					Trace.TraceWarning(message);
					MessageCenter.SendUnknownAttribute(attribute.Name);
				}
			}
		}
		public bool IsEqual([NotNull] Iterator iterator)
		{
			if (iterator == null) throw new ArgumentNullException(@"iterator");

			if (!Equals(mName, iterator.mName))
				return false;

			if (!Equals(mWeight, iterator.mWeight))
				return false;

			if (!Equals(mColor, iterator.mColor))
				return false;

			if (!Equals(mColorSpeed, iterator.mColorSpeed))
				return false;

			if (!Equals(mOpacity, iterator.mOpacity))
				return false;

			if (!Equals(mDirectColor, iterator.mDirectColor))
				return false;

			if (!Equals(mGroupIndex, iterator.mGroupIndex))
				return false;

			if (mPreAffine != iterator.mPreAffine)
				return false;

			if (mPostAffine != iterator.mPostAffine)
				return false;

			if (!mVariations.IsEqual(iterator.mVariations))
				return false;

			return true;
		}
		public void WriteXml([NotNull] out XElement element)
		{
			element = new XElement(XName.Get(GroupIndex == 0 ? @"xform" : @"finalxform"));

			if (GroupIndex == 0)
				element.Add(new XAttribute(XName.Get(@"weight"), Weight.Serialize()));

			element.Add(new XAttribute(XName.Get(@"color"), Color.Serialize()));

			if (System.Math.Abs(ColorSpeed) > float.Epsilon)
			{
				element.Add(new XAttribute(XName.Get(@"symmetry"), ColorSpeed.Serialize()));
				element.Add(new XAttribute(XName.Get(@"color_speed"), ColorSpeed.Serialize()));
			}

			foreach (var variation in Variations.Where(x => System.Math.Abs(x.Weight) > float.Epsilon))
			{
				element.Add(new XAttribute(XName.Get(variation.Name), variation.Weight));

				foreach (var variable in variation.EnumerateVariables())
				{
					element.Add(new XAttribute(XName.Get(variable), variation.GetVariable(variable)));
				}
			}

			var affine = new [] { PreAffine.M11, -PreAffine.M12, -PreAffine.M21, PreAffine.M22, PreAffine.M31, -PreAffine.M32 };
			var postAffine = new[] { PostAffine.M11, -PostAffine.M12, -PostAffine.M21, PostAffine.M22, PostAffine.M31, -PostAffine.M32 };

			element.Add(new XAttribute(XName.Get(@"coefs"), affine.Serialize()));

			if (!PostAffine.IsIdentity)
				element.Add(new XAttribute(XName.Get(@"post"), postAffine.Serialize()));

			if (GroupIndex == 0)
				element.Add(new XAttribute(XName.Get(@"opacity"), Opacity.Serialize()));

			if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty((Name??string.Empty).Trim()))
				element.Add(new XAttribute(XName.Get(@"name"), Name ?? string.Empty));

			if (System.Math.Abs(DirectColor - 1) > float.Epsilon)
				element.Add(new XAttribute(XName.Get(@"var_color"), DirectColor.Serialize()));
		}
	}
}