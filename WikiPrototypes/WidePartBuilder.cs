using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public static class WidePartBuilder
    {
        public static Curve[] GetMiddleHoles(double posX, double posY)
        {
            var result = new Curve[]
            {
                ConnectorBuilder.GetRoundCutOff(10, 0.9, posX, posY - 10, 0),
                ConnectorBuilder.GetHCutOff(12, 1.8, posX, posY, 0),
                ConnectorBuilder.GetRoundCutOff(10, 0.9, posX, posY + 10, 0),
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
                ConnectorBuilder.GetSquareNotch(1.8, 7.5, rightPosX, posY - 6.250, 0),
                new Line(rightPosX, posY + 02.500, 0, rightPosX, posY - 02.500, 0).ToNurbsCurve(),
                ConnectorBuilder.GetSquareNotch(1.8, 7.5, rightPosX, posY + 6.250, 0),
            };

            var leftCurves = new Curve[]
            {
                ConnectorBuilder.GetSquareNotch(1.8, 7.5, leftPosX, posY - 6.250, rot180),
                new Line(leftPosX, posY + 02.500, 0, leftPosX, posY - 02.500, 0).ToNurbsCurve(),
                ConnectorBuilder.GetSquareNotch(1.8, 7.5, leftPosX, posY + 6.250, rot180),
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
                new Line(rightPosX, posY - 20.000, 0, rightPosX, posY - 13.230, 0).ToNurbsCurve(),
                ConnectorBuilder.GetParallelConnector(rightPosX, posY - 10.445, rot90),
                new Line(rightPosX, posY - 07.660, 0, rightPosX, posY - 03.750, 0).ToNurbsCurve(),
                ConnectorBuilder.GetSquareNotch(1.8, 7.5, rightPosX, posY, 0),
                new Line(rightPosX, posY + 07.660, 0, rightPosX, posY + 03.750, 0).ToNurbsCurve(),
                ConnectorBuilder.GetParallelConnector(rightPosX, posY + 10.445, rot90),
                new Line(rightPosX, posY + 20.000, 0, rightPosX, posY + 13.230, 0).ToNurbsCurve(),
            };

            var leftCurves = new Curve[]
            {
                new Line(leftPosX, posY - 20.000, 0, leftPosX, posY - 13.230, 0).ToNurbsCurve(),
                ConnectorBuilder.GetParallelConnector(leftPosX, posY - 10.445, -rot90),
                new Line(leftPosX, posY - 07.660, 0, leftPosX, posY - 03.750, 0).ToNurbsCurve(),
                ConnectorBuilder.GetSquareNotch(1.8, 7.5, leftPosX, posY, rot180),
                new Line(leftPosX, posY + 07.660, 0, leftPosX, posY + 03.750, 0).ToNurbsCurve(),
                ConnectorBuilder.GetParallelConnector(leftPosX, posY + 10.445, -rot90),
                new Line(leftPosX, posY + 20.000, 0, leftPosX, posY + 13.230, 0).ToNurbsCurve(),
            };

            var result = new Curve[2]
            {
                Curve.JoinCurves(leftCurves)[0],
                Curve.JoinCurves(rightCurves)[0],
            };

            return result;
        }

        // Cut layer
        public static Curve GetEndConnector(double posX, double posY, double excess, bool hole, double rotation)
        {
            var lineFR = new Line(+26.934 + posX, posY, 0, +28.200 + posX, posY, 0).ToNurbsCurve();
            var lineMR = new Line(+22.275 + posX, posY, 0, +23.540 + posX, posY, 0).ToNurbsCurve();
            var lineNR = new Line(+02.785 + posX, posY, 0, +16.705 + posX, posY, 0).ToNurbsCurve();
            var lineFL = new Line(-26.934 + posX, posY, 0, -28.200 + posX, posY, 0).ToNurbsCurve();
            var lineML = new Line(-22.275 + posX, posY, 0, -23.540 + posX, posY, 0).ToNurbsCurve();
            var lineNL = new Line(-02.785 + posX, posY, 0, -16.705 + posX, posY, 0).ToNurbsCurve();

            var cornerConnectors = GetCornerConnectors(posX, posY, 30, excess, hole, 0);

            var curves = new Curve[]
            {
                cornerConnectors[0],
                lineFL,
                ConnectorBuilder.GetRoundNotch(-25.237 + posX, posY, 0),
                lineML,
                ConnectorBuilder.GetParallelConnector(-19.49 + posX, posY, 0),
                lineNL,
                ConnectorBuilder.GetParallelConnector(posX, posY, 0),
                lineNR,
                ConnectorBuilder.GetParallelConnector(+19.49 + posX, posY, 0),
                lineMR,
                ConnectorBuilder.GetRoundNotch(+25.237 + posX, posY, 0),
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

                var shapeParts = new Curve[] { arcA, arcB, arcC, arcD, lineA, lineB, lineC };

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

        public static Curve[] GetCornerConnectors(double posX, double posY, double xExtends, double excess, bool hole, double rotation)
        {
            var result = new Curve[2];
            var index = 0;

            if (hole)
            {
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
                    new Point3d(x * (-xExtends + 1.8) + posX, posY + 8.8 + excess, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 8.8 + excess, 0),
                    new Point3d(x * (-xExtends + 2.4) + posX, posY + 10 + excess, 0),
                    new Point3d(x * (-xExtends + 0.0) + posX, posY + 10 + excess, 0),
                    };

                    result[index] = new PolylineCurve(cornerPoints);

                    index++;
                }
            }
            else
            {
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
            }

            if (rotation % (Math.PI * 2) == 0)
                return result;

            foreach (var curve in result)
                curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }
    }
}