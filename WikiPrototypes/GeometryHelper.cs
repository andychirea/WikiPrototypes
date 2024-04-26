using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    internal static class GeometryHelper
    {
        public static Curve[] GetEndCutOuts(double length)
        {
            var rot180 = Math.PI;
            var rot90 = Math.PI * .5;

            var endCutOuts = new Curve[18]
            {
                GetOCutOff(0, 0, 0),
                GetOCutOff(-19.4, 0, 0),
                GetOCutOff(+19.4, 0, 0),
                GetOCutOff(0, length, rot180),
                GetOCutOff(-19.4, length, rot180),
                GetOCutOff(+19.4, length, rot180),
                GetHCutOff(5.2, 3.6, -30, 1.40, rot90),
                GetHCutOff(3.5, 3.6, -30, 8.25, rot90),
                GetHCutOff(5.2, 3.6, +30, 1.40, rot90),
                GetHCutOff(3.5, 3.6, +30, 8.25, rot90),
                GetHCutOff(5.2, 3.6, -30, length - 1.40, rot90),
                GetHCutOff(3.5, 3.6, -30, length - 8.25, rot90),
                GetHCutOff(5.2, 3.6, +30, length - 1.40, rot90),
                GetHCutOff(3.5, 3.6, +30, length - 8.25, rot90),
                GetRoundHeadCutOff(-25.238, 0, 0),
                GetRoundHeadCutOff(+25.238, 0, 0),
                GetRoundHeadCutOff(-25.238, length, rot180),
                GetRoundHeadCutOff(+25.238, length, rot180),
            };

            return endCutOuts;
        }

        public static NurbsCurve[] GetConnectorModule(double posX, double posY)
        {
            var rot90 = Math.PI * .5;

            var curves = new NurbsCurve[5]
            {
                GetHCutOff(16.25, 3.6,  +15.9 + posX, 18.125 + posY, rot90).ToNurbsCurve(),
                GetHCutOff(16.25, 3.6,  +15.9 + posX, 41.875 + posY, rot90).ToNurbsCurve(),
                GetHCutOff(16.25, 10,   -15.9 + posX, 18.125 + posY, rot90).ToNurbsCurve(),
                GetHCutOff(16.25, 10,   -15.9 + posX, 41.875 + posY, rot90).ToNurbsCurve(),
                GetSCutOff(3.2, 7.5, -14.3 + posX, 30 + posY).ToNurbsCurve(),
            };

            return curves;
        }

        public static Curve[] GetConnectorModule1(double posX, double posY)
        {
            var rot90 = Math.PI * .5;

            var curves = new Curve[6]
            {
                GetHCutOff(7.5, 3.6,  -30 + posX, 30 + posY, rot90).ToNurbsCurve(),
                GetHCutOff(7.5, 3.6,  +30 + posX, 30 + posY, rot90).ToNurbsCurve(),
                GetOCutOff(-30 + posX, 40.445 + posY, -rot90),
                GetOCutOff(+30 + posX, 40.445 + posY, +rot90),
                GetOCutOff(-30 + posX, 19.555 + posY, -rot90),
                GetOCutOff(+30 + posX, 19.555 + posY, +rot90),
            };

            return curves;
        }

        public static NurbsCurve[] GetConnectorModule2(double posX, double posY)
        {
            var rot90 = Math.PI * .5;

            var curves = new NurbsCurve[4]
            {
                GetHCutOff(7.5, 3.6,  -30 + posX, 53.750 + posY, rot90).ToNurbsCurve(),
                GetHCutOff(7.5, 3.6,  +30 + posX, 53.750 + posY, rot90).ToNurbsCurve(),
                GetHCutOff(7.5, 3.6,  -30 + posX, 66.250 + posY, rot90).ToNurbsCurve(),
                GetHCutOff(7.5, 3.6,  +30 + posX, 66.250 + posY, rot90).ToNurbsCurve(),
            };

            return curves;
        }

        public static NurbsCurve[] GetMiddleCutOuts(double posX, double posY)
        {
            var curves = new NurbsCurve[3]
            {
                GetHCutOff(12, 1.8, posX, 60 + posY, 0).ToNurbsCurve(),
                GetRCutOff(10, 0.9, posX, 50 + posY, 0).ToNurbsCurve(),
                GetRCutOff(10, 0.9, posX, 70 + posY, 0).ToNurbsCurve(),
            };

            return curves;
        }

        public static Rectangle3d GetSCutOff(double x, double y, double posX, double posY)
        {
            var xExtends = x * .5;
            var yExtends = y * .5;

            var cornerA = new Point3d(-xExtends + posX, -yExtends + posY, 0);
            var cornerB = new Point3d(+xExtends + posX, +yExtends + posY, 0);

            var rectangle = new Rectangle3d(Plane.WorldXY, cornerA, cornerB);

            return rectangle;
        }

        public static PolylineCurve GetHCutOff(double x, double y, double posX, double posY, double rotation)
        {
            var center = new Point3d(posX, posY, 0);

            var extendX = x * .5;
            var extendY = y * .5;

            var result = new Point3d[13]
            {
                new Point3d(-extendX + 1.2 + posX,     -extendY + posY,        0),
                new Point3d(-extendX + 1.2 + posX,     -extendY - .6 + posY,   0),
                new Point3d(-extendX + posX,           -extendY - .6 + posY,   0),
                new Point3d(-extendX + posX,           +extendY + .6 + posY,   0),
                new Point3d(-extendX + 1.2 + posX,     +extendY + .6 + posY,   0),
                new Point3d(-extendX + 1.2 + posX,     +extendY + posY,        0),
                new Point3d(extendX - 1.2 + posX,      +extendY + posY,        0),
                new Point3d(extendX - 1.2 + posX,      +extendY + .6 + posY,   0),
                new Point3d(extendX + posX,            +extendY + .6 + posY,   0),
                new Point3d(extendX + posX,            -extendY - .6 + posY,   0),
                new Point3d(extendX - 1.2 + posX,      -extendY - .6 + posY,   0),
                new Point3d(extendX - 1.2 + posX,      -extendY + posY,        0),
                new Point3d(-extendX + 1.2 + posX,     -extendY + posY,        0),
            };

            var polyline = new PolylineCurve(result);
            polyline.Rotate(rotation, Vector3d.ZAxis, center);

            return polyline;
        }

        public static Curve GetRCutOff(double length, double radius, double posX, double posY, double rotation)
        {
            var extends = length * .5;

            var pointA = new Point3d(-extends + posX, +radius + posY, 0);
            var pointB = new Point3d(-extends + posX, -radius + posY, 0);
            var pointC = new Point3d(+extends + posX, +radius + posY, 0);
            var pointD = new Point3d(+extends + posX, -radius + posY, 0);

            var lineA = new Line(pointA, pointC);
            var lineB = new Line(pointB, pointD);

            var arcA = new Arc(pointA, new Point3d(-extends - radius + posX, posY, 0), pointB);
            var arcB = new Arc(pointC, new Point3d(+extends + radius + posX, posY, 0), pointD);

            var curves = new Curve[4]
            {
                lineA.ToNurbsCurve(),
                arcA.ToNurbsCurve(),
                lineB.ToNurbsCurve(),
                arcB.ToNurbsCurve(),
            };

            var joinnedCurves = Curve.JoinCurves(curves);

            if (joinnedCurves.Length == 0)
                return null;

            var curve = joinnedCurves[0];
            curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return curve;
        }

        public static Curve GetOCutOff(double posX, double posY, double rotation)
        {
            var arcApointA = new Point3d(-2.785 + posX, 0.000 + posY, 0);
            var arcApointB = new Point3d(-2.791 + posX, 0.087 + posY, 0);
            var arcApointC = new Point3d(-2.810 + posX, 0.172 + posY, 0);
            var arcA = new Arc(arcApointA, arcApointB, arcApointC);

            var arcBpointA = new Point3d(-4.500 + posX, 5.000 + posY, 0);
            var arcBpointB = new Point3d(-4.324 + posX, 5.424 + posY, 0);
            var arcBpointC = new Point3d(-3.900 + posX, 5.600 + posY, 0);
            var arcB = new Arc(arcBpointA, arcBpointB, arcBpointC);

            var arcCpointA = new Point3d(2.785 + posX, 0.000 + posY, 0);
            var arcCpointB = new Point3d(2.791 + posX, 0.087 + posY, 0);
            var arcCpointC = new Point3d(2.810 + posX, 0.172 + posY, 0);
            var arcC = new Arc(arcCpointA, arcCpointB, arcCpointC);

            var arcDpointA = new Point3d(4.500 + posX, 5.000 + posY, 0);
            var arcDpointB = new Point3d(4.324 + posX, 5.424 + posY, 0);
            var arcDpointC = new Point3d(3.900 + posX, 5.600 + posY, 0);
            var arcD = new Arc(arcDpointA, arcDpointB, arcDpointC);

            var lineA = new Line(arcApointC, arcBpointA);
            var lineB = new Line(arcBpointC, arcDpointC);
            var lineC = new Line(arcDpointA, arcCpointC);
            var lineD = new Line(arcApointA, arcCpointA);

            var curves = new Curve[8]
            {
                arcA.ToNurbsCurve(),
                lineA.ToNurbsCurve(),
                arcB.ToNurbsCurve(),
                lineB.ToNurbsCurve(),
                arcC.ToNurbsCurve(),
                lineC.ToNurbsCurve(),
                arcD.ToNurbsCurve(),
                lineD.ToNurbsCurve(),
            };

            if (rotation % (Math.PI * 2) != 0)
            {
                foreach (var curve in curves)
                {
                    curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));
                }
            }

            return Curve.JoinCurves(curves)[0];
        }

        public static Curve GetRoundHeadCutOff(double posX, double posY, double rotation)
        {
            var pointA = new Point3d(-1.697 + posX, 0 + posY, 0);
            var pointB = new Point3d(-1.351 + posX, 0.11 + posY, 0);
            var pointC = new Point3d(-1.131 + posX, 0.4 + posY, 0);
            var pointD = new Point3d(0 + posX, 1.2 + posY, 0);
            var pointE = new Point3d(+1.131 + posX, 0.4 + posY, 0);
            var pointF = new Point3d(+1.351 + posX, 0.11 + posY, 0);
            var pointG = new Point3d(+1.697 + posX, 0 + posY, 0);

            var curves = new Curve[4]
            {
                new Arc(pointA, pointB, pointC).ToNurbsCurve(),
                new Arc(pointC, pointD, pointE).ToNurbsCurve(),
                new Arc(pointE, pointF, pointG).ToNurbsCurve(),
                new Line(pointG, pointA).ToNurbsCurve(),
            };

            if (rotation % (Math.PI * 2) != 0)
            {
                foreach (var curve in curves)
                {
                    curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));
                }
            }

            return Curve.JoinCurves(curves)[0];
        }
    }
}