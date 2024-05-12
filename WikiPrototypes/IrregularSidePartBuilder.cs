using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace WikiPrototypes
{
    public static class IrregularSidePartBuilder
    {
        public static Curve[] GetShapes(Line[] guideLines)
        {
            var output = new List<Curve>();
            var curvesToConnect = new List<Curve>();
            var horizontalDirection = Vector3d.XAxis;
            var contourPoints = GetContourPoints(guideLines);

            for (int i = 0; i < guideLines.Length; i++)
            {
                // GET LINE DATA
                var guideLine = guideLines[i];
                var isFirst = i == 0;
                var isLast = i == (guideLines.Length - 1);
                var startLinePoint = guideLine.PointAt(0);
                var endLinePoint = guideLine.PointAt(1);
                var lineDirection = guideLine.Direction;
                var verticalDirection = GetDeviationSign(horizontalDirection, lineDirection, Vector3d.ZAxis); // 1 for up, -1 for down
                var rotation = Vector3d.VectorAngle(lineDirection, horizontalDirection) * verticalDirection - Math.PI * .5;

                // GET AXIS SEGMENT OF SYMMETRICAL CONNECTIONS
                var startCornerPoint = contourPoints[i];
                var endCornerPoint = contourPoints[i + 1];
                var unitDirection = lineDirection;
                unitDirection.Unitize();

                var startCornerBisectorSegment = startCornerPoint - startLinePoint;
                var startCornerBisectorAngle = Vector3d.VectorAngle(unitDirection, startCornerBisectorSegment);
                var startCornerBisectorAngleMin = Math.Min(startCornerBisectorAngle, Math.PI - startCornerBisectorAngle);
                var startAxisSegmentDistanceFromCorner = Math.Cos(startCornerBisectorAngleMin) * startCornerBisectorSegment.Length;
                var startAxisSegmentPoint = startLinePoint + unitDirection * startAxisSegmentDistanceFromCorner;

                var endCornerBisectorSegment = endCornerPoint - endLinePoint;
                var endCornerBisectorAngle = Vector3d.VectorAngle(-unitDirection, endCornerBisectorSegment);
                var endCornerBisectorAngleMax = Math.Max(endCornerBisectorAngle, Math.PI - endCornerBisectorAngle);
                var endAxisSegmentDistanceFromCorner = Math.Cos(endCornerBisectorAngleMax) * endCornerBisectorSegment.Length;
                var endAxisSegmentPoint = endLinePoint + unitDirection * endAxisSegmentDistanceFromCorner;

                var axisSegment = new Line(startAxisSegmentPoint, endAxisSegmentPoint);

                // GET CONNECTORS COUNT
                var freeSpaceForConnectors = (endAxisSegmentPoint - startAxisSegmentPoint).Length;
                var connectorACount = Math.Floor((freeSpaceForConnectors + 40 / 2 - 10) / 60);
                var connectorBCount = Math.Floor((freeSpaceForConnectors - 40 + 20 / 2 - 10) / 60);
                var restOfSpace = freeSpaceForConnectors - connectorACount * 40 - connectorBCount * 20;
                var connectorsOffset = restOfSpace * .5;

                // BUILD CONNECTORS by types
                for (int mA = 0; mA < connectorACount; mA++)
                {
                    var distanceInCurve = 60 * mA + 40 / 2 + connectorsOffset;
                    var parameterInCurve = distanceInCurve / freeSpaceForConnectors;
                    var pointInLine = axisSegment.PointAt(parameterInCurve);

                    curvesToConnect.AddRange(GetMiddleConnectorA(pointInLine.X, pointInLine.Y, rotation));
                }

                for (int mB = 0; mB < connectorBCount; mB++)
                {
                    var distanceInCurve = 60 * mB + 20 / 2 + 40 + connectorsOffset;
                    var parameterInCurve = distanceInCurve / freeSpaceForConnectors;
                    var pointInLine = axisSegment.PointAt(parameterInCurve);

                    curvesToConnect.AddRange(GetMiddleConnectorB(pointInLine.X, pointInLine.Y, rotation));

                    output.AddRange(GetMiddleHoles(pointInLine.X, pointInLine.Y, rotation));
                }

                // BUILD CAPS & LINES TO CORNER
                var connectorsOffsetDirection = unitDirection * connectorsOffset;
                var maxStartDistFromCorner = unitDirection * startAxisSegmentDistanceFromCorner * 2;
                var maxEndDistFromCorner = unitDirection * endAxisSegmentDistanceFromCorner * 2;

                if (isFirst)
                {
                    // BUILD START CAP
                    var endConnectorCurve = GetEndConnector(startLinePoint.X, startLinePoint.Y, connectorsOffset - 10, connectorsOffset - 10, rotation);
                    curvesToConnect.Add(endConnectorCurve);
                }
                else if (isLast)
                {
                    // BUILD END CAP
                    var endConnectorCurve = GetEndConnector(endLinePoint.X, endLinePoint.Y, connectorsOffset - 10, connectorsOffset - 10, Math.PI + rotation);
                    curvesToConnect.Add(endConnectorCurve);
                }

                if (!isFirst)
                {
                    var prevLineDirection = guideLines[i - 1].Direction;
                    var deviationWithPrevius = GetDeviationSign(lineDirection, prevLineDirection, Vector3d.ZAxis);

                    var startPoint1 = contourPoints[contourPoints.Length - 1 - i];
                    var cornerDir = deviationWithPrevius != 1 ? Vector3d.Zero : maxStartDistFromCorner;
                    var endPoint1 = startPoint1 + connectorsOffsetDirection + cornerDir;
                    var line1 = new Line(startPoint1, endPoint1);
                    
                    curvesToConnect.Add(line1.ToNurbsCurve());

                    var startPoint2 = contourPoints[i];
                    var dev2 = deviationWithPrevius != -1 ? Vector3d.Zero : maxStartDistFromCorner;
                    var endPoint2 = startPoint2 + connectorsOffsetDirection + dev2;
                    var line2 = new Line(startPoint2, endPoint2);
                    
                    curvesToConnect.Add(line2.ToNurbsCurve());
                }

                if (!isLast)
                {
                    var nextLineDirection = guideLines[i + 1].Direction;
                    var deviationWithNext = GetDeviationSign(lineDirection, nextLineDirection, Vector3d.ZAxis);

                    var startPoint3 = contourPoints[i + 1];
                    var cornerDir = deviationWithNext != 1 ? Vector3d.Zero : maxEndDistFromCorner;
                    var endPoint3 = startPoint3 - connectorsOffsetDirection + cornerDir;
                    var line3 = new Line(startPoint3, endPoint3);
                    
                    curvesToConnect.Add(line3.ToNurbsCurve());

                    var startPoint4 = contourPoints[contourPoints.Length - 2 - i];
                    var dev4 = deviationWithNext != -1 ? Vector3d.Zero : maxEndDistFromCorner;
                    var endPoint4 = startPoint4 - connectorsOffsetDirection + dev4;
                    var line4 = new Line(startPoint4, endPoint4);
                    
                    curvesToConnect.Add(line4.ToNurbsCurve());
                }
            }

            var contour = Curve.JoinCurves(curvesToConnect);
            output.AddRange(contour);

            return output.ToArray();
        }

        public static Point3d[] GetContourPoints(Line[] guideLines)
        {
            var points = new Point3d[guideLines.Length * 2 + 2];
            var planeNormal = Vector3d.ZAxis;

            for (int i = -1; i < guideLines.Length; i++)
            {
                var guideLine = i == -1? guideLines[0] : guideLines[i];
                var nextGuideLine = i == guideLines.Length - 1 ? guideLines[guideLines.Length - 1] : guideLines[i + 1];

                var lineDirection = guideLine.Direction;
                lineDirection.Unitize();
                var nextLineDirection = nextGuideLine.Direction;
                nextLineDirection.Unitize();

                var deviationAngle = (Math.PI - Vector3d.VectorAngle(-lineDirection, nextLineDirection)) * .5;
                var deviationLength = 14.3 * Math.Tan(deviationAngle);
                var deviationSign = GetDeviationSign(lineDirection, nextLineDirection, planeNormal);
                var deviationTranslation = lineDirection * deviationLength * deviationSign;

                var normalUnitTranslation = Vector3d.CrossProduct(lineDirection, planeNormal);
                normalUnitTranslation.Unitize();
                var normalTranslation = normalUnitTranslation * 14.3;

                var segmentEndPoint = i == -1 ? guideLine.PointAt(0) : guideLine.PointAt(1);
                points[i + 1] = segmentEndPoint + normalTranslation + deviationTranslation;
                points[points.Length - 2 - i] = segmentEndPoint - normalTranslation - deviationTranslation;
            }

            return points;
        }

        private static int GetDeviationSign(Vector3d currentVector, Vector3d referenceVector, Vector3d planeNormal)
        {
            var guideVector = Vector3d.CrossProduct(currentVector, planeNormal);
            var dotProduct = guideVector.X * referenceVector.X + guideVector.Y * referenceVector.Y + guideVector.Z * referenceVector.Z;
            return -Math.Sign(dotProduct);
        }

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

        public static Curve[] GetMiddleConnectorA(double posX, double posY, double rotation)
        {
            var rot180 = Math.PI;

            var result = new Curve[2];

            var rBotNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX + 14.3, posY - 11.875, 0);
            var rTopNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX + 14.3, posY + 11.875, 0);
            var rVLine = new Line(posX + 14.3, posY + 3.75, 0, posX + 14.3, posY - 3.75, 0).ToNurbsCurve();

            result[0] = Curve.JoinCurves(new Curve[] { rBotNotch, rTopNotch, rVLine })[0];

            var lBotNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX - 14.3, posY - 11.875, rot180);
            var lTopNotch = ConnectorBuilder.GetSquareNotch(1.8, 16.25, posX - 14.3, posY + 11.875, rot180);
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

        public static Curve[] GetMiddleHoles(double posX, double posY, double rotation)
        {
            var rot90 = Math.PI * .5;

            var hShape = ConnectorBuilder.GetHCutOff(5, 1.8, posX - 0.004, posY, 0);
            var rShape = ConnectorBuilder.GetRoundCutOff(5, .45, posX + 12.950, posY, rot90);
            var lShape = ConnectorBuilder.GetRoundCutOff(5, .45, posX - 12.950, posY, rot90);

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
    }
}