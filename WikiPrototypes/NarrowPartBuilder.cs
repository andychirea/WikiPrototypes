using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public static class NarrowPartBuilder
    {
        public static Curve GetSplitCurve(double posX, double posY)
        {
            var curves = new Curve[3];

            var arcA = new Arc(new Point3d(posX - 4.208, posY, 0),
                new Point3d(posX - 3.750, posY + 0.212, 0),
                new Point3d(posX - 3.616, posY + 0.699, 0)).ToNurbsCurve();
            var arcB = new Arc(new Point3d(posX + 4.208, posY, 0),
                new Point3d(posX + 3.750, posY + 0.212, 0),
                new Point3d(posX + 3.616, posY + 0.699, 0)).ToNurbsCurve();

            var arcC = new Arc(new Point3d(posX - 5.804, posY + 5.301, 0),
                new Point3d(posX - 5.670, posY + 5.788, 0),
                new Point3d(posX - 5.212, posY + 6.000, 0)).ToNurbsCurve();
            var arcD = new Arc(new Point3d(posX + 5.804, posY + 5.301, 0),
                new Point3d(posX + 5.670, posY + 5.788, 0),
                new Point3d(posX + 5.212, posY + 6.000, 0)).ToNurbsCurve();

            var lineA = new Line(posX - 3.616, posY + 0.699, 0, posX - 5.804, posY + 5.301, 0).ToNurbsCurve();
            var lineB = new Line(posX + 3.616, posY + 0.699, 0, posX + 5.804, posY + 5.301, 0).ToNurbsCurve();
            var lineC = new Line(posX - 5.212, posY + 6.000, 0, posX + 5.212, posY + 6.000, 0).ToNurbsCurve();

            var shapeParts = new Curve[] { arcA, arcB, arcC, arcD, lineA, lineB, lineC };

            curves[0] = Curve.JoinCurves(shapeParts)[0];
            curves[1] = new Line(posX - 11.90, posY, 0, posX - 4.208, posY, 0).ToNurbsCurve();
            curves[2] = new Line(posX + 11.90, posY, 0, posX + 4.208, posY, 0).ToNurbsCurve();

            var result = Curve.JoinCurves(curves);

            return result[0];
        }

        public static Curve GetEndConnector(double posX, double posY, bool rotate)
        {
            var rot90 = Math.PI * .5;

            var rConnector = ConnectorBuilder.GetSquareNotch(1.8, 6, posX + 5.400, posY, -rot90);
            var lConnector = ConnectorBuilder.GetSquareNotch(1.8, 6, posX - 6.600, posY, -rot90);

            var rsLine = new Line(posX + 14.3, posY, 0, posX + 14.3, posY + 10, 0).ToNurbsCurve();
            var lsLine = new Line(posX - 17.5, posY, 0, posX - 17.5, posY + 10, 0).ToNurbsCurve();
            var rfLine = new Line(posX + 8.4, posY, 0, posX + 14.3, posY, 0).ToNurbsCurve();
            var lfLine = new Line(posX - 9.6, posY, 0, posX - 17.5, posY, 0).ToNurbsCurve();
            var mLine = new Line(posX - 3.6, posY, 0, posX + 2.4, posY, 0).ToNurbsCurve();

            var result = Curve.JoinCurves(new Curve[]
            { rConnector, lConnector, rsLine, lsLine, lfLine, rfLine, mLine })[0];

            if (!rotate)
                return result;

            var transform = new Transform(1)
            {
                M00 = -1
            };

            result.Rotate(Math.PI, Vector3d.ZAxis, new Point3d(posX, posY, 0));
            result.Transform(transform);

            return result;
        }

        public static Curve[] GetMiddleHoles(double posX, double posY)
        {
            var rot180 = Math.PI;
            var rot90 = Math.PI * .5;

            var hShape = ConnectorBuilder.GetHCutOff(5, 1.8, posX - 0.004, posY, 0);
            var rShape = ConnectorBuilder.GetRoundCutOff(5, .45, posX + 12.950, posY, rot90);

            var notch = ConnectorBuilder.GetSquareNotch(0.6, 5.0, posX - 13.1, posY, rot180);
            var lineA = new Line(posX - 13.1, posY + 2.5, 0, posX - 13.1, posY + 3.1, 0).ToNurbsCurve();
            var lineB = new Line(posX - 13.1, posY - 2.5, 0, posX - 13.1, posY - 3.1, 0).ToNurbsCurve();
            var lineC = new Line(posX - 13.1, posY + 3.1, 0, posX - 14.3, posY + 3.1, 0).ToNurbsCurve();
            var lineD = new Line(posX - 13.1, posY - 3.1, 0, posX - 14.3, posY - 3.1, 0).ToNurbsCurve();
            var lineE = new Line(posX - 14.3, posY + 3.1, 0, posX - 14.3, posY - 3.1, 0).ToNurbsCurve();

            var otherShape = Curve.JoinCurves(new Curve[] { notch, lineA, lineB, lineC, lineD, lineE })[0];

            var result = new Curve[] 
            { 
                hShape,
                rShape,
                otherShape,
            };

            return result;
        }

        public static Curve[] GetEndHoles(double posX, double posY, bool rotate)
        {
            var rot90 = Math.PI * .5;

            var result = new Curve[2];

            if (!rotate)
            {
                result[0] = ConnectorBuilder.GetRoundCutOff(2.574, 0.45, posX - 12.95, posY + 5.213, rot90);
                result[1] = ConnectorBuilder.GetRoundCutOff(2.574, 0.45, posX + 12.95, posY + 5.213, rot90);
            }
            else
            {
                result[0] = ConnectorBuilder.GetRoundCutOff(2.574, 0.45, posX - 12.95, posY - 5.213, rot90);
                result[1] = ConnectorBuilder.GetRoundCutOff(2.574, 0.45, posX + 12.95, posY - 5.213, rot90);
            }

            return result;
        }

        public static Curve[] GetMiddleConnectorA(double posX, double posY)
        {
            var rot180 = Math.PI;

            var result = new Curve[2];

            var rBotNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX + 14.3, posY - 11.875, 0);
            var rTopNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX + 14.3, posY + 11.875, 0);
            var rVLine = new Line(posX + 14.3, posY + 3.75, 0, posX + 14.3, posY - 3.75, 0).ToNurbsCurve();

            result[0] = Curve.JoinCurves(new Curve[] { rBotNotch, rTopNotch, rVLine })[0];

            var lBotNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX - 14.3, posY - 11.875, rot180);
            var lTopNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX - 14.3, posY + 11.875, rot180);
            var lHTopLine = new Line(posX - 17.5, posY + 20, 0, posX - 14.3, posY + 20, 0).ToNurbsCurve();
            var lHBotLine = new Line(posX - 17.5, posY - 20, 0, posX - 14.3, posY - 20, 0).ToNurbsCurve();
            var lVLine = new Line(posX - 14.3, posY + 3.75, 0, posX - 14.3, posY - 3.75, 0).ToNurbsCurve();

            result[1] = Curve.JoinCurves(new Curve[] { lBotNotch, lTopNotch, lHTopLine, lHBotLine, lVLine })[0];

            return result;
        }

        public static Curve[] GetMiddleConnectorB(double posX, double posY)
        {
            var result = new Curve[2];

            result[0] = new Line(posX - 17.5, posY + 10, 0, posX - 17.5, posY - 10, 0).ToNurbsCurve();
            result[1] = new Line(posX + 14.3, posY + 10, 0, posX + 14.3, posY - 10, 0).ToNurbsCurve();

            return result;
        }

        public static Curve[] GetMiddleConnectorRest(double posX, double posY, double rest, bool hole)
        {
            var rot180 = Math.PI;
            var result = new Curve[2];

            if (hole)
            {
                result[0] = ConnectorBuilder.GetSquareNotch(1.8, rest, posX + 14.3, posY, 0);
                result[1] = ConnectorBuilder.GetSquareNotch(5.0, rest, posX - 17.5, posY, rot180);
            }
            else
            {
                var yExtends = rest * .5;
                result[0] = new Line(posX + 14.3, posY - yExtends, 0, posX + 14.3, posY + yExtends, 0).ToNurbsCurve();
                result[1] = new Line(posX - 17.5, posY - yExtends, 0, posX - 17.5, posY + yExtends, 0).ToNurbsCurve();
            }

            return result;
        }
    }
}