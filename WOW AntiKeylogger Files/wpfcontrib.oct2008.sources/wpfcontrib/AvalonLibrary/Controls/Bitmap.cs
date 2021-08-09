using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Avalon.Windows.Utility;

namespace Avalon.Windows.Controls
{
    /// <summary>
    /// Provides a bitmap image control that prevents blurriness.
    /// <remarks>
    /// Source from http://blogs.msdn.com/dwayneneed/archive/2007/10/05/blurry-bitmaps.aspx.
    /// </remarks>
    /// </summary>
    public class Bitmap : UIElement
    {
        #region Fields

        private EventHandler _sourceDownloaded;
        private EventHandler<ExceptionEventArgs> _sourceFailed;
        private Point _pixelOffset;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class.
        /// </summary>
        public Bitmap()
        {
            _sourceDownloaded = new EventHandler(OnSourceDownloaded);
            _sourceFailed = new EventHandler<ExceptionEventArgs>(OnSourceFailed);

            LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(BitmapSource),
            typeof(Bitmap),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                new PropertyChangedCallback(Bitmap.OnSourceChanged)));

        /// <summary>
        /// Gets or sets the bitmap source.
        /// </summary>
        /// <value>The bitmap source.</value>
        public BitmapSource Source
        {
            get
            {
                return (BitmapSource)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when loading the bitmap has failed.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> BitmapFailed;

        #endregion

        #region Overrides

        /// <summary>
        /// Return our measure size to be the size needed to display the bitmap pixels.
        /// </summary>
        /// <param name="availableSize">The available size that the parent element can allocate for the child.</param>
        /// <returns>
        /// The desired size of this element in layout.
        /// </returns>
        protected override Size MeasureCore(Size availableSize)
        {
            Size measureSize = new Size();

            BitmapSource bitmapSource = Source;
            if (bitmapSource != null)
            {
                Matrix fromDevice = UIHelpers.DpiTransformFromDevice;

                Vector pixelSize = new Vector(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
                Vector measureSizeV = fromDevice.Transform(pixelSize);
                measureSize = new Size(measureSizeV.X, measureSizeV.Y);
            }

            return measureSize;
        }

        /// <summary>
        /// Renders the bitmap.
        /// </summary>
        /// <param name="drawingContext">The drawing context.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            _pixelOffset = GetPixelOffset();

            BitmapSource bitmapSource = Source;
            if (bitmapSource != null)
            {
                // Render the bitmap offset by the needed amount to align to pixels.
                drawingContext.DrawImage(bitmapSource, new Rect(_pixelOffset, DesiredSize));
            }
        }

        #endregion

        #region Event Handlers

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Bitmap bitmap = (Bitmap)d;

            BitmapSource oldValue = (BitmapSource)e.OldValue;
            BitmapSource newValue = (BitmapSource)e.NewValue;

            if (((oldValue != null) && (bitmap._sourceDownloaded != null)) && (!oldValue.IsFrozen && (oldValue is BitmapSource)))
            {
                ((BitmapSource)oldValue).DownloadCompleted -= bitmap._sourceDownloaded;
                ((BitmapSource)oldValue).DownloadFailed -= bitmap._sourceFailed;
                //((BitmapSource)newValue).DecodeFailed -= bitmap._sourceFailed;
            }
            if (((newValue != null) && (newValue is BitmapSource)) && !newValue.IsFrozen)
            {
                ((BitmapSource)newValue).DownloadCompleted += bitmap._sourceDownloaded;
                ((BitmapSource)newValue).DownloadFailed += bitmap._sourceFailed;
                //((BitmapSource)newValue).DecodeFailed += bitmap._sourceFailed;
            }
        }

        private void OnSourceDownloaded(object sender, EventArgs e)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }

        private void OnSourceFailed(object sender, ExceptionEventArgs e)
        {
            Source = null; // setting a local value seems scetchy...

            BitmapFailed(this, e);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            // This event just means that layout happened somewhere.  However, this is
            // what we need since layout anywhere could affect our pixel positioning.
            Point pixelOffset = GetPixelOffset();
            if (!AreClose(pixelOffset, _pixelOffset))
            {
                InvalidateVisual();
            }
        }

        #endregion

        #region Helper Methods

        private Point GetPixelOffset()
        {
            Point pixelOffset = new Point();


            Visual rootVisual = this.FindVisualRoot() as Visual;
            if (rootVisual != null && rootVisual != this)
            {
                // Transform (0,0) from this element up to pixels.
                pixelOffset = TransformToAncestor(rootVisual).Transform(pixelOffset);
                pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, false);
                pixelOffset = UIHelpers.DpiTransformToDevice.Transform(pixelOffset);

                // Round the origin to the nearest whole pixel.
                pixelOffset.X = Math.Round(pixelOffset.X);
                pixelOffset.Y = Math.Round(pixelOffset.Y);

                // Transform the whole-pixel back to this element.
                pixelOffset = UIHelpers.DpiTransformFromDevice.Transform(pixelOffset);
                pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, true);
                pixelOffset = rootVisual.TransformToDescendant(this).Transform(pixelOffset);
            }

            return pixelOffset;
        }

        /// <summary>
        /// Gets the matrix that will convert a point from "above" the
        /// coordinate space of a visual into the the coordinate space
        /// "below" the visual.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static Matrix GetVisualTransform(Visual v)
        {
            if (v != null)
            {
                Matrix m = Matrix.Identity;

                Transform transform = VisualTreeHelper.GetTransform(v);
                if (transform != null)
                {
                    Matrix cm = transform.Value;
                    m = Matrix.Multiply(m, cm);
                }

                Vector offset = VisualTreeHelper.GetOffset(v);
                m.Translate(offset.X, offset.Y);

                return m;
            }

            return Matrix.Identity;
        }

        private static Point TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError, out bool success)
        {
            success = true;
            if (v != null)
            {
                Matrix visualTransform = GetVisualTransform(v);
                if (inverse)
                {
                    if (!throwOnError && !visualTransform.HasInverse)
                    {
                        success = false;
                        return new Point(0, 0);
                    }
                    visualTransform.Invert();
                }
                point = visualTransform.Transform(point);
            }
            return point;
        }

        private static Point ApplyVisualTransform(Point point, Visual v, bool inverse)
        {
            bool success = true;
            return TryApplyVisualTransform(point, v, inverse, true, out success);
        }

        private static bool AreClose(Point point1, Point point2)
        {
            return AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);
        }

        private static bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            double delta = value1 - value2;
            return ((delta < 1.53E-06) && (delta > -1.53E-06));
        }

        #endregion
    }
}
