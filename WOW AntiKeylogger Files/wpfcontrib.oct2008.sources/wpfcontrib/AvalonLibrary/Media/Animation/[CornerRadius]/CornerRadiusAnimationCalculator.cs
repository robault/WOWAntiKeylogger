using System;
using System.Windows;

namespace Avalon.Windows.Media.Animation
{
    internal class CornerRadiusAnimationCalculator : IAnimationCalculator<CornerRadius>
    {
        DoubleAnimationCalculator calc = new DoubleAnimationCalculator();

        public CornerRadius Add(CornerRadius value1, CornerRadius value2)
        {
            return new CornerRadius(calc.Add(value1.TopLeft, value2.TopLeft), calc.Add(value1.TopRight, value2.TopRight), calc.Add(value1.BottomRight, value2.BottomRight), calc.Add(value1.BottomLeft, value2.BottomLeft));
        }

        public CornerRadius Subtract(CornerRadius value1, CornerRadius value2)
        {
            return new CornerRadius(value1.TopLeft - value2.TopLeft, value1.TopRight - value2.TopRight, value1.BottomRight - value2.BottomRight, value1.BottomLeft - value2.BottomLeft);
        }

        public CornerRadius Scale(CornerRadius value, double factor)
        {
            return new CornerRadius(calc.Scale(value.TopLeft, factor), calc.Scale(value.TopRight, factor), calc.Scale(value.BottomRight, factor), calc.Scale(value.BottomLeft, factor));
        }

        public CornerRadius Interpolate(CornerRadius from, CornerRadius to, double progress)
        {
            return new CornerRadius(calc.Interpolate(from.TopLeft, to.TopLeft, progress), calc.Interpolate(from.TopRight, to.TopRight, progress), calc.Interpolate(from.BottomRight, to.BottomRight, progress), calc.Interpolate(from.BottomLeft, to.BottomLeft, progress));
        }

        public CornerRadius GetZeroValue(CornerRadius baseValue)
        {
            return new CornerRadius(calc.GetZeroValue(baseValue.TopLeft), calc.GetZeroValue(baseValue.TopRight), calc.GetZeroValue(baseValue.BottomRight), calc.GetZeroValue(baseValue.BottomLeft));
        }

        public double GetSegmentLength(CornerRadius from, CornerRadius to)
        {
            double result = ((Math.Pow(calc.GetSegmentLength(from.TopLeft, to.TopLeft), 2) +
                Math.Pow(calc.GetSegmentLength(from.TopRight, to.TopRight), 2)) +
                Math.Pow(calc.GetSegmentLength(from.BottomRight, to.BottomRight), 2)) +
                Math.Pow(calc.GetSegmentLength(from.BottomLeft, to.BottomLeft), 2);
            return Math.Sqrt(result);
        }

        public bool IsValidAnimationValue(CornerRadius value)
        {
            if ((!calc.IsValidAnimationValue(value.TopLeft) && !calc.IsValidAnimationValue(value.TopRight)) && (!calc.IsValidAnimationValue(value.BottomRight) && !calc.IsValidAnimationValue(value.BottomLeft)))
            {
                return false;
            }
            return true;
        }
    }
}
