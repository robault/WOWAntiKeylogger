using System;
using System.Security;
using System.Windows.Markup;
using System.Windows.Media.Effects;

namespace Avalon.Windows.Media.Effects
{
    /// <summary>
    /// Attempts to create a <see cref="BlurBitmapEffect"/>.
    /// <remarks>
    /// Catches the security exception that's thrown in partial trust.
    /// </remarks>
    /// </summary>
    [MarkupExtensionReturnType(typeof(BlurBitmapEffect))]
    public class BlurBitmapEffectExtension : MarkupExtension
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlurBitmapEffectExtension"/> class.
        /// </summary>
        public BlurBitmapEffectExtension()
        {
            KernelType = KernelType.Gaussian;
            Radius = 5;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of blur kernel to use.
        /// </summary>
        /// <value>
        /// The type of blur kernel. The default value is <see cref="System.Windows.Media.Effects.KernelType.Gaussian"/>.
        /// </value>
        public KernelType KernelType { get; set; }
        
        /// <summary>
        /// Gets or sets the radius used in the blur kernel. A larger radius implies more blurring. 
        /// </summary>
        /// <value>
        /// The radius used in the blur kernel, in DIU (1/96 of an inch). The default value is 5.
        /// </value>
        public double Radius { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the <see cref="BlurBitmapEffect"/>.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                return new BlurBitmapEffect
                {
                    KernelType = KernelType,
                    Radius = Radius,
                };
            }
            catch (SecurityException) { }
            
            return null;
        }

        #endregion
    }
}
