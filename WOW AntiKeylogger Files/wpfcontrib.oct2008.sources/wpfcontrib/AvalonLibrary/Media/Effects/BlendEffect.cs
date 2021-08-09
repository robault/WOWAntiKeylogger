﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Avalon.Internal.Utility;

namespace Avalon.Windows.Media.Effects
{
    /// <summary>
    /// An effect that blend two textures.
    /// </summary>
    public class BlendEffect : ShaderEffect
    {
        #region Fields

        private static PixelShader _pixelShader = new PixelShader();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="BlendEffect"/> class.
        /// </summary>
        static BlendEffect()
        {
            _pixelShader.UriSource = UriHelper.MakePackUri("Media/Effects/Blend.ps");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendEffect"/> class.
        /// </summary>
        public BlendEffect()
        {
            PixelShader = _pixelShader;

            UpdateShaderValue(Input1Property);
            UpdateShaderValue(Input2Property);
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the first input.
        /// </summary>
        /// <value>The first input.</value>
        public Brush Input1
        {
            get { return (Brush)GetValue(Input1Property); }
            set { SetValue(Input1Property, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Input1"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Input1Property =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input1", typeof(BlendEffect), 0);

        /// <summary>
        /// Gets or sets the second input.
        /// </summary>
        /// <value>The second input.</value>
        public Brush Input2
        {
            get { return (Brush)GetValue(Input2Property); }
            set { SetValue(Input2Property, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Input2"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Input2Property =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input2", typeof(BlendEffect), 1);

        #endregion
    }
}
