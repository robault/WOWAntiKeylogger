using System;
using System.Security;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows;

namespace Avalon.Windows.Media.Effects
{
    /// <summary>
    /// Attempts to create a <see cref="DropShadowBitmapEffect"/>.
    /// <remarks>
    /// Catches the security exception that's thrown in partial trust.
    /// </remarks>
    /// </summary>
    [MarkupExtensionReturnType(typeof(DropShadowBitmapEffect))]
    public class DropShadowBitmapEffectExtension : MarkupExtension
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DropShadowBitmapEffectExtension"/> class.
        /// </summary>
        public DropShadowBitmapEffectExtension()
        {
            Color = Colors.Black;
            Direction = 315;
            Noise = 0;
            Opacity = 1;
            ShadowDepth = 5;
            Softness = 0.5;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color of the shadow.
        /// </summary>
        /// <value>
        /// The color of the shadow. The default value is FF000000 (black).
        /// </value>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the angle at which the shadow is cast.
        /// </summary>
        /// <value>
        /// The angle at which the shadow is cast. The valid range of values is from
        /// 0 through 360. The value 0 puts the direction immediately to the right of
        /// the object. Subsequent values move the direction around the object in a counter-clockwise
        /// direction. For example, a value of 90 indicates the shadow is cast directly
        /// upward from the object; a value of 180 is cast directly to the left of the
        /// object, and so on. The default value is 315.
        /// </value>
        public double Direction { get; set; }

        /// <summary>
        /// Gets or sets the graininess, or "noise level," of the shadow.
        /// </summary>
        /// <value>
        /// The noise level of the shadow. The valid range of values is from 0 through
        /// 1. A value of 0 indicates no noise and 1 indicates maximum noise. A value
        /// of 0.5 indicates 50 percent noise, a value of 0.75 indicates 75 percent noise,
        /// and so on. The default value is 0.
        /// </value>
        public double Noise { get; set; }

        /// <summary>
        /// Gets or sets the degree of opacity of the shadow.
        /// </summary>
        /// <value>
        /// The degree of opacity. The valid range of values is from 0 through 1. A value
        /// of 0 indicates that the shadow is completely transparent, and a value of
        /// 1 indicates that the shadow is completely opaque. A value of 0.5 indicates
        /// the shadow is 50 percent opaque, a value of 0.725 indicates the shadow is
        /// 72.5 percent opaque, and so on. Values less than 0 are treated as 0, while
        /// values greater than 1 are treated as 1. The default is 1.
        /// </value>
        public double Opacity { get; set; }

        /// <summary>
        /// Gets or sets the distance between the object and the shadow that it casts.
        /// </summary>
        /// <value>
        /// The distance between the plane of the object casting the shadow and the shadow
        /// plane measured in device-independent units (1/96th inch per unit). The valid
        /// range of values is from 0 through 300. The default is 5.
        /// </value>
        public double ShadowDepth { get; set; }

        /// <summary>
        /// Gets or sets the softness of the shadow
        /// </summary>
        /// <value>
        /// The shadow's softness. The valid range of values is from 0 through 1. A value
        /// of 0.0 indicates no softness (a sharply defined shadow) and 1.0 indicates
        /// maximum softness (a very diffused shadow). A value of 0.5 indicates 50 percent
        /// softness, a value of 0.75 indicates 75 percent softness, and so on. The default
        /// is 0.5.
        /// </value>
        public double Softness { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the <see cref="DropShadowBitmapEffect"/>.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                return new DropShadowBitmapEffect
                {
                    Color = Color,
                    Direction = Direction,
                    Noise = Noise,
                    Opacity = Opacity,
                    ShadowDepth = ShadowDepth,
                    Softness = Softness,
                };
            }
            catch (SecurityException) { }

            return null;
        }

        #endregion
    }
}
