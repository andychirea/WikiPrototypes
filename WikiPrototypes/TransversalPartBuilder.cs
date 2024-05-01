using Rhino.Geometry;

namespace WikiPrototypes
{
    public static class TransversalPartBuilder
    {
        public static Curve GetContour()
        {
            var points = new Point3d[]
            {
                new Point3d(30.0, 2.5, 0),
                new Point3d(27.6, 2.5, 0),
                new Point3d(27.6, 3.7, 0),
                new Point3d(28.2, 3.7, 0),
                new Point3d(28.2, 12.5, 0),
                new Point3d(7.20, 12.5, 0),
                new Point3d(7.20, 11.9, 0),
                new Point3d(6.00, 11.9, 0),
                new Point3d(6.00, 14.3, 0),

                new Point3d(-6.00, 14.3, 0),
                new Point3d(-6.00, 11.9, 0),
                new Point3d(-7.20, 11.9, 0),
                new Point3d(-7.20, 12.5, 0),
                new Point3d(-28.2, 12.5, 0),
                new Point3d(-28.2, 3.7, 0),
                new Point3d(-27.6, 3.7, 0),
                new Point3d(-27.6, 2.5, 0),
                new Point3d(-30.0, 2.5, 0),

                new Point3d(-30.0, -2.5, 0),
                new Point3d(-27.6, -2.5, 0),
                new Point3d(-27.6, -3.7, 0),
                new Point3d(-28.2, -3.7, 0),
                new Point3d(-28.2, -12.5, 0),
                new Point3d(-7.20, -12.5, 0),
                new Point3d(-7.20, -11.9, 0),
                new Point3d(-6.00, -11.9, 0),
                new Point3d(-6.00, -14.3, 0),

                new Point3d(6.00, -14.3, 0),
                new Point3d(6.00, -11.9, 0),
                new Point3d(7.20, -11.9, 0),
                new Point3d(7.20, -12.5, 0),
                new Point3d(28.2, -12.5, 0),
                new Point3d(28.2, -3.7, 0),
                new Point3d(27.6, -3.7, 0),
                new Point3d(27.6, -2.5, 0),
                new Point3d(30.0, -2.5, 0),

                new Point3d(30.0, 2.5, 0),
            };

            var contour = new PolylineCurve(points);

            return contour;
        }
    }
}