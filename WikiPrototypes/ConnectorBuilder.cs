using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public static class ConnectorBuilder
    {
        public static Curve GetParallelConnector(double posX, double posY, double rotation)
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

            var curves = new Curve[]
            {
                arcA.ToNurbsCurve(),
                lineA.ToNurbsCurve(),
                arcB.ToNurbsCurve(),
                lineB.ToNurbsCurve(),
                arcC.ToNurbsCurve(),
                lineC.ToNurbsCurve(),
                arcD.ToNurbsCurve(),
            };

            var result = Curve.JoinCurves(curves)[0];

            if (rotation % (Math.PI * 2) == 0)
                return result;

            result.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve GetSquareNotch(double sizeX, double sizeY, double posX, double posY, double rotation)
        {
            var yExtends = sizeY * .5;

            // sizeX + 0.6
            // sizeY + 1.2

            var points = new Point3d[]
            {
                new Point3d(posX - 0.000, posY - yExtends, 0),
                new Point3d(posX - (sizeX + .6), posY - yExtends, 0),
                new Point3d(posX - (sizeX + .6), posY - (yExtends - 1.2), 0),
                new Point3d(posX - sizeX, posY - (yExtends - 1.2), 0),
                new Point3d(posX - sizeX, posY + (yExtends - 1.2), 0),
                new Point3d(posX - (sizeX + .6), posY + (yExtends - 1.2), 0),
                new Point3d(posX - (sizeX + .6), posY + yExtends, 0),
                new Point3d(posX - 0.000, posY + yExtends, 0),
            };

            var result = new Polyline(points).ToNurbsCurve();

            if (rotation % (Math.PI * 2) == 0)
                return result;

            result.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve GetRoundNotch(double posX, double posY, double rotation)
        {
            var pointA = new Point3d(-1.697 + posX, 0 + posY, 0);
            var pointB = new Point3d(-1.351 + posX, 0.11 + posY, 0);
            var pointC = new Point3d(-1.131 + posX, 0.4 + posY, 0);
            var pointD = new Point3d(0 + posX, 1.2 + posY, 0);
            var pointE = new Point3d(+1.131 + posX, 0.4 + posY, 0);
            var pointF = new Point3d(+1.351 + posX, 0.11 + posY, 0);
            var pointG = new Point3d(+1.697 + posX, 0 + posY, 0);

            var curves = new Curve[]
            {
                new Arc(pointA, pointB, pointC).ToNurbsCurve(),
                new Arc(pointC, pointD, pointE).ToNurbsCurve(),
                new Arc(pointE, pointF, pointG).ToNurbsCurve(),
            };

            var result = Curve.JoinCurves(curves)[0];

            if (rotation % (Math.PI * 2) == 0)
                return result;

            result.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static PolylineCurve GetHCutOff(double x, double y, double posX, double posY, double rotation)
        {
            var center = new Point3d(posX, posY, 0);

            var extendX = x * .5;
            var extendY = y * .5;

            var result = new Point3d[]
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

        public static Curve GetRoundCutOff(double length, double radius, double posX, double posY, double rotation)
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

            var curves = new Curve[]
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
    }
}