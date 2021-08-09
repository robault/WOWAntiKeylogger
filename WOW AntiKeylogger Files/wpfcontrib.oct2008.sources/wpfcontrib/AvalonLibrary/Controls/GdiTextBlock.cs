using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Avalon.Windows.Controls
{
    /// <summary>
    /// Provides a control for displaying small amounts of aliased flow text content.
    /// </summary>
    public class GdiTextBlock : FrameworkElement, IDisposable
    {
        #region Fields

        private Bitmap _bitmapPresenter;

        private System.Drawing.Brush _drawingBrush;
        private System.Drawing.Font _drawingFont;
        private System.Drawing.StringFormat _drawingStringFormat;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the text contents of an <see cref="GdiTextBlock"/>.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            TextBlock.TextProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true });

        /// <summary>
        /// Gets or sets a value that indicates the quality of the rendered text.
        /// </summary>
        public GdiTextQuality TextQuality
        {
            get { return (GdiTextQuality)GetValue(TextQualityProperty); }
            set { SetValue(TextQualityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextQuality"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextQualityProperty =
            DependencyProperty.Register("TextQuality", typeof(GdiTextQuality), typeof(GdiTextBlock), new FrameworkPropertyMetadata { DefaultValue = GdiTextQuality.SingleBitPerPixelGridFit, AffectsMeasure = true, CoerceValueCallback = CoerceTextQuality });

        private static object CoerceTextQuality(DependencyObject o, object baseValue)
        {
            if (!Enum.IsDefined(typeof(GdiTextQuality), baseValue))
            {
                baseValue = TextQualityProperty.DefaultMetadata.DefaultValue;
            }
            return baseValue;
        }

        /// <summary>
        /// Gets or sets a value that indicates the horizontal alignment of text content.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty =
            Block.TextAlignmentProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true, CoerceValueCallback = CoerceTextAlignment });

        private static object CoerceTextAlignment(DependencyObject o, object baseValue)
        {
            // justify is not supported
            if (baseValue.Equals(TextAlignment.Justify))
            {
                baseValue = TextAlignmentProperty.DefaultMetadata.DefaultValue;
            }
            return baseValue;
        }

        /// <summary>
        /// Gets or sets how the <see cref="GdiTextBlock"/> should wrap text.
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextWrapping"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty =
            TextBlock.TextWrappingProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true, CoerceValueCallback = CoerceTextWrapping });

        private static object CoerceTextWrapping(DependencyObject o, object baseValue)
        {
            if (baseValue.Equals(TextWrapping.WrapWithOverflow))
            {
                baseValue = TextWrapping.Wrap;
            }
            return baseValue;
        }

        /// <summary>
        /// Gets or sets the text trimming behavior to employ when content overflows the content area.
        /// </summary>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextTrimming"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty =
            TextBlock.TextTrimmingProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true });

        /// <summary>
        /// Gets or sets the preferred top-level font family for the <see cref="GdiTextBlock"/>.
        /// </summary>
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FontFamily"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true });

        /// <summary>
        /// Gets or sets the top-level font style for the <see cref="GdiTextBlock"/>.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FontStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true, CoerceValueCallback = CoerceFontStyle });

        private static object CoerceFontStyle(DependencyObject o, object baseValue)
        {
            // oblique is not supported
            if (baseValue.Equals(FontStyles.Oblique))
            {
                baseValue = TextAlignmentProperty.DefaultMetadata.DefaultValue;
            }
            return baseValue;
        }

        /// <summary>
        /// Gets or sets the top-level font weight for the <see cref="GdiTextBlock"/>.
        /// </summary>
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true, CoerceValueCallback = CoerceFontWeight });

        private static object CoerceFontWeight(DependencyObject o, object baseValue)
        {
            // only bold, regular and normal are supported
            if (!baseValue.Equals(FontWeights.Bold) && !baseValue.Equals(FontWeights.Regular) && !baseValue.Equals(FontWeights.Normal))
            {
                baseValue = TextAlignmentProperty.DefaultMetadata.DefaultValue;
            }
            return baseValue;
        }

        /// <summary>
        /// Gets or sets the top-level font size for the <see cref="GdiTextBlock"/>.
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true });

        /// <summary>
        /// Gets or sets the <see cref="SolidColorBrush"/> to apply to the text contents of the <see cref="GdiTextBlock"/>.
        /// </summary>
        public SolidColorBrush Foreground
        {
            get { return (SolidColorBrush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(typeof(GdiTextBlock), new FrameworkPropertyMetadata { AffectsMeasure = true, CoerceValueCallback = CoerceForeground });

        private static object CoerceForeground(DependencyObject o, object baseValue)
        {
            // allow only a solid color brush
            if (!(baseValue is SolidColorBrush))
            {
                baseValue = ForegroundProperty.DefaultMetadata.DefaultValue;
            }
            return baseValue;
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="GdiTextBlock"/> is reclaimed by garbage collection.
        /// </summary>
        ~GdiTextBlock()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and (optinally) managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                DisposeDrawingObjects();
            }
        }

        #endregion

        #region Add the Bitmap as a Visual Child

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            _bitmapPresenter = new Bitmap();
            AddVisualChild(_bitmapPresenter);

            base.OnInitialized(e);
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of visual child elements for this element.
        /// </returns>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            return _bitmapPresenter;
        }

        #endregion

        #region Measure and Arrange

        /// <summary>
        /// Measures the size in layout required for the bitmap control and determines a size.
        /// <remarks>
        /// Also creates the GDI bitmap that contains the text.
        /// </remarks>
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>
        /// The size that this element determines it needs during layout, based on its calculations of child element sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            System.Drawing.Size size = new System.Drawing.Size();

            // measure the text size according to the available size (its size doesn't matter; we just need a DC to work with)
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(1, 1))
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            {
                float dpiX = g.DpiX / 96f;
                float dpiY = g.DpiY / 96f;

                CreateDrawingObjects(dpiY);

                g.TextRenderingHint = (System.Drawing.Text.TextRenderingHint)TextQuality;

                System.Drawing.SizeF boundary = new System.Drawing.SizeF((float)availableSize.Width * dpiX, (float)availableSize.Height * dpiY);
                boundary = g.MeasureString(Text, _drawingFont, boundary, _drawingStringFormat);

                size = new System.Drawing.Size((int)Math.Ceiling(boundary.Width), (int)Math.Ceiling(boundary.Height));
            }

            // render the bitmap with the text
            using (System.Drawing.Bitmap textBitmap = new System.Drawing.Bitmap(size.Width, size.Height))
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(textBitmap))
            {
                g.TextRenderingHint = (System.Drawing.Text.TextRenderingHint)TextQuality;

                Color color = ((SolidColorBrush)Foreground).Color;
                g.DrawString(Text, _drawingFont, _drawingBrush,
                    new System.Drawing.RectangleF(0, 0, size.Width, size.Height), _drawingStringFormat);

                _bitmapPresenter.Source = CreateBitmapSource(textBitmap);
                _bitmapPresenter.InvalidateMeasure();
                _bitmapPresenter.InvalidateVisual();
            }

            _bitmapPresenter.Measure(availableSize);
            return _bitmapPresenter.DesiredSize;
        }

        /// <summary>
        /// Positions the bitmap control and determines the final size.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _bitmapPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        #endregion

        #region Convert Bitmap

        /// <summary>
        /// Creates a <see cref="System.Windows.Media.Imaging.BitmapSource"/> from a <see cref="System.Drawing.Bitmap"/>.
        /// <remarks>
        /// </remarks>
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static System.Windows.Media.Imaging.BitmapSource CreateBitmapSource(System.Drawing.Bitmap bitmap)
        {
            System.Windows.Media.Imaging.BitmapSource source = null;

            // the fast way requires the UnmanagedCode permission
            try
            {
                new System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode).Demand();

                source = FastCreateBitmapSource(bitmap);
            }
            catch (System.Security.SecurityException)
            {
                source = SlowCreateBitmapSource(bitmap);
            }

            return source;
        }

        /// <summary>
        /// Creates a <see cref="System.Windows.Media.Imaging.BitmapSource"/> from a <see cref="System.Drawing.Bitmap"/>.
        /// <remarks>
        /// Uses the slow (but safe) <see cref="System.Drawing.Bitmap.GetPixel"/>.
        /// </remarks>
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        private static System.Windows.Media.Imaging.BitmapSource SlowCreateBitmapSource(System.Drawing.Bitmap bitmap)
        {
            int stride = bitmap.Width * 4;
            int bufferSize = stride * bitmap.Height;
            byte[] buffer = new byte[bitmap.Width * 4 * bitmap.Height];
            for (int x = 0; x < bitmap.Width; ++x)
            {
                for (int y = 0; y < bitmap.Height; ++y)
                {
                    System.Drawing.Color c = bitmap.GetPixel(x, y);
                    int loc = (x * 4) + (stride * y);
                    buffer[loc] = c.B;
                    buffer[loc + 1] = c.G;
                    buffer[loc + 2] = c.R;
                    buffer[loc + 3] = c.A;
                }
            }

            return System.Windows.Media.Imaging.BitmapSource.Create(bitmap.Width, bitmap.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Pbgra32, null,
                buffer, stride);
        }

        /// <summary>
        /// Creates a <see cref="System.Windows.Media.Imaging.BitmapSource"/> from a <see cref="System.Drawing.Bitmap"/>.
        /// <remarks>
        /// This method seems to be much faster and consumes significantly less memory than <see cref="System.Windows.Interop.InteropBitmap"/>.
        /// </remarks>
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        private static System.Windows.Media.Imaging.BitmapSource FastCreateBitmapSource(System.Drawing.Bitmap bitmap)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            System.Windows.Media.Imaging.BitmapSource source = System.Windows.Media.Imaging.BitmapSource.Create(bitmap.Width, bitmap.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Pbgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmap.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return source;
        }
        
        #endregion

        #region GDI Drawing Objects

        /// <summary>
        /// Creates the System.Drawing objects required to create the text bitmap.
        /// </summary>
        private void CreateDrawingObjects(float dpi)
        {
            DisposeDrawingObjects();

            Color color = ((SolidColorBrush)Foreground).Color;
            _drawingBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb((int)color.A, (int)color.R, (int)color.G, (int)color.B));

            System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
            if (FontWeight == FontWeights.Bold)
            {
                style |= System.Drawing.FontStyle.Bold;
            }
            if (FontStyle == FontStyles.Italic)
            {
                style |= System.Drawing.FontStyle.Italic;
            }

            // enlarge the font according to the system dpi
            _drawingFont = new System.Drawing.Font(FontFamily.Source, (float)(FontSize * dpi), style, System.Drawing.GraphicsUnit.Pixel);

            _drawingStringFormat = new System.Drawing.StringFormat();

            switch (TextTrimming)
            {
                case TextTrimming.None:
                    _drawingStringFormat.Trimming = System.Drawing.StringTrimming.None;
                    break;
                case TextTrimming.CharacterEllipsis:
                    _drawingStringFormat.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                    break;
                case TextTrimming.WordEllipsis:
                    _drawingStringFormat.Trimming = System.Drawing.StringTrimming.EllipsisWord;
                    break;
            }

            _drawingStringFormat.FormatFlags = System.Drawing.StringFormatFlags.LineLimit;

            if (TextWrapping == TextWrapping.NoWrap)
            {
                _drawingStringFormat.FormatFlags |= System.Drawing.StringFormatFlags.NoWrap;
            }

            if (FlowDirection == FlowDirection.RightToLeft)
            {
                _drawingStringFormat.FormatFlags |= System.Drawing.StringFormatFlags.DirectionRightToLeft;
            }

            switch (TextAlignment)
            {
                case TextAlignment.Left:
                    _drawingStringFormat.Alignment = System.Drawing.StringAlignment.Near;
                    break;
                case TextAlignment.Center:
                    _drawingStringFormat.Alignment = System.Drawing.StringAlignment.Center;
                    break;
                case TextAlignment.Right:
                    _drawingStringFormat.Alignment = System.Drawing.StringAlignment.Far;
                    break;
            }
        }

        /// <summary>
        /// Disposes the System.Drawing objects.
        /// </summary>
        private void DisposeDrawingObjects()
        {
            if (_drawingBrush != null)
            {
                _drawingBrush.Dispose();
            }

            if (_drawingFont != null)
            {
                _drawingFont.Dispose();
            }

            if (_drawingStringFormat != null)
            {
                _drawingStringFormat.Dispose();
            }
        }

        #endregion
    }
}
