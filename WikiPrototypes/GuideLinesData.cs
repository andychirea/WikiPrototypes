using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public struct GuideLinesData
    {
        public Line[] GuideLines { get; private set; }
        public Line[] SymmetryLines { get; private set; }
        public double[] Rotations { get; private set; }
        public double[] OffsetsAtStart { get; private set; }
        public double[] OffsetsAtEnd { get; private set; }
        public int[] DeviationSigns { get; private set; }
        public Point3d[] ContourPoints { get; private set; }

        public GuideLinesData(Line[] guideLines)
        {
            var length = guideLines.Length;

            GuideLines = guideLines;
            ContourPoints = new Point3d[length * 2 + 2];
            SymmetryLines = new Line[length];
            Rotations = new double[length];
            OffsetsAtStart = new double[length];
            OffsetsAtEnd = new double[length];
            DeviationSigns = new int[length];

            SetContourPoints(guideLines);
            SetSymmetryData(guideLines);
        }

        private void SetContourPoints(Line[] guideLines)
        {
            var planeNormal = Vector3d.ZAxis;

            for (int i = -1; i < guideLines.Length; i++)
            {
                var guideLine = i == -1 ? guideLines[0] : guideLines[i];
                var nextGuideLine = i == guideLines.Length - 1 ? guideLines[guideLines.Length - 1] : guideLines[i + 1];

                var lineDirection = guideLine.Direction;
                lineDirection.Unitize();
                var nextLineDirection = nextGuideLine.Direction;
                nextLineDirection.Unitize();

                var deviationAngle = (Math.PI - Vector3d.VectorAngle(-lineDirection, nextLineDirection)) * .5;
                var deviationSign = GetDeviationSign(lineDirection, nextLineDirection, planeNormal);
                var cornerLength = 14.3 * Math.Tan(deviationAngle);
                var lineTranslation = lineDirection * cornerLength * deviationSign;

                if (i > -1)
                    DeviationSigns[i] = deviationSign;

                var perpendicularVector = Vector3d.CrossProduct(lineDirection, planeNormal);
                perpendicularVector.Unitize();
                var perpendicularTranslation = perpendicularVector * 14.3;

                var segmentEndPoint = i == -1 ? guideLine.PointAt(0) : guideLine.PointAt(1);
                ContourPoints[i + 1] = segmentEndPoint + perpendicularTranslation + lineTranslation;
                ContourPoints[ContourPoints.Length - 2 - i] = segmentEndPoint - perpendicularTranslation - lineTranslation;
            }
        }

        private void SetSymmetryData(Line[] guideLines)
        {
            var horizontalDirection = Vector3d.XAxis;

            for (int i = 0; i < guideLines.Length; i++)
            {
                // GET LINE DATA
                var guideLine = guideLines[i];
                var startLinePoint = guideLine.PointAt(0);
                var endLinePoint = guideLine.PointAt(1);
                var lineDirection = guideLine.Direction;
                var verticalDirection = GetDeviationSign(horizontalDirection, lineDirection, Vector3d.ZAxis); // 1 for up, -1 for down
                var rotation = Vector3d.VectorAngle(lineDirection, horizontalDirection) * verticalDirection - Math.PI * .5;

                // GET AXIS SEGMENT OF SYMMETRICAL CONNECTIONS
                var unitDirection = lineDirection;
                unitDirection.Unitize();

                var startCornerPoint = ContourPoints[i];
                var startCornerVector = startCornerPoint - startLinePoint;
                var startCornerBisectorAngle = Vector3d.VectorAngle(unitDirection, startCornerVector);
                var startCornerBisectorAngleMin = Math.Min(startCornerBisectorAngle, Math.PI - startCornerBisectorAngle);
                var startAxisSegmentDistanceFromCorner = Math.Cos(startCornerBisectorAngleMin) * startCornerVector.Length;
                var startAxisSegmentPoint = startLinePoint + unitDirection * startAxisSegmentDistanceFromCorner;

                var endCornerPoint = ContourPoints[i + 1];
                var endCornerVector = endCornerPoint - endLinePoint;
                var endCornerBisectorAngle = Vector3d.VectorAngle(-unitDirection, endCornerVector);
                var endCornerBisectorAngleMax = Math.Max(endCornerBisectorAngle, Math.PI - endCornerBisectorAngle);
                var endAxisSegmentDistanceFromCorner = Math.Cos(endCornerBisectorAngleMax) * endCornerVector.Length;
                var endAxisSegmentPoint = endLinePoint + unitDirection * endAxisSegmentDistanceFromCorner;

                SymmetryLines[i] = new Line(startAxisSegmentPoint, endAxisSegmentPoint);
                Rotations[i] = rotation;
                OffsetsAtStart[i] = startAxisSegmentDistanceFromCorner;
                OffsetsAtEnd[i] = endAxisSegmentDistanceFromCorner;
            }
        }

        private int GetDeviationSign(Vector3d currentVector, Vector3d referenceVector, Vector3d planeNormal)
        {
            var guideVector = Vector3d.CrossProduct(currentVector, planeNormal);
            var dotProduct = guideVector.X * referenceVector.X + guideVector.Y * referenceVector.Y + guideVector.Z * referenceVector.Z;
            return Math.Sign(dotProduct) == 0? 1 : -Math.Sign(dotProduct);
        }
    }
}