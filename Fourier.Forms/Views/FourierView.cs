using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Fourier.Forms.Views
{
    public class FourierView : ContentView
    {
        const float DegreeToRadFactor = 0.01745329252f;
        private const string ComponentsAnimationName = "ComponentsAnimation";

        SKPath _concatPath = new SKPath();
        SKPath _vectorsPath = new SKPath();
        SKPath _circlesPath = new SKPath();

        SKCanvasView _canvasView;

        List<Component> _components = new List<Component>();
        readonly Random rand = new Random();

        SKPaint _concatPaint = new SKPaint()
        {
            Color = SKColors.DarkOrange,
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 10
        };

        SKPaint _vectorsPaint = new SKPaint()
        {
            Color = SKColors.White,
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 4
        };

        SKPaint _circlesPaint = new SKPaint()
        {
            Color = new SKColor(0xFF, 0xFF, 0xFF, 0x66),
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 2
        };

        public FourierView()
        {
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += PaintFourier;
            Content = _canvasView;

            InitComponents();
        }


        protected override void OnParentSet()
        {
            base.OnParentSet();
            _canvasView?.InvalidateSurface();

            RunRandomVectorAnimation();

            Console.WriteLine("Running");
        }

        public void Anew()
        {
            this.AbortAnimation(ComponentsAnimationName);

            _concatPath.Reset();
            _vectorsPath.Reset();

            InitComponents();

            RunRandomVectorAnimation();
        }

        private void InitComponents()
        {
            _components.Clear();

            var amount = rand.Next(5, 20);

            for (int i = 0; i < amount; i++)
            {
                _components.Add(
                    new Component
                    {
                        Vector = new Vector
                        {
                            Magnitude = (float)(50 + 100 * rand.NextDouble()),
                            Angle = 0.0f
                        },
                        RotationFactor = (float)(0.5f + 3.5f * rand.NextDouble())
                    }
                );
            }
        }


        private void RunRandomVectorAnimation()
        {
            var animation = new Animation(
                interpolatedValue =>
                {
                    foreach (var component in _components)
                    {
                        component.Vector.Angle = (float)(interpolatedValue * 360.0f * component.RotationFactor);
                    }

                    _circlesPath.Reset();
                    _vectorsPath.Reset();
                    var superposition = new SKPoint(0.0f, 0.0f);
                    foreach (var component in _components)
                    {
                        var circleCenter = new SKPoint(superposition.X, superposition.Y);

                        var currentX = (float)(component.Vector.Magnitude * Math.Cos(component.Vector.Angle * DegreeToRadFactor));
                        var currentY = (float)(component.Vector.Magnitude * Math.Sin(component.Vector.Angle * DegreeToRadFactor));

                        superposition.X += currentX;
                        superposition.Y += currentY;

                        _vectorsPath.LineTo(superposition);

                        var circleRadius = (float)Math.Sqrt(Math.Pow(superposition.X - circleCenter.X, 2) + Math.Pow(circleCenter.Y - superposition.Y, 2));
                        _circlesPath.AddCircle(circleCenter.X, circleCenter.Y, circleRadius);
                    }

                    _concatPath.LineTo(superposition);

                    _canvasView.InvalidateSurface();
                },
                end: 1.0d
            );

            animation.Commit(this, ComponentsAnimationName,
                length: 20000,
                repeat: () => true,
                finished: (d, i) =>
                {
                    _concatPath.Reset();
                    _vectorsPath.Reset();

                    _canvasView.InvalidateSurface();
                });
        }

        private void PaintFourier(object sender, SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);

            var canvasCenter = new SKPoint(info.Size.Width / 2.0f, info.Size.Height / 2.0f);
            canvas.Translate(canvasCenter);

            canvas.DrawPath(_circlesPath, _circlesPaint);
            canvas.DrawPath(_vectorsPath, _vectorsPaint);
            canvas.DrawPath(_concatPath, _concatPaint);
        }
    }

    internal class Component
    {
        public Vector Vector { get; set; }
        public float RotationFactor { get; set; }
    }

    internal class Vector
    {
        public float Angle { get; set; }
        public float Magnitude { get; set; }
    }
}