using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public static class PartSectionBuilder
    {
        public static Curve[] GetMiddleHoles(double posX, double posY)
        {
            var result = new Curve[]
            {
                GetRoundCutOff(10, 0.9, posX, posY - 10, 0),
                GetHCutOff(12, 1.8, posX, posY, 0),
                GetRoundCutOff(10, 0.9, posX, posY + 10, 0),
            };

            return result;
        }

        // Cut layer
        public static Curve[] GetMiddleSquareConnector(double posX, double posY)
        {
            var xExtends = 30;
            var rot180 = Math.PI;

            var rightPosX = posX + xExtends;
            var leftPosX = posX - xExtends;

            var rightCurves = new Curve[]
            {
                new Line(rightPosX, posY - 13.385, 0, rightPosX, posY - 10.000, 0).ToNurbsCurve(),
                GetSquareNotch(rightPosX, posY - 6.250, 0),
                new Line(rightPosX, posY + 02.500, 0, rightPosX, posY - 02.500, 0).ToNurbsCurve(),
                GetSquareNotch(rightPosX, posY + 6.250, 0),
                new Line(rightPosX, posY + 13.385, 0, rightPosX, posY + 10.000, 0).ToNurbsCurve(),
            };

            var leftCurves = new Curve[]
            {
                new Line(leftPosX, posY - 13.385, 0, leftPosX, posY - 10.000, 0).ToNurbsCurve(),
                GetSquareNotch(leftPosX, posY - 6.250, rot180),
                new Line(leftPosX, posY + 02.500, 0, leftPosX, posY - 02.500, 0).ToNurbsCurve(),
                GetSquareNotch(leftPosX, posY + 6.250, rot180),
                new Line(leftPosX, posY + 13.385, 0, leftPosX, posY + 10.000, 0).ToNurbsCurve(),
            };

            var result = new Curve[]
            {
                Curve.JoinCurves(leftCurves)[0],
                Curve.JoinCurves(rightCurves)[0],
            };

            return result;
        }

        // Cut layer
        public static Curve[] GetMiddleParallelConnector(double posX, double posY)
        {
            var xExtends = 30;
            var rot180 = Math.PI;
            var rot90 = Math.PI * .5;

            var rightPosX = posX + xExtends;
            var leftPosX = posX - xExtends;

            var rightCurves = new Curve[]
            {
                new Line(rightPosX, posY - 16.615, 0, rightPosX, posY - 13.230, 0).ToNurbsCurve(),
                GetParallelConnector(rightPosX, posY - 10.445, rot90),
                new Line(rightPosX, posY - 07.660, 0, rightPosX, posY - 03.750, 0).ToNurbsCurve(),
                GetSquareNotch(rightPosX, posY, 0),
                new Line(rightPosX, posY + 07.660, 0, rightPosX, posY + 03.750, 0).ToNurbsCurve(),
                GetParallelConnector(rightPosX, posY + 10.445, rot90),
                new Line(rightPosX, posY + 16.615, 0, rightPosX, posY + 13.230, 0).ToNurbsCurve(),
            };

            var leftCurves = new Curve[]
            {
                new Line(leftPosX, posY - 16.615, 0, leftPosX, posY - 13.230, 0).ToNurbsCurve(),
                GetParallelConnector(leftPosX, posY - 10.445, -rot90),
                new Line(leftPosX, posY - 07.660, 0, leftPosX, posY - 03.750, 0).ToNurbsCurve(),
                GetSquareNotch(leftPosX, posY, rot180),
                new Line(leftPosX, posY + 07.660, 0, leftPosX, posY + 03.750, 0).ToNurbsCurve(),
                GetParallelConnector(leftPosX, posY + 10.445, -rot90),
                new Line(leftPosX, posY + 16.615, 0, leftPosX, posY + 13.230, 0).ToNurbsCurve(),
            };

            var result = new Curve[2]
            {
                Curve.JoinCurves(leftCurves)[0],
                Curve.JoinCurves(rightCurves)[0],
            };

            return result;
        }

        // Cut layer
        public static Curve GetEndConnector(double posX, double posY, double excess, double rotation)
        {
            var lineFR = new Line(+26.934 + posX, posY, 0, +28.200 + posX, posY, 0).ToNurbsCurve();
            var lineMR = new Line(+22.275 + posX, posY, 0, +23.540 + posX, posY, 0).ToNurbsCurve();
            var lineNR = new Line(+02.785 + posX, posY, 0, +16.705 + posX, posY, 0).ToNurbsCurve();
            var lineFL = new Line(-26.934 + posX, posY, 0, -28.200 + posX, posY, 0).ToNurbsCurve();
            var lineML = new Line(-22.275 + posX, posY, 0, -23.540 + posX, posY, 0).ToNurbsCurve();
            var lineNL = new Line(-02.785 + posX, posY, 0, -16.705 + posX, posY, 0).ToNurbsCurve();

            var cornerConnectors = GetCornerPerpendicularConnectiors(posX, posY, 30, excess, 0);

            var curves = new Curve[]
            {
                cornerConnectors[0],
                lineFL,
                GetRoundNotch(-25.237 + posX, posY, 0),
                lineML,
                GetParallelConnector(-19.49 + posX, posY, 0),
                lineNL,
                GetParallelConnector(posX, posY, 0),
                lineNR,
                GetParallelConnector(+19.49 + posX, posY, 0),
                lineMR,
                GetRoundNotch(+25.237 + posX, posY, 0),
                lineFR,
                cornerConnectors[1],
            };

            var result = Curve.JoinCurves(curves)[0];

            if (rotation % (Math.PI * 2) == 0)
                return result;

            result.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve GetSplitCurve(double posX, double posY)
        {
            var curves = new Curve[7];

            for (int x = -1; x <= 1; x++)
            {
                var arcA = new Arc(new Point3d(posX - 3.165 + x * 12.670, posY, 0),
                    new Point3d(posX - 2.658 + x * 12.670, posY + 0.280, 0),
                    new Point3d(posX - 2.624 + x * 12.670, posY + 0.858, 0)).ToNurbsCurve();
                var arcB = new Arc(new Point3d(posX + 3.165 + x * 12.670, posY, 0),
                    new Point3d(posX + 2.658 + x * 12.670, posY + 0.280, 0),
                    new Point3d(posX + 2.624 + x * 12.670, posY + 0.858, 0)).ToNurbsCurve();
                var arcC = new Arc(new Point3d(posX - 3.711 + x * 12.670, posY + 3.142, 0),
                    new Point3d(posX - 3.677 + x * 12.670, posY + 3.720, 0),
                    new Point3d(posX - 3.170 + x * 12.670, posY + 4.000, 0)).ToNurbsCurve();
                var arcD = new Arc(new Point3d(posX + 3.711 + x * 12.670, posY + 3.142, 0),
                    new Point3d(posX + 3.677 + x * 12.670, posY + 3.720, 0),
                    new Point3d(posX + 3.170 + x * 12.670, posY + 4.00, 0)).ToNurbsCurve();
                var lineA = new Line(posX - 2.624 + x * 12.670, posY + 0.858, 0, posX - 3.711 + x * 12.670, posY + 3.142, 0).ToNurbsCurve();
                var lineB = new Line(posX + 2.624 + x * 12.670, posY + 0.858, 0, posX + 3.711 + x * 12.670, posY + 3.142, 0).ToNurbsCurve();
                var lineC = new Line(posX - 3.170 + x * 12.670, posY + 4.000, 0, posX + 3.170 + x * 12.670, posY + 4.000, 0).ToNurbsCurve();

                var shapeParts = new Curve[] { arcA, arcB, arcC, arcD, lineA, lineB, lineC};

                var shape = Curve.JoinCurves(shapeParts)[0];

                curves[x + 1] = shape;
            }

            curves[3] = new Line(posX - 28.20, posY, 0, posX - 15.835, posY, 0).ToNurbsCurve();
            curves[4] = new Line(posX + 28.20, posY, 0, posX + 15.835, posY, 0).ToNurbsCurve();
            curves[5] = new Line(posX - 9.505, posY, 0, posX - 3.165, posY, 0).ToNurbsCurve();
            curves[6] = new Line(posX + 9.505, posY, 0, posX + 3.165, posY, 0).ToNurbsCurve();

            var result = Curve.JoinCurves(curves);

            return result[0];
        }

        public static Curve GetSquareNotch(double posX, double posY, double rotation)
        {
            var points = new Point3d[]
            {
                new Point3d(posX - 0.000, posY - 3.750, 0),
                new Point3d(posX - 2.400, posY - 3.750, 0),
                new Point3d(posX - 2.400, posY - 2.550, 0),
                new Point3d(posX - 1.800, posY - 2.550, 0),
                new Point3d(posX - 1.800, posY + 2.550, 0),
                new Point3d(posX - 2.400, posY + 2.550, 0),
                new Point3d(posX - 2.400, posY + 3.750, 0),
                new Point3d(posX - 0.000, posY + 3.750, 0),
            };

            var result = new Polyline(points).ToNurbsCurve();

            if (rotation % (Math.PI * 2) == 0)
                return result;

            result.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve[] GetCornerPerpendicularConnectiors(double posX, double posY, double xExtends, double excess, double rotation)
        {
            var result = new Curve[2];
            var index = 0;

            for (int x = -1; x <= 1; x += 2)
            {
                var cornerPoints = new Point3d[]
                {
                    new Point3d(x * (-xExtends + 1.8) + posX, posY, 0),
                    new Point3d(x * (-xExtends + 1.8) + posX, posY + 2.8, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 2.8, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 4.0, 0),
                    new Point3d(x * (-xExtends + 0.0) + posX, posY + 4.0, 0),
                    new Point3d(x * (-xExtends + 0.0) + posX, posY + 6.5, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 6.5, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 7.7, 0),
                    new Point3d(x * (-xExtends + 1.8) + posX, posY + 7.7, 0),
                    new Point3d(x * (-xExtends + 1.8) + posX, posY + 8.8, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 8.8, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 10, 0),
                    new Point3d(x * (-xExtends + 0.0) + posX, posY + 10, 0),
                    new Point3d(x * (-xExtends + 0.0) + posX, posY + 10 + excess, 0),
                };

                result[index] = new PolylineCurve(cornerPoints);

                index++;
            }

            if (rotation % (Math.PI * 2) == 0)
                return result;

            foreach (var curve in result)
                curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

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