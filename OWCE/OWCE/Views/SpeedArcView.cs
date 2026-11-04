using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Messaging;
using OWCE.Themes;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace OWCE.Views
{
    /// <summary>
    /// The speed gauge: a semicircle with a rounded track, and a rounded arc on it that grows with the speed.
    /// It is drawn on the speed panel's colour, in the panel's text colour.
    /// </summary>
    public class SpeedArcView : SKCanvasView
    {
        // The gauge is the top half of a circle: 180 degrees, from the left (9 o'clock) over the top to the right.
        const float GaugeStartAngle = 180f;
        const float GaugeSweep = 180f;

        // Even at a standstill a dot of arc is shown, so the start of the gauge is visible.
        const float MinSweepAngle = 0.5f;

        SKPaint _backgroundPaint;
        SKPaint _trackPaint;
        SKPaint _arcPaint;
        float _displayScale;
        float _radius;
        float _centerX;
        SKRect _arcRect;

        float _currentSweepAngle = MinSweepAngle;
        int _animationNumber = 0;

        public static readonly BindableProperty CurrentRPMProperty = BindableProperty.Create(
            propertyName: nameof(CurrentRPM),
            returnType: typeof(int),
            declaringType: typeof(SpeedArcView),
            defaultValue: 0);

        public int CurrentRPM
        {
            get { return (int)GetValue(CurrentRPMProperty); }
            set { SetValue(CurrentRPMProperty, value); }
        }

        public static readonly BindableProperty MinRPMProperty = BindableProperty.Create(
            propertyName: nameof(MinRPM),
            returnType: typeof(int),
            declaringType: typeof(SpeedArcView),
            defaultValue: 0);

        public int MinRPM
        {
            get { return (int)GetValue(MinRPMProperty); }
            set { SetValue(MinRPMProperty, value); }
        }

        public static readonly BindableProperty MaxRPMProperty = BindableProperty.Create(
            propertyName: nameof(MaxRPM),
            returnType: typeof(int),
            declaringType: typeof(SpeedArcView),
            defaultValue: 0);

        public int MaxRPM
        {
            get { return (int)GetValue(MaxRPMProperty); }
            set { SetValue(MaxRPMProperty, value); }
        }

        public SpeedArcView()
        {
            _backgroundPaint = new SKPaint();
            _trackPaint = new SKPaint() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
            _arcPaint = new SKPaint() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
            ApplyThemeColors();

            _displayScale = (float)DeviceDisplay.Current.MainDisplayInfo.Density;

            // Redraw in the new colours when the theme changes.
            // The handler must not capture "this", or the messenger would keep the view alive forever.
            WeakReferenceMessenger.Default.Register<SpeedArcView, ThemeChangedMessage>(this, (recipient, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    recipient.ApplyThemeColors();
                    recipient.InvalidateSurface();
                });
            });
        }

        // The track is the panel's text colour at low opacity, the arc is the text colour itself.
        void ApplyThemeColors()
        {
            var panel = ThemeManager.GetColor("SpeedPanel", Colors.Yellow).ToSKColor();
            var on = ThemeManager.GetColor("OnSpeedPanel", Colors.Black).ToSKColor();

            _backgroundPaint.Color = panel;
            _trackPaint.Color = on.WithAlpha(38); // 15% alpha
            _arcPaint.Color = on;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width <= 0)
            {
                return;
            }

            // Scale up for units we use to draw with SkiaSharp.
            var scaledWidth = (float)(width * _displayScale);

            // The gauge is as wide as the panel, less some padding, and sits at the top of it.
            var padding = scaledWidth * 0.24f;
            var outerRadius = (scaledWidth - padding) * 0.5f;

            var strokeWidth = outerRadius * 0.11f;
            _trackPaint.StrokeWidth = strokeWidth;
            _arcPaint.StrokeWidth = strokeWidth;

            // The stroke is centred on the arc, so keep it inside the circle.
            _radius = outerRadius - (strokeWidth * 0.5f);
            _centerX = scaledWidth * 0.5f;

            // The circle's centre is on the bottom edge of the panel (the gauge is its top half).
            var centerY = outerRadius;
            _arcRect = new SKRect(_centerX - _radius, centerY - _radius, _centerX + _radius, centerY + _radius);

            // The panel is as tall as the gauge, plus a little for the ends of the arc.
            var screenOuterRadius = outerRadius / _displayScale;
            HeightRequest = screenOuterRadius + (screenOuterRadius * 0.0288f);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == CurrentRPMProperty.PropertyName
                || propertyName == MinRPMProperty.PropertyName
                || propertyName == MaxRPMProperty.PropertyName)
            {
                UpdateTargetArcAngle();
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var canvas = e.Surface.Canvas;
            canvas.Clear(_backgroundPaint.Color);

            if (_radius <= 0)
            {
                return;
            }

            canvas.DrawArc(_arcRect, GaugeStartAngle, GaugeSweep, false, _trackPaint);
            canvas.DrawArc(_arcRect, GaugeStartAngle, _currentSweepAngle, false, _arcPaint);
        }

        void UpdateTargetArcAngle()
        {
            var range = MaxRPM - MinRPM;
            var fraction = range <= 0 ? 0f : Math.Clamp((CurrentRPM - MinRPM) / (float)range, 0f, 1f);
            var targetSweepAngle = Math.Max(MinSweepAngle, GaugeSweep * fraction);

            var oldSweepAngle = _currentSweepAngle;

            ++_animationNumber;

            var animation = new Animation(v =>
            {
                _currentSweepAngle = (float)v;
                InvalidateSurface();
            }, oldSweepAngle, targetSweepAngle);

            animation.Commit(this, "SimpleAnimation", 16, 250, Easing.Linear);
        }
    }
}
