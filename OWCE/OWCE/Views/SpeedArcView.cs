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
        SKPaint _glowPaint;
        SKPaint _tickPaint;
        SKPaint _tipPaint;
        float _radius;
        float _centerX;
        float _centerY;
        float _strokeWidth;
        SKRect _arcRect;
        SKColor _onColor;

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
            _glowPaint = new SKPaint() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
            _tickPaint = new SKPaint() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
            _tipPaint = new SKPaint() { IsAntialias = true, Style = SKPaintStyle.Fill };
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
            _onColor = on;
            _glowPaint.Color = on.WithAlpha(110);
            _tipPaint.Color = on;
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

            // The gauge is as wide as the panel, less some padding.
            var padding = scaledWidth * 0.24f;
            var outerRadius = (scaledWidth - padding) * 0.5f;

            _strokeWidth = outerRadius * 0.11f;
            _trackPaint.StrokeWidth = _strokeWidth;
            _arcPaint.StrokeWidth = _strokeWidth;
            _glowPaint.StrokeWidth = _strokeWidth * 1.15f;
            _glowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _strokeWidth * 0.55f);
            _tickPaint.StrokeWidth = Math.Max(2f, _strokeWidth * 0.12f);

            // The stroke is centred on the arc, so keep it inside the circle.
            _radius = outerRadius - (_strokeWidth * 0.5f);
            _centerX = scaledWidth * 0.5f;

            // Room above the gauge, so it does not touch the top of the panel.
            var topPad = outerRadius * 0.16f;

            // The circle's centre is at the bottom of the gauge (the gauge is its top half).
            _centerY = topPad + outerRadius;
            _arcRect = new SKRect(_centerX - _radius, _centerY - _radius, _centerX + _radius, _centerY + _radius);

            // The panel is as tall as the gauge, the room above it, and a little for the round ends of the arc.
            var screenHeight = (topPad + outerRadius + _strokeWidth * 0.5f) / _displayScale;
            HeightRequest = screenHeight + 4;
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

            // Tick marks just inside the track, a longer one every quarter.
            var tickOuter = _radius - _strokeWidth * 0.95f;
            for (var i = 0; i <= 20; ++i)
            {
                var major = i % 5 == 0;
                var angle = (GaugeStartAngle + GaugeSweep * i / 20f) * (float)Math.PI / 180f;
                var inner = tickOuter - _strokeWidth * (major ? 0.55f : 0.28f);
                var reached = (GaugeSweep * i / 20f) <= _currentSweepAngle + 0.5f;

                _tickPaint.Color = _onColor.WithAlpha((byte)(reached ? (major ? 200 : 140) : (major ? 90 : 55)));
                canvas.DrawLine(
                    _centerX + tickOuter * (float)Math.Cos(angle), _centerY + tickOuter * (float)Math.Sin(angle),
                    _centerX + inner * (float)Math.Cos(angle), _centerY + inner * (float)Math.Sin(angle),
                    _tickPaint);
            }

            if (_currentSweepAngle > MinSweepAngle + 1f)
            {
                // A soft glow under the arc.
                canvas.DrawArc(_arcRect, GaugeStartAngle, _currentSweepAngle, false, _glowPaint);

                // The arc fades up from dim at the start to full strength at its tip. (A sweep gradient starts at 3 o'clock, so
                // the canvas is turned half a circle to make it start at 9 o'clock.)
                canvas.Save();
                canvas.RotateDegrees(180f, _centerX, _centerY);
                using (var shader = SKShader.CreateSweepGradient(
                    new SKPoint(_centerX, _centerY),
                    new[] { _onColor.WithAlpha(110), _onColor, _onColor },
                    new[] { 0f, Math.Max(0.02f, _currentSweepAngle / 360f), 1f }))
                {
                    _arcPaint.Shader = shader;
                    canvas.DrawArc(_arcRect, 0f, _currentSweepAngle, false, _arcPaint);
                    _arcPaint.Shader = null;
                }
                canvas.Restore();

                // A bright dot at the tip.
                var tip = (GaugeStartAngle + _currentSweepAngle) * (float)Math.PI / 180f;
                canvas.DrawCircle(_centerX + _radius * (float)Math.Cos(tip), _centerY + _radius * (float)Math.Sin(tip), _strokeWidth * 0.78f, _tipPaint);
            }
            else
            {
                // At a standstill, just the dot where the gauge starts.
                canvas.DrawArc(_arcRect, GaugeStartAngle, _currentSweepAngle, false, _arcPaint);
            }
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
                _currentSweepAngle = Math.Clamp((float)v, MinSweepAngle, GaugeSweep);
                InvalidateSurface();
            }, oldSweepAngle, targetSweepAngle);

            animation.Commit(this, "SimpleAnimation", 16, 500, Easing.SpringOut);
        }
    }
}
