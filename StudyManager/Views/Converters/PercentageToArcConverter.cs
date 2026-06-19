using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StudyManager.Views.Converters
{
    public class PercentageToArcConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double percentage = 0.0;
            if (value is double d)
            {
                percentage = d;
            }
            else if (value is int i)
            {
                percentage = i;
            }
            else if (value is float f)
            {
                percentage = f;
            }

            // Cap percentage
            if (percentage < 0.0) percentage = 0.0;
            if (percentage > 100.0) percentage = 100.0;
            
            // Special case: 100% can look like 0% if end point matches start point,
            // so we make it 99.99% to keep the arc rendering correctly.
            if (percentage >= 100.0)
            {
                percentage = 99.99;
            }

            double radius = 12.0;
            Point center = new Point(14, 14); // assuming a 28x28 canvas/bounding box
            double angle = percentage * 3.6; // 360 degrees * (percentage / 100)
            double angleRad = (angle - 90.0) * Math.PI / 180.0; // Start at top (-90 degrees)

            double x = center.X + radius * Math.Cos(angleRad);
            double y = center.Y + radius * Math.Sin(angleRad);

            Point startPoint = new Point(center.X, center.Y - radius);
            Point endPoint = new Point(x, y);
            bool isLargeArc = percentage > 50.0;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false
            };

            ArcSegment arc = new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLargeArc
            };

            figure.Segments.Add(arc);
            geometry.Figures.Add(figure);

            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
