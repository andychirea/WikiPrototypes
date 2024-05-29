using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public static class IrregularNarrowPartBuilder
    {
        public static Curve GetEndConnector(double posX, double posY, double addSpaceR, double addSpaceL, double rotation)
        {
            var rot90 = Math.PI * .5;

            var rConnector = ConnectorBuilder.GetSquareNotch(1.8, 6, posX + 5.400, posY, -rot90);
            var lConnector = ConnectorBuilder.GetSquareNotch(1.8, 6, posX - 6.600, posY, -rot90);

            var rsLine = new Line(posX + 14.3, posY, 0, posX + 14.3, posY + 10 + addSpaceR, 0).ToNurbsCurve();
            var lsLine = new Line(posX - 14.3, posY, 0, posX - 14.3, posY + 10 + addSpaceL, 0).ToNurbsCurve();
            var rfLine = new Line(posX + 8.40, posY, 0, posX + 14.3, posY, 0).ToNurbsCurve();
            var lfLine = new Line(posX - 9.60, posY, 0, posX - 14.3, posY, 0).ToNurbsCurve();
            var mmLine = new Line(posX - 3.60, posY, 0, posX + 2.40, posY, 0).ToNurbsCurve();

            var result = Curve.JoinCurves(new Curve[]
            { rConnector, lConnector, rsLine, lsLine, lfLine, rfLine, mmLine })[0];

            result.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve[] GetMiddleConnectorA(double posX, double posY, double thickness, double rotation)
        {
            var rot180 = Math.PI;

            var result = new Curve[2];

            var rBotNotch = ConnectorBuilder.GetSquareNotch(thickness, 16.25, posX + 14.3, posY - 11.875, 0);
            var rTopNotch = ConnectorBuilder.GetSquareNotch(thickness, 16.25, posX + 14.3, posY + 11.875, 0);
            var rVLine = new Line(posX + 14.3, posY + 3.75, 0, posX + 14.3, posY - 3.75, 0).ToNurbsCurve();

            result[0] = Curve.JoinCurves(new Curve[] { rBotNotch, rTopNotch, rVLine })[0];

            var lBotNotch = ConnectorBuilder.GetSquareNotch(thickness, 16.25, posX - 14.3, posY - 11.875, rot180);
            var lTopNotch = ConnectorBuilder.GetSquareNotch(thickness, 16.25, posX - 14.3, posY + 11.875, rot180);
            var lVLine = new Line(posX - 14.3, posY + 3.75, 0, posX - 14.3, posY - 3.75, 0).ToNurbsCurve();

            result[1] = Curve.JoinCurves(new Curve[] { lBotNotch, lTopNotch, lVLine })[0];

            if (rotation == 0)
                return result;

            foreach (var curve in result)
                curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve[] GetMiddleConnectorB(double posX, double posY, double rotation)
        {
            var result = new Curve[2];

            result[0] = new Line(posX - 14.3, posY + 10, 0, posX - 14.3, posY - 10, 0).ToNurbsCurve();
            result[1] = new Line(posX + 14.3, posY + 10, 0, posX + 14.3, posY - 10, 0).ToNurbsCurve();

            if (rotation == 0)
                return result;

            foreach (var curve in result)
                curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve[] GetMiddleHoles(double posX, double posY, double thickness, double rotation)
        {
            var rot90 = Math.PI * .5;
            var offset = thickness - 1.8;

            var hShape = ConnectorBuilder.GetHCutOff(5, thickness, posX - 0.004, posY, 0);
            var rShape = ConnectorBuilder.GetRoundCutOff(5, thickness / 4, posX + 12.950 - offset / 2, posY, rot90);
            var lShape = ConnectorBuilder.GetRoundCutOff(5, thickness / 4, posX - 12.950 + offset / 2, posY, rot90);

            var result = new Curve[]
            {
                hShape,
                rShape,
                lShape,
            };

            if (rotation == 0)
                return result;

            foreach (var curve in result)
                curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }

        public static Curve[] GetEndHoles(double posX, double posY, double thickness, double rotation, double offset)
        {
            var rot90 = Math.PI * .5;
            var offsetX = thickness - 1.8;

            var result = new Curve[2];

            result[0] = ConnectorBuilder.GetRoundCutOff(2.574, thickness / 4, posX - 12.95 + offsetX / 2, posY + 5.213 + offset, rot90);
            result[1] = ConnectorBuilder.GetRoundCutOff(2.574, thickness / 4, posX + 12.95 - offsetX / 2, posY + 5.213 - offset, rot90);

            if (rotation % (Math.PI * 2) == 0)
                return result;

            foreach (var curve in result)
                curve.Rotate(rotation, Vector3d.ZAxis, new Point3d(posX, posY, 0));

            return result;
        }
    }
}