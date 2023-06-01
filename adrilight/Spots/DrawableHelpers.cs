using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace adrilight.Spots
{
    public class DrawableHelpers
    {
        public Rect GetBound(Rect[] rects)
        {
            double xMin = rects.Min(s => s.Left);
            double yMin = rects.Min(s => s.Top);
            double xMax = rects.Max(s => s.Left + s.Width);
            double yMax = rects.Max(s => s.Top + s.Height);
            var rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            return rect;
        }
        public Rect GetBound(IControlZone[] zones)
        {
            var listRect = new List<Rect>();
            foreach (var zone in zones)
            {
                double top = (zone as IDrawable).Top;
                double left = (zone as IDrawable).Left;
                double width = (zone as IDrawable).Width;
                double height = (zone as IDrawable).Height;

                listRect.Add(new Rect(left, top, width, height));
            }




            return GetBound(listRect.ToArray());
        }
        public Rect GetRealBound(IControlZone[] zones)
        {
            var listRect = new List<Rect>();
            foreach (var zone in zones)
            {
                double top = (zone as IDrawable).GetRect.Top;
                double left = (zone as IDrawable).GetRect.Left;
                double width = (zone as IDrawable).GetRect.Width;
                double height = (zone as IDrawable).GetRect.Height;

                listRect.Add(new Rect(left, top, width, height));
            }




            return GetBound(listRect.ToArray());
        }
        public Rect GetRealBound(IDrawable[] zones)
        {
            var listRect = new List<Rect>();
            foreach (var zone in zones)
            {
                double top = zone.GetRect.Top;
                double left = zone.GetRect.Left;
                double width = zone.GetRect.Width;
                double height = zone.GetRect.Height;

                listRect.Add(new Rect(left, top, width, height));
            }




            return GetBound(listRect.ToArray());
        }
        public Rect GetBound(List<IDrawable> items)
        {
            var listRect = new List<Rect>();
            foreach (var item in items)
            {
                if (item == null)
                    continue;
                double top = (item as IDrawable).Top;
                double left = (item as IDrawable).Left;
                double width = (item as IDrawable).Width;
                double height = (item as IDrawable).Height;

                listRect.Add(new Rect(left, top, width, height));
            }




            return GetBound(listRect.ToArray());
        }
        public Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point {
                X =

                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =

                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        public Rect RotateRectangle(Rect inputRect, Point centerPoint, double angleInDegrees)
        {
            //rotate 4 point
            var newTopLeft = RotatePoint(inputRect.TopLeft, centerPoint, angleInDegrees);
            var newTopRight = RotatePoint(inputRect.TopRight, centerPoint, angleInDegrees);
            var newBottomLeft = RotatePoint(inputRect.BottomLeft, centerPoint, angleInDegrees);
            var newTopBottomRight = RotatePoint(inputRect.BottomRight, centerPoint, angleInDegrees);
            var listPoint = new List<Point>();
            listPoint.Add(newTopLeft); listPoint.Add(newTopRight); listPoint.Add(newBottomLeft); listPoint.Add(newTopBottomRight);
            double minX = listPoint.Min(p => p.X);
            double minY = listPoint.Min(p => p.Y);
            double maxX = listPoint.Max(p => p.X);
            double MaxY = listPoint.Max(p => p.Y);
            //get boundingBox
            var rect = new Rect(minX, minY, maxX - minX, MaxY - minY);
            return rect;
        }
    }
}
