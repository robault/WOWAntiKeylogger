using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Avalon.Windows.Media.Animation
{
    /// <summary>
    /// Animates the value of a <typeparamref name="T"/> property along a set of <see cref="KeyFramesAnimationBase{T}.KeyFrames" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ContentProperty("KeyFrames")]
    public abstract class KeyFramesAnimationBase<T> : AnimationBase<T>, IKeyFrameAnimation, IAddChild
        where T : struct
    {
        #region Fields

        private bool _areKeyTimesValid = true;
        private KeyFrameCollection<T> _keyFrames;
        private ResolvedKeyFrameEntry[] _sortedResolvedKeyFrames;

        #endregion

        #region IAddChild Methods

        /// <summary>Adds a child <see cref="KeyFrame{T}" /> to this <see cref="KeyFramesAnimationBase{T}" />. </summary>
        /// <param name="child">The object to be added as the child of this <see cref="KeyFramesAnimationBase{T}" />. </param>
        /// <exception cref="System.ArgumentException">The parameter child is not a <see cref="KeyFrame{T}" />.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void AddChild(object child)
        {
            KeyFrame<T> keyFrame = child as KeyFrame<T>;
            if (keyFrame == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.KeyFramesAnimationBase_ChildNotKeyFrame, typeof(T).Name), "child");
            }
            KeyFrames.Add(keyFrame);
        }

        /// <summary>Adds a text string as a child of this <see cref="KeyFramesAnimationBase{T}" />.</summary>
        /// <param name="childText">The text added to the <see cref="KeyFramesAnimationBase{T}" />.</param>
        /// <exception cref="System.InvalidOperationException">A <see cref="KeyFramesAnimationBase{T}" /> does not accept text as a child, so this method will raise this exception unless a derived class has overridden this behavior which allows text to be added.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void AddText(string childText)
        {
            throw new InvalidOperationException("Text children not allowed.");
        }

        void IAddChild.AddChild(object child)
        {
            WritePreamble();
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            AddChild(child);
            WritePostscript();
        }

        void IAddChild.AddText(string childText)
        {
            if (childText == null)
            {
                throw new ArgumentNullException("childText");
            }
            AddText(childText);
        }

        #endregion
        
        #region Clone/Freeze Methods

        /// <summary>Creates a modifiable clone of this <see cref="KeyFramesAnimationBase{T}" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
        /// <returns>A modifiable clone of the current object. The cloned object's <see cref="System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="System.Windows.Freezable.IsFrozen" /> property was true.</returns>
        public new KeyFramesAnimationBase<T> Clone()
        {
            return (KeyFramesAnimationBase<T>)base.Clone();
        }

        /// <summary>Makes this instance a deep copy of the specified <see cref="KeyFramesAnimationBase{T}" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
        /// <param name="sourceFreezable">The <see cref="KeyFramesAnimationBase{T}" /> to clone.</param>
        protected override void CloneCore(Freezable sourceFreezable)
        {
            KeyFramesAnimationBase<T> frames = (KeyFramesAnimationBase<T>)sourceFreezable;
            base.CloneCore(sourceFreezable);
            CopyCommon(frames, false);
        }

        /// <summary>Creates a modifiable clone of this <see cref="KeyFramesAnimationBase{T}" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
        /// <returns>A modifiable clone of the current object. The cloned object's <see cref="System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="System.Windows.Freezable.IsFrozen" /> property was true.</returns>
        public new KeyFramesAnimationBase<T> CloneCurrentValue()
        {
            return (KeyFramesAnimationBase<T>)base.CloneCurrentValue();
        }

        /// <summary>Makes this instance a modifiable deep copy of the specified <see cref="KeyFramesAnimationBase{T}" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
        /// <param name="sourceFreezable">The <see cref="KeyFramesAnimationBase{T}" /> to clone.</param>
        protected override void CloneCurrentValueCore(Freezable sourceFreezable)
        {
            KeyFramesAnimationBase<T> anim = (KeyFramesAnimationBase<T>)sourceFreezable;
            base.CloneCurrentValueCore(sourceFreezable);
            CopyCommon(anim, true);
        }

        private void CopyCommon(KeyFramesAnimationBase<T> sourceAnimation, bool isCurrentValueClone)
        {
            _areKeyTimesValid = sourceAnimation._areKeyTimesValid;
            if (_areKeyTimesValid && (sourceAnimation._sortedResolvedKeyFrames != null))
            {
                _sortedResolvedKeyFrames = (ResolvedKeyFrameEntry[])sourceAnimation._sortedResolvedKeyFrames.Clone();
            }
            if (sourceAnimation._keyFrames != null)
            {
                if (isCurrentValueClone)
                {
                    _keyFrames = (KeyFrameCollection<T>)sourceAnimation._keyFrames.CloneCurrentValue();
                }
                else
                {
                    _keyFrames = sourceAnimation._keyFrames.Clone();
                }
                OnFreezablePropertyChanged(null, _keyFrames);
            }
        }

        /// <summary>Creates a new instance of <see cref="KeyFramesAnimationBase{T}" />. </summary>
        /// <returns>A new instance of <see cref="KeyFramesAnimationBase{T}" />.</returns>
        protected abstract override Freezable CreateInstanceCore();

        /// <summary>Makes this instance of <see cref="KeyFramesAnimationBase{T}" /> object unmodifiable or determines whether it can be made unmodifiable..</summary>
        /// <returns>If isChecking is true, this method returns true if this <see cref="KeyFramesAnimationBase{T}" /> can be made unmodifiable, or false if it cannot be made unmodifiable. If isChecking is false, this method returns true if the specified <see cref="KeyFramesAnimationBase{T}" /> is now unmodifiable, or false if it cannot be made unmodifiable, with the side effect of having made the actual change in frozen status to this object.</returns>
        /// <param name="isChecking">true if the <see cref="KeyFramesAnimationBase{T}" /> instance should actually freeze itself when this method is called. false if the <see cref="KeyFramesAnimationBase{T}" /> instance should simply return whether it can be frozen.</param>
        protected override bool FreezeCore(bool isChecking)
        {
            bool freeze = base.FreezeCore(isChecking) && Freezable.Freeze(_keyFrames, isChecking);
            if (freeze & !_areKeyTimesValid)
            {
                ResolveKeyTimes();
            }
            return freeze;
        }

        /// <summary>Makes this instance a clone of the specified <see cref="KeyFramesAnimationBase{T}" /> object. </summary>
        /// <param name="sourceFreezable">The <see cref="KeyFramesAnimationBase{T}" /> object to clone.</param>
        protected override void GetAsFrozenCore(Freezable sourceFreezable)
        {
            KeyFramesAnimationBase<T> frames = (KeyFramesAnimationBase<T>)sourceFreezable;
            base.GetAsFrozenCore(sourceFreezable);
            CopyCommon(frames, false);
        }

        /// <summary>Makes this instance a frozen clone of the specified <see cref="KeyFramesAnimationBase{T}" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
        /// <param name="sourceFreezable">The <see cref="KeyFramesAnimationBase{T}" /> to copy and freeze.</param>
        protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
        {
            KeyFramesAnimationBase<T> frames = (KeyFramesAnimationBase<T>)sourceFreezable;
            base.GetCurrentValueAsFrozenCore(sourceFreezable);
            CopyCommon(frames, true);
        }

        #endregion

        #region Animation Methods

        /// <summary> Calculates a value that represents the current value of the property being animated, as determined by this instance of <see cref="KeyFramesAnimationBase{T}" />.</summary>
        /// <returns>The calculated value of the property, as determined by the current instance.</returns>
        /// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
        /// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
        /// <param name="animationClock">An <see cref="System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the host animation.</param>
        protected sealed override T GetCurrentValueCore(T defaultOriginValue, T defaultDestinationValue, AnimationClock animationClock)
        {
            T value;
            if (_keyFrames == null)
            {
                return defaultDestinationValue;
            }
            if (!_areKeyTimesValid)
            {
                ResolveKeyTimes();
            }
            if (_sortedResolvedKeyFrames == null)
            {
                return defaultDestinationValue;
            }
            TimeSpan currentTime = animationClock.CurrentTime.Value;
            int frames = _sortedResolvedKeyFrames.Length;
            int lastFrameIndex = frames - 1;
            int frameCount = 0;
            while ((frameCount < frames) && (currentTime > _sortedResolvedKeyFrames[frameCount].resolvedKeyTime))
            {
                frameCount++;
            }
            while ((frameCount < lastFrameIndex) && (currentTime == _sortedResolvedKeyFrames[frameCount + 1].resolvedKeyTime))
            {
                frameCount++;
            }
            if (frameCount == frames)
            {
                value = GetResolvedKeyFrameValue(lastFrameIndex);
            }
            else if (currentTime == _sortedResolvedKeyFrames[frameCount].resolvedKeyTime)
            {
                value = GetResolvedKeyFrameValue(frameCount);
            }
            else
            {
                T newValue;
                double progress = 0;
                if (frameCount == 0)
                {
                    if (IsAdditive)
                    {
                        newValue = Calculator.GetZeroValue(defaultOriginValue);
                    }
                    else
                    {
                        newValue = defaultOriginValue;
                    }
                    progress = currentTime.TotalMilliseconds / _sortedResolvedKeyFrames[0].resolvedKeyTime.TotalMilliseconds;
                }
                else
                {
                    int prevFrame = frameCount - 1;
                    TimeSpan keyTime = _sortedResolvedKeyFrames[prevFrame].resolvedKeyTime;
                    newValue = GetResolvedKeyFrameValue(prevFrame);
                    TimeSpan timeDiff = currentTime - keyTime;
                    TimeSpan totalTimeDiff = _sortedResolvedKeyFrames[frameCount].resolvedKeyTime - keyTime;
                    progress = timeDiff.TotalMilliseconds / totalTimeDiff.TotalMilliseconds;
                }
                value = GetResolvedKeyFrame(frameCount).InterpolateValue(newValue, progress);
            }
            if (IsCumulative)
            {
                int? currentIteration = animationClock.CurrentIteration;
                currentIteration = currentIteration != null ? new int?(currentIteration.Value - 1) : new int?();
                double currentValue = (double)currentIteration.Value;
                if (currentValue > 0)
                {
                    value = Calculator.Add(value, Calculator.Scale(GetResolvedKeyFrameValue(lastFrameIndex), currentValue));
                }
            }
            if (IsAdditive)
            {
                return Calculator.Add(defaultOriginValue, value);
            }
            return value;
        }

        /// <summary>Provide a custom natural <see cref="System.Windows.Duration" /> when the <see cref="System.Windows.Duration" /> property is set to <see cref="System.Windows.Duration.Automatic" />. </summary>
        /// <returns>If the last key frame of this animation is a <see cref="System.Windows.Media.Animation.KeyTime" />, then this value is used as the <see cref="System.Windows.Media.Animation.Clock.NaturalDuration" />; otherwise it will be one second.</returns>
        /// <param name="clock">The <see cref="System.Windows.Media.Animation.Clock" /> whose natural duration is desired.</param>
        protected sealed override Duration GetNaturalDurationCore(Clock clock)
        {
            return new Duration(LargestTimeSpanKeyTime);
        }

        private KeyFrame<T> GetResolvedKeyFrame(int resolvedKeyFrameIndex)
        {
            return _keyFrames[_sortedResolvedKeyFrames[resolvedKeyFrameIndex].originalKeyFrameIndex];
        }

        private T GetResolvedKeyFrameValue(int resolvedKeyFrameIndex)
        {
            return GetResolvedKeyFrame(resolvedKeyFrameIndex).Value;
        }

        /// <summary>Called when the current <see cref="KeyFramesAnimationBase{T}" /> object is modified.</summary>
        protected override void OnChanged()
        {
            _areKeyTimesValid = false;
            base.OnChanged();
        }

        private void ResolveKeyTimes()
        {
            int keyFramesCount = 0;
            if (_keyFrames != null)
            {
                keyFramesCount = _keyFrames.Count;
            }
            if (keyFramesCount == 0)
            {
                _sortedResolvedKeyFrames = null;
                _areKeyTimesValid = true;
            }
            else
            {
                _sortedResolvedKeyFrames = new ResolvedKeyFrameEntry[keyFramesCount];
                int currentFrameIndex = 0;
                while (currentFrameIndex < keyFramesCount)
                {
                    _sortedResolvedKeyFrames[currentFrameIndex].originalKeyFrameIndex = currentFrameIndex;
                    currentFrameIndex++;
                }
                TimeSpan time = TimeSpan.Zero;
                Duration duration = Duration;
                if (duration.HasTimeSpan)
                {
                    time = duration.TimeSpan;
                }
                else
                {
                    time = LargestTimeSpanKeyTime;
                }
                int lastKeyFrameIndex = keyFramesCount - 1;
                List<KeyTimeBlock> blocks = new List<KeyTimeBlock>();
                bool isPaced = false;
                currentFrameIndex = 0;
                while (currentFrameIndex < keyFramesCount)
                {
                    KeyTime keyTime = _keyFrames[currentFrameIndex].KeyTime;
                    switch (keyTime.Type)
                    {
                        case KeyTimeType.Uniform:
                        case KeyTimeType.Paced:
                            {
                                if (currentFrameIndex != lastKeyFrameIndex)
                                {
                                    break;
                                }
                                _sortedResolvedKeyFrames[currentFrameIndex].resolvedKeyTime = time;
                                currentFrameIndex++;
                                continue;
                            }
                        case KeyTimeType.Percent:
                            {
                                _sortedResolvedKeyFrames[currentFrameIndex].resolvedKeyTime = TimeSpan.FromMilliseconds(keyTime.Percent * time.TotalMilliseconds);
                                currentFrameIndex++;
                                continue;
                            }
                        case KeyTimeType.TimeSpan:
                            {
                                _sortedResolvedKeyFrames[currentFrameIndex].resolvedKeyTime = keyTime.TimeSpan;
                                currentFrameIndex++;
                                continue;
                            }
                        default:
                            {
                                continue;
                            }
                    }
                    if ((currentFrameIndex == 0) && (keyTime.Type == KeyTimeType.Paced))
                    {
                        _sortedResolvedKeyFrames[currentFrameIndex].resolvedKeyTime = TimeSpan.Zero;
                        currentFrameIndex++;
                        continue;
                    }
                    if (keyTime.Type == KeyTimeType.Paced)
                    {
                        isPaced = true;
                    }
                    KeyTimeBlock block = new KeyTimeBlock();
                    block.BeginIndex = currentFrameIndex;
                    while (++currentFrameIndex < lastKeyFrameIndex)
                    {
                        KeyTimeType keyTimeType = _keyFrames[currentFrameIndex].KeyTime.Type;
                        if (keyTimeType == KeyTimeType.Paced)
                        {
                            isPaced = true;
                        }
                    }
                    block.EndIndex = currentFrameIndex;
                    blocks.Add(block);
                }
                for (int blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
                {
                    KeyTimeBlock block = blocks[blockIndex];
                    TimeSpan blockTime = TimeSpan.Zero;
                    if (block.BeginIndex > 0)
                    {
                        blockTime = _sortedResolvedKeyFrames[block.BeginIndex - 1].resolvedKeyTime;
                    }
                    long blockIndices = (block.EndIndex - block.BeginIndex) + 1;
                    TimeSpan blockTimeDiff = _sortedResolvedKeyFrames[block.EndIndex].resolvedKeyTime - blockTime;
                    TimeSpan blockAvgTime = TimeSpan.FromTicks(blockTimeDiff.Ticks / blockIndices);
                    currentFrameIndex = block.BeginIndex;
                    TimeSpan currentBlockTime = blockTime + blockAvgTime;
                    while (currentFrameIndex < block.EndIndex)
                    {
                        _sortedResolvedKeyFrames[currentFrameIndex].resolvedKeyTime = currentBlockTime;
                        currentBlockTime += blockAvgTime;
                        currentFrameIndex++;
                    }
                }
                if (isPaced)
                {
                    ResolvePacedKeyTimes();
                }
                Array.Sort<ResolvedKeyFrameEntry>(_sortedResolvedKeyFrames);
                _areKeyTimesValid = true;
            }
        }

        private void ResolvePacedKeyTimes()
        {
            int count = 1;
            int lastFrameIndex = _sortedResolvedKeyFrames.Length - 1;
            do
            {
                if (_keyFrames[count].KeyTime.Type == KeyTimeType.Paced)
                {
                    int originalCount = count;
                    List<double> segLengthSums = new List<double>();
                    TimeSpan time = _sortedResolvedKeyFrames[count - 1].resolvedKeyTime;
                    double segLengthSum = 0;
                    T value = _keyFrames[count - 1].Value;
                    do
                    {
                        T newValue = _keyFrames[count].Value;
                        segLengthSum += Calculator.GetSegmentLength(value, newValue);
                        segLengthSums.Add(segLengthSum);
                        value = newValue;
                        count++;
                    }
                    while ((count < lastFrameIndex) && (_keyFrames[count].KeyTime.Type == KeyTimeType.Paced));
                    segLengthSum += Calculator.GetSegmentLength(value, _keyFrames[count].Value);
                    TimeSpan timeDiff = _sortedResolvedKeyFrames[count].resolvedKeyTime - time;
                    for (int i = 0, j = originalCount; i < segLengthSums.Count; j++)
                    {
                        _sortedResolvedKeyFrames[j].resolvedKeyTime = time + TimeSpan.FromMilliseconds((segLengthSums[i] / segLengthSum) * timeDiff.TotalMilliseconds);
                        i++;
                    }
                }
                else
                {
                    count++;
                }
            }
            while (count < lastFrameIndex);
        }

        /// <summary>Returns true if the value of the <see cref="KeyFramesAnimationBase{T}.KeyFrames" /> property of this instance of <see cref="KeyFramesAnimationBase{T}" /> should be value-serialized.</summary>
        /// <returns>true if the property value should be serialized; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeKeyFrames()
        {
            ReadPreamble();
            if (_keyFrames != null)
            {
                return (_keyFrames.Count > 0);
            }
            return false;
        }

        #endregion

        #region Properties

        /// <summary> Gets or sets the collection of <see cref="KeyFrame{T}" /> objects that define the animation. </summary>
        /// <returns>The collection of <see cref="KeyFrame{T}" /> objects that define the animation. The default value is <see cref="KeyFrameCollection{T}.Empty" />.</returns>
        public KeyFrameCollection<T> KeyFrames
        {
            get
            {
                ReadPreamble();
                if (_keyFrames == null)
                {
                    if (IsFrozen)
                    {
                        _keyFrames = KeyFrameCollection<T>.Empty;
                    }
                    else
                    {
                        WritePreamble();
                        _keyFrames = new KeyFrameCollection<T>();
                        OnFreezablePropertyChanged(null, _keyFrames);
                        WritePostscript();
                    }
                }
                return _keyFrames;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                WritePreamble();
                if (value != _keyFrames)
                {
                    OnFreezablePropertyChanged(_keyFrames, value);
                    _keyFrames = value;
                    WritePostscript();
                }
            }
        }

        private TimeSpan LargestTimeSpanKeyTime
        {
            get
            {
                bool success = false;
                TimeSpan time = TimeSpan.Zero;
                if (_keyFrames != null)
                {
                    for (int i = 0; i < _keyFrames.Count; i++)
                    {
                        KeyTime newTime = _keyFrames[i].KeyTime;
                        if (newTime.Type == KeyTimeType.TimeSpan)
                        {
                            success = true;
                            if (newTime.TimeSpan > time)
                            {
                                time = newTime.TimeSpan;
                            }
                        }
                    }
                }
                if (success)
                {
                    return time;
                }
                return TimeSpan.FromSeconds(1);
            }
        }

        System.Collections.IList IKeyFrameAnimation.KeyFrames
        {
            get { return KeyFrames; }
            set { KeyFrames = (KeyFrameCollection<T>)value; }
        }

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyTimeBlock
        {
            public int BeginIndex;
            public int EndIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ResolvedKeyFrameEntry : IComparable<ResolvedKeyFrameEntry>
        {
            internal int originalKeyFrameIndex;
            internal TimeSpan resolvedKeyTime;

            public int CompareTo(ResolvedKeyFrameEntry other)
            {
                if (other.resolvedKeyTime > resolvedKeyTime)
                {
                    return -1;
                }
                if (other.resolvedKeyTime < resolvedKeyTime)
                {
                    return 1;
                }
                if (other.originalKeyFrameIndex > originalKeyFrameIndex)
                {
                    return -1;
                }
                if (other.originalKeyFrameIndex < originalKeyFrameIndex)
                {
                    return 1;
                }
                return 0;
            }
        }

        #endregion
    }
}
