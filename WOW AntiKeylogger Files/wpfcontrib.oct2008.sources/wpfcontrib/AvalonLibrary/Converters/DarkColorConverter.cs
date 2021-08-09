using System;
using System.Windows.Data;
using System.Windows.Media;
using Avalon.Windows.Utility;

namespace Avalon.Windows.Converters
{    
    /// <summary>
    /// Converts a <see cref="Color"/> to a <see cref="Color"/> with a lower brightness.
    /// </summary>
    public class DarkColorConverter : ValueConverter
    {
        private float _factor = 1.0F;
        /// <summary>
        /// Gets or sets the factor.
        /// </summary>
        /// <value>The factor.</value>
        public float Factor
        {
            get { return _factor; }
            set { _factor = value; }
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the GTMT#binding source.</param>
        /// <param name="targetType">The type of the GTMT#binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color? color = value as Color?;
            if (color == null)
            {
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(value as string);
                }
                catch (FormatException) { }
            }
            if (color != null)
            {
                return ColorHelpers.Darken(color.Value, _factor);
            }
            
            return Binding.DoNothing;
        }

        /// <summary>
        /// Converts a value.
        /// <remarks>Not implemented.</remarks>
        /// </summary>
        /// <param name="value">The value that is produced by the GTMT#binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
