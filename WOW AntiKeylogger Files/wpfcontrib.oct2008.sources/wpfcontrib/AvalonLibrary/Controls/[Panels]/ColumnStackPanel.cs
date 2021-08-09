using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Avalon.Windows.Controls
{
    /// <summary>
    /// Stacks elements in columns.
    /// <remarks>
    /// Not sure how useful this panel is. <see cref="AutoGrid"/> can do similar things.
    /// Maybe if the RowOrdinal would be automatically calculated, it would be easier to use.
    /// </remarks>
    /// </summary>
    public class ColumnStackPanel : Panel
    {
        #region Fields

        private SortedList<Location, double> _heights;
        private int _maxOrdinal;

        #endregion

        #region Dependency Properties

        #region ColumnCount

        /// <summary>
        /// Gets or sets the column count.
        /// </summary>
        /// <value>The column count.</value>
        public int ColumnCount
        {
            get { return (int)GetValue(ColumnCountProperty); }
            set { SetValue(ColumnCountProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ColumnCount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register("ColumnCount", typeof(int), typeof(ColumnStackPanel), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, null, CoerceColumnCount));

        private static object CoerceColumnCount(DependencyObject o, object baseValue)
        {
            int value = (int)baseValue;

            if (value < 1)
            {
                value = 1;
            }

            return value;
        }

        #endregion

        #endregion

        #region Attached Properties
                
        #region StartColumn

        /// <summary>
        /// Gets the start column.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static int GetStartColumn(DependencyObject obj)
        {
            return (int)obj.GetValue(StartColumnProperty);
        }

        /// <summary>
        /// Sets the start column.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetStartColumn(DependencyObject obj, int value)
        {
            obj.SetValue(StartColumnProperty, value);
        }

        /// <summary>
        /// Identifies the <c>StartColumn</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty StartColumnProperty =
            DependencyProperty.RegisterAttached("StartColumn", typeof(int), typeof(ColumnStackPanel), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure, OnStartColumnChanged, CoerceStartColumn));

        private static void OnStartColumnChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            o.CoerceValue(EndColumnProperty);
        }

        private static object CoerceStartColumn(DependencyObject o, object baseValue)
        {
            ColumnStackPanel panel = VisualTreeHelper.GetParent(o) as ColumnStackPanel;
            int value = (int)baseValue;

            if (panel != null)
            {
                int lastColumn = panel.ColumnCount - 1;

                if (value < 0)
                {
                    value = 0;
                }
                else if (value > lastColumn)
                {
                    value = lastColumn;
                }
            }

            return value;
        }

        #endregion

        #region EndColumn

        /// <summary>
        /// Gets the end column.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static int GetEndColumn(DependencyObject obj)
        {
            return (int)obj.GetValue(EndColumnProperty);
        }

        /// <summary>
        /// Sets the end column.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">The value.</param>
        public static void SetEndColumn(DependencyObject obj, int value)
        {
            obj.SetValue(EndColumnProperty, value);
        }

        /// <summary>
        /// Identifies the <c>EndColumn</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty EndColumnProperty =
            DependencyProperty.RegisterAttached("EndColumn", typeof(int), typeof(ColumnStackPanel), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure, null, CoerceEndColumn));

        private static object CoerceEndColumn(DependencyObject o, object baseValue)
        {
            ColumnStackPanel panel = VisualTreeHelper.GetParent(o) as ColumnStackPanel;
            int value = (int)baseValue;

            if (panel != null)
            {
                int lastColumn = panel.ColumnCount - 1;
                int startColumn = GetStartColumn(o);

                if (value < startColumn)
                {
                    value = startColumn;
                }
                else if (value > lastColumn)
                {
                    value = lastColumn;
                }
            }

            return value;
        }

        #endregion

        #region RowOrdinal

        /// <summary>
        /// Gets the row ordinal.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static int GetRowOrdinal(DependencyObject obj)
        {
            return (int)obj.GetValue(RowOrdinalProperty);
        }

        /// <summary>
        /// Sets the row ordinal.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetRowOrdinal(DependencyObject obj, int value)
        {
            obj.SetValue(RowOrdinalProperty, value);
        }

        /// <summary>
        /// Identifies the <c>RowOrdinal</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty RowOrdinalProperty =
            DependencyProperty.RegisterAttached("RowOrdinal", typeof(int), typeof(ColumnStackPanel), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        #endregion

        #endregion

        #region Measure/Arrange Methods

        /// <summary>
        /// Measures the size in layout required for child elements.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            UIElementCollection children = base.InternalChildren;
            Size result = new Size();
            Size childConstraint = availableSize;
            childConstraint.Height = double.PositiveInfinity;


            int childCount = children.Count;
            for (int currentChild = 0; currentChild < childCount; ++currentChild)
            {
                UIElement child = children[currentChild];
                if (child != null)
                {
                    double size;
                    int span = GetEndColumn(child) - GetStartColumn(child) + 1;
                    childConstraint.Width = (availableSize.Width / ColumnCount) * span;
                    child.Measure(childConstraint);
                    Size desiredChildSize = child.DesiredSize;
                    result.Width = Math.Max(result.Width, desiredChildSize.Width);
                    size = desiredChildSize.Height;
                }
            }

            CalculateHeights();

            result.Height = GetMaximumHeight(_maxOrdinal, 1, ColumnCount);

            return result;
        }

        /// <summary>
        /// Positions child elements and determines a size.
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElementCollection children = base.Children;
            Rect rect = new Rect(finalSize);
            double size = 0;
            int childCount = children.Count;

            CalculateHeights();

            for (int currentChild = 0; currentChild < childCount; ++currentChild)
            {
                UIElement child = children[currentChild];
                if (child != null)
                {
                    int ordinal = GetRowOrdinal(child);
                    int startCol = GetStartColumn(child);
                    int endCol = GetEndColumn(child);
                    int span = endCol - startCol + 1;

                    rect = new Rect();
                    rect.X = (finalSize.Width / ColumnCount) * startCol;
                    rect.Y = GetMaximumHeight(ordinal - 1, startCol, endCol);
                    size = child.DesiredSize.Height;
                    rect.Height = size;
                    rect.Width = (finalSize.Width / ColumnCount) * span;
                    child.Arrange(rect);
                }
            }
            return finalSize;
        }

        private void CalculateHeights()
        {
            _heights = new SortedList<Location, double>();

            SortedList<Location, double> tops = new SortedList<Location, double>();
            UIElementCollection children = InternalChildren;
            int childCount = children.Count;

            for (int currentChild = 0; currentChild < childCount; ++currentChild)
            {
                UIElement child = children[currentChild];
                if (child != null)
                {
                    int ordinal = GetRowOrdinal(child);
                    int startCol = GetStartColumn(child);
                    int endCol = GetEndColumn(child);

                    for (int i = startCol; i <= endCol; ++i)
                    {
                        Location location = new Location(i, ordinal);
                        tops[location] = child.DesiredSize.Height;
                    }

                    _maxOrdinal = Math.Max(ordinal, _maxOrdinal);
                }
            }

            foreach (KeyValuePair<Location, double> kv in tops)
            {
                Location p = kv.Key;
                _heights[p] = kv.Value;

                for (int i = p.Ordinal - 1; i >= 0; --i)
                {
                    double value;
                    tops.TryGetValue(new Location(p.Column, i), out value);
                    _heights[p] += value;
                }
            }
        }

        private double GetMaximumHeight(int ordinal, int startColumn, int endColumn)
        {
            double max = 0;

            if (ordinal >= 0)
            {
                for (int i = startColumn; i <= endColumn; ++i)
                {
                    Location val = new Location(i, ordinal);
                    double currentHeight;
                    _heights.TryGetValue(val, out currentHeight);
                    max = Math.Max(max, currentHeight);
                }
            }
            return max;
        }

        #region Location Structure

        private struct Location : IComparable<Location>
        {
            public Location(int column, int ordinal)
            {
                _column = column;
                _ordinal = ordinal;
            }

            private readonly int _column;
            public int Column
            {
                get { return _column; }
            }

            private readonly int _ordinal;
            public int Ordinal
            {
                get { return _ordinal; }
            }

            #region IComparable<Location> Members

            public int CompareTo(Location other)
            {
                int result = _column.CompareTo(other._column);
                if (result == 0)
                {
                    result = _ordinal.CompareTo(other._ordinal);
                }

                return result;
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
