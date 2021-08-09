﻿using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Avalon.Windows.Utility
{
    /// <summary>
    /// Encapsulates methods and properties for handling animations.
    /// </summary>
    public static class AnimationHelpers
    {
        #region Methods

        /// <summary>
        /// Switches between the To and From properties of the each AnimationTimeline in the Storyboard.
        /// </summary>
        /// <param name="storyboard">The storyboard.</param>
        public static void ReverseStoryboard(this Storyboard storyboard)
        {
            foreach (AnimationTimeline anim in storyboard.Children)
            {
                DependencyProperty from = DependencyHelpers.GetDependencyProperty(anim, "From");
                DependencyProperty to = DependencyHelpers.GetDependencyProperty(anim, "To");

                object fromValue = anim.GetValue(from);
                anim.SetValue(from, anim.GetValue(to));
                anim.SetValue(to, fromValue);
            }
        }

        /// <summary>
        /// Returns a cloned Storyboard where the To and From properties of the AnimationTimeline have been switched.
        /// </summary>
        /// <param name="storyboard">The storyboard.</param>
        /// <returns></returns>
        public static Storyboard GetReversedStoryboard(this Storyboard storyboard)
        {
            Storyboard cloned = storyboard.Clone();

            ReverseStoryboard(cloned);

            return cloned;
        }

        /// <summary>
        /// Creates and adds an AnimationTimeline to a Storyboard.
        /// </summary>
        /// <typeparam name="TAnimation">The type of the animation.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="storyboard">The storyboard.</param>
        /// <param name="path">The path.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        public static TAnimation AddLinearAnimation<TAnimation, T>(this Storyboard storyboard, PropertyPath path, T? from, T? to, Duration duration)
            where TAnimation : AnimationTimeline, new()
            where T : struct
        {
            TAnimation timeline = new TAnimation();

            DependencyProperty fromProp = DependencyHelpers.GetDependencyProperty(timeline, "From");
            DependencyProperty toProp = DependencyHelpers.GetDependencyProperty(timeline, "To");

            timeline.Duration = duration;

            timeline.SetValue(fromProp, from);
            timeline.SetValue(toProp, to);

            Storyboard.SetTargetProperty(timeline, path);

            storyboard.Children.Add(timeline);

            return timeline;
        }

        /// <summary>
        /// Adds the animation to the storyboard.
        /// </summary>
        /// <param name="storyboard">The storyboard.</param>
        /// <param name="timeline">The timeline.</param>
        /// <param name="target">The target.</param>
        /// <param name="property">The property.</param>
        public static void AddAnimation(this Storyboard storyboard, Timeline timeline, DependencyObject target, DependencyProperty property)
        {
            Storyboard.SetTarget(timeline, target);
            Storyboard.SetTargetProperty(timeline, new PropertyPath(property));
            storyboard.Children.Add(timeline);
        }

        /// <summary>
        /// Adds the animation to the storyboard.
        /// </summary>
        /// <param name="storyboard">The storyboard.</param>
        /// <param name="timeline">The timeline.</param>
        /// <param name="targetName">Name of the target.</param>
        /// <param name="property">The property.</param>
        public static void AddAnimation(this Storyboard storyboard, Timeline timeline, string targetName, DependencyProperty property)
        {
            Storyboard.SetTargetName(timeline, targetName);
            Storyboard.SetTargetProperty(timeline, new PropertyPath(property));
            storyboard.Children.Add(timeline);
        }

        #endregion

        #region Attached Properties

        #region ActualWidth (private)

        private static double? GetActualWidth(FrameworkElement obj)
        {
            return (double?)obj.GetValue(ActualWidthProperty);
        }

        private static void SetActualWidth(FrameworkElement obj, double? value)
        {
            obj.SetValue(ActualWidthPropertyKey, value);
        }

        private static void ClearActualWidth(FrameworkElement obj)
        {
            obj.ClearValue(ActualWidthPropertyKey);
        }

        private static readonly DependencyPropertyKey ActualWidthPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ActualWidth", typeof(double?), typeof(AnimationHelpers), new FrameworkPropertyMetadata());

        private static readonly DependencyProperty ActualWidthProperty = ActualWidthPropertyKey.DependencyProperty;

        #endregion

        #region WidthPercentage

        /// <summary>
        /// Gets the width percentage.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static int GetWidthPercentage(FrameworkElement obj)
        {
            return (int)obj.GetValue(WidthPercentageProperty);
        }

        /// <summary>
        /// Sets the width percentage.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">The value.</param>
        public static void SetWidthPercentage(FrameworkElement obj, int value)
        {
            obj.SetValue(WidthPercentageProperty, value);
        }

        /// <summary>
        /// Identifies the <c>WidthPercentage</c> attached property.
        /// </summary>
        public static readonly DependencyProperty WidthPercentageProperty =
            DependencyProperty.RegisterAttached("WidthPercentage", typeof(int), typeof(AnimationHelpers), new FrameworkPropertyMetadata(100, OnWidthPercentageChanged, CoercePercentage));

        private static void OnWidthPercentageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)o;

            int percent = (int)e.NewValue;

            if (percent == 100)
            {
                element.ClearValue(FrameworkElement.WidthProperty);
                ClearActualWidth(element);
            }
            else
            {
                double? actualWidth = GetActualWidth(element);
                if (actualWidth == null)
                {
                    if (element.IsArrangeValid)
                    {
                        actualWidth = element.ActualWidth;
                        SetActualWidth(element, actualWidth);
                    }
                    else
                    {
                        element.Loaded += DeferActualWidth;
                    }
                }

                if (actualWidth != null)
                {
                    SetWidth(element, percent, actualWidth.Value);
                }
            }
        }

        private static void DeferActualWidth(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            fe.Loaded -= DeferActualWidth;

            SetActualWidth(fe, fe.ActualWidth);
            SetWidth(fe, GetWidthPercentage(fe), fe.ActualWidth);
        }

        private static void SetWidth(FrameworkElement element, int percent, double actualWidth)
        {
            element.Width = (percent / 100D) * actualWidth;
        }

        #endregion

        #region ActualHeight (private)

        private static double? GetActualHeight(FrameworkElement obj)
        {
            return (double?)obj.GetValue(ActualHeightProperty);
        }

        private static void SetActualHeight(FrameworkElement obj, double? value)
        {
            obj.SetValue(ActualHeightPropertyKey, value);
        }

        private static void ClearActualHeight(FrameworkElement obj)
        {
            obj.ClearValue(ActualHeightPropertyKey);
        }

        private static readonly DependencyPropertyKey ActualHeightPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ActualHeight", typeof(double?), typeof(AnimationHelpers), new FrameworkPropertyMetadata());

        private static readonly DependencyProperty ActualHeightProperty = ActualHeightPropertyKey.DependencyProperty;

        #endregion

        #region HeightPercentage

        /// <summary>
        /// Gets the height percentage.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static int GetHeightPercentage(FrameworkElement obj)
        {
            return (int)obj.GetValue(HeightPercentageProperty);
        }

        /// <summary>
        /// Sets the height percentage.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">The value.</param>
        public static void SetHeightPercentage(FrameworkElement obj, int value)
        {
            obj.SetValue(HeightPercentageProperty, value);
        }

        /// <summary>
        /// Identifies the <c>HeightPercentage</c> attached property.
        /// </summary>
        public static readonly DependencyProperty HeightPercentageProperty =
            DependencyProperty.RegisterAttached("HeightPercentage", typeof(int), typeof(AnimationHelpers), new FrameworkPropertyMetadata(100, OnHeightPercentageChanged, CoercePercentage));
        
        private static void OnHeightPercentageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)o;

            int percent = (int)e.NewValue;

            if (percent == 100)
            {
                element.ClearValue(FrameworkElement.HeightProperty);
                ClearActualHeight(element);
            }
            else
            {
                double? actualHeight = GetActualHeight(element);
                if (actualHeight == null)
                {
                    if (element.IsArrangeValid)
                    {
                        actualHeight = element.ActualHeight;
                        SetActualHeight(element, actualHeight);
                    }
                    else
                    {
                        element.Loaded += DeferActualHeight;
                    }
                }

                if (actualHeight != null)
                {
                    SetHeight(element, percent, actualHeight.Value);
                }
            }
        }

        private static void DeferActualHeight(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            fe.Loaded -= DeferActualHeight;

            SetActualHeight(fe, fe.ActualHeight);
            SetHeight(fe, GetHeightPercentage(fe), fe.ActualHeight);
        }

        private static void SetHeight(FrameworkElement element, int percent, double actualHeight)
        {
            element.Height = (percent / 100D) * actualHeight;
        }

        #endregion

        #region Private Methods

        private static object CoercePercentage(DependencyObject o, object value)
        {
            int current = (int)value;

            if (current < 0)
            {
                current = 0;
            }

            return current;
        }
        
        #endregion

        #endregion
    }
}
