using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;

namespace WikiPrototypes
{
    public class IrregularWallBlueprint
    {
        public Line[] GuideLines { get; private set; }
        public DataTree<Curve> OutsideCuts { get; private set; }
        public DataTree<Curve> InsideCuts { get; private set; }
        public DataTree<Curve> HalfMills { get; private set; }

        public List<int> NarrowPartBrachIndexes { get; private set; }
        public List<int> WidePartBrachIndexes { get; private set; }
        public List<int> TransversalPartBrachIndexes { get; private set; }

        public List<int> WidePartGuideLineIndex { get; private set; }

        public Transform GuideLineTransform { get; private set; }
        public Plane GuideLinePlane { get; private set; }
        public GuideLinesData GuideLineData { get; private set; }
        public List<Point3d> WidePartPositions { get; private set; }

        public IrregularWallBlueprint(Curve curve, double maxPartLength, double maxConerLength, double thickness, double millingDiameter)
        {
            OutsideCuts = new DataTree<Curve>();
            InsideCuts = new DataTree<Curve>();
            HalfMills = new DataTree<Curve>();

            NarrowPartBrachIndexes = new List<int>();
            WidePartBrachIndexes = new List<int>();
            TransversalPartBrachIndexes = new List<int>();

            WidePartGuideLineIndex = new List<int>();
            WidePartPositions = new List<Point3d>();

            GuideLines = GetGuideLines(curve, maxPartLength);

            if (GuideLines == null || GuideLines.Length <= 1)
                return;

            GuideLineData = new GuideLinesData(GuideLines);

            SolveNarrowPart(GuideLineData, maxPartLength, maxConerLength, thickness, 0, out var narrowPartCount);
            SolveWidePart(GuideLineData, maxPartLength, thickness, narrowPartCount, out var widePartCount);
        }

        private Line[] GetGuideLines(Curve curve, double maxPartLength)
        {
            if (!curve.IsPlanar())
                return null;

            if (curve.TryGetPlane(out var plane))
            {
                var normal = plane.Normal;

                if (normal.Z < 0)
                    plane.Flip();

                normal = plane.Normal;
                GuideLinePlane = plane;

                if (normal != Vector3d.ZAxis)
                {
                    GuideLineTransform = Transform.PlaneToPlane(plane, Plane.WorldXY);
                    curve.Transform(GuideLineTransform);
                }
                else
                {
                    GuideLineTransform = Transform.Identity;
                    GuideLinePlane = new Plane(curve.PointAtStart, Vector3d.XAxis, Vector3d.YAxis);
                }
            }

            Curve[] segments;
            if (!curve.IsPolyline())
            {
                var activeDoc = RhinoDoc.ActiveDoc;
                var tolerance = activeDoc.ModelAbsoluteTolerance;
                var angleTolerance = activeDoc.ModelAngleToleranceRadians;
                var polylineCurve = curve.ToPolyline(tolerance, angleTolerance, 60, maxPartLength).Simplify(CurveSimplifyOptions.All, tolerance, angleTolerance);
                segments = polylineCurve.DuplicateSegments();
            }
            else
            {
                segments = curve.DuplicateSegments();
            }

            if (segments.Length == 0)
                return null;

            var guideLines = new Line[segments.Length];

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                guideLines[i] = new Line(segment.PointAtStart, segment.PointAtEnd);
            }

            return guideLines;
        }

        private void SolveWidePart(GuideLinesData guideLinesData, double maxPartLength, double thickness, int index, out int partCount)
        {
            var guideLines = guideLinesData.GuideLines;
            var contourPoints = guideLinesData.ContourPoints;
            var symmetryLines = guideLinesData.SymmetryLines;

            var posX = 0;

            partCount = 0;

            var contourLines = new Line[guideLines.Length * 2];

            for (int i = 0; i < guideLines.Length; i++)
            {
                var pointA = contourPoints[i];
                var pointB = contourPoints[i + 1];
                contourLines[i] = new Line(pointA, pointB);

                var pointC = contourPoints[contourPoints.Length - 1 - i];
                var pointD = contourPoints[contourPoints.Length - 2 - i];
                contourLines[guideLines.Length + i] = new Line(pointC, pointD);
            }

            for (int i = 0; i < contourLines.Length; i++)
            {
                posX = 70 * i;

                var splitCurves = new List<Curve>();
                var holes = new List<Curve>();
                var mills = new List<Curve>();
                var curvesToConnect = new List<Curve>();

                var contourLine = contourLines[i];
                var symmetryLine = symmetryLines[i % symmetryLines.Length];
                var startPointContour = contourLine.PointAt(0);
                var startPointSymmetry = symmetryLine.PointAt(0);
                var contourLength = contourLine.Length;
                var symmetryLength = symmetryLine.Length;
                var direction = contourLine.Direction / contourLine.Length;

                var contourLineStart = direction.X * startPointContour.X + direction.Y * startPointContour.Y;
                var symmetryLineStart = direction.X * startPointSymmetry.X + direction.Y * startPointSymmetry.Y;
                var startOffset = Math.Abs(contourLineStart - symmetryLineStart);
                var endOffset = contourLength - startOffset - symmetryLength;

                var moduleACount = Math.Floor((symmetryLength - 10 - 10 + 40 / 2) / 60);
                var moduleBCount = Math.Floor((symmetryLength - 10 - 10 - 40 + 20 / 2) / 60);
                if (moduleBCount >= moduleACount && moduleBCount > 0)
                    moduleBCount--;
                var excess = (symmetryLength - 10 - 10 - moduleACount * 40 - moduleBCount * 20) * .5;

                var botEnd = WidePartBuilder.GetStraightEndConnector(posX, 0, startOffset + excess, thickness, true, 0);
                var upEnd = WidePartBuilder.GetStraightEndConnector(posX, contourLength, endOffset + excess, thickness, moduleACount > moduleBCount, Math.PI);

                mills.AddRange(WidePartBuilder.GetStraightEndMill(posX, 0, thickness, 0));
                mills.AddRange(WidePartBuilder.GetStraightEndMill(posX, contourLength, thickness, Math.PI));

                curvesToConnect.Add(botEnd);
                curvesToConnect.Add(upEnd);

                for (int mA = 0; mA < moduleACount; mA++)
                {
                    var posY = 60 * mA + 40 / 2 + 10 + excess + startOffset;
                    curvesToConnect.AddRange(WidePartBuilder.GetMiddleParallelConnector(posX, posY, thickness));

                    mills.AddRange(WidePartBuilder.GetMiddleParallelMill(posX, posY));
                }

                for (int mB = 0; mB < moduleBCount; mB++)
                {
                    var posY = 60 * mB + 20 / 2 + 50 + excess + startOffset;
                    curvesToConnect.AddRange(WidePartBuilder.GetMiddleSquareConnector(posX, posY, thickness));
                    holes.AddRange(WidePartBuilder.GetMiddleHoles(posX, posY, thickness));

                    mills.AddRange(WidePartBuilder.GetMiddleSquareMill(posX, posY, thickness));
                }

                if (maxPartLength < contourLength)
                {
                    var splitSeparation = Math.Floor(maxPartLength / 60) * 60;
                    var startY = splitSeparation - 30;
                    var completeModulesCount = Math.Floor((symmetryLength - excess * 2 - startY) / splitSeparation);

                    for (int j = 0; j <= completeModulesCount; j++)
                    {
                        splitCurves.Add(WidePartBuilder.GetSplitCurve(posX, startOffset + excess + startY + splitSeparation * j, thickness));
                    }
                }

                var contour = Curve.JoinCurves(curvesToConnect)[0];

                SplitPart(contour, splitCurves, holes, mills, index + partCount, WidePartBrachIndexes, out var thisPartCount);
                partCount += thisPartCount;

                var guideLineCount = guideLines.Length;
                var guideIndex = i < guideLineCount? (i % guideLineCount) + 1 : -(i % guideLineCount) - 1;
                var pos = new Point3d(posX, 0, 0);
                for (int w = 0; w < thisPartCount; w++)
                {
                    WidePartGuideLineIndex.Add(guideIndex);
                    WidePartPositions.Add(pos);
                }
            }
        }

        private void SolveNarrowPart(GuideLinesData guideLineData, double maxStraightLength, double maxCornerLength, double thickness, int index, out int partCount)
        {
            var holes = new List<Curve>();
            var splitCurves = new List<Curve>();
            var halfMills = new List<Curve>();

            var curvesToConnect = new List<Curve>();

            var guideLines = guideLineData.GuideLines;
            var contourPoints = guideLineData.ContourPoints;
            var offsetAtStart = guideLineData.OffsetsAtStart;
            var offsetAtEnd = guideLineData.OffsetsAtEnd;
            var symmetryLines = guideLineData.SymmetryLines;
            var rotations = guideLineData.Rotations;

            for (int i = 0; i < guideLines.Length; i++)
            {
                // GET LINE DATA
                var guideLine = guideLines[i];
                var isFirst = i == 0;
                var isLast = i == (guideLines.Length - 1);
                var startLinePoint = guideLine.PointAt(0);
                var endLinePoint = guideLine.PointAt(1);
                var lineDirection = guideLine.Direction;
                var rotation = rotations[i];

                // GET AXIS SEGMENT OF SYMMETRICAL CONNECTIONS
                var unitDirection = lineDirection;
                unitDirection.Unitize();

                var symmetryLine = symmetryLines[i];
                var distanceFromStartCorner = offsetAtStart[i];
                var distanceFromEndCornder = offsetAtEnd[i];

                // GET CONNECTORS COUNT
                var freeSpaceForConnectors = symmetryLine.Length;
                var connectorACount = Math.Floor((freeSpaceForConnectors - 10 - 10 + 40 / 2) / 60);
                var connectorBCount = Math.Floor((freeSpaceForConnectors - 10 - 10 - 40 + 20 / 2) / 60);
                if (connectorBCount >= connectorACount && connectorBCount > 0)
                    connectorBCount--;
                var restOfSpace = freeSpaceForConnectors - connectorACount * 40 - connectorBCount * 20;
                var connectorsOffset = restOfSpace * .5;

                var posibleSplitParameter = new List<double>();

                // BUILD CONNECTORS by types
                for (int mA = 0; mA < connectorACount; mA++)
                {
                    var distanceInCurve = 60 * mA + 40 / 2 + connectorsOffset;
                    var parameterInCurve = distanceInCurve / freeSpaceForConnectors;
                    var pointInLine = symmetryLine.PointAt(parameterInCurve);

                    curvesToConnect.AddRange(IrregularNarrowPartBuilder.GetMiddleConnectorA(pointInLine.X, pointInLine.Y, thickness, rotation));

                    posibleSplitParameter.Add(parameterInCurve);
                }

                for (int mB = 0; mB < connectorBCount; mB++)
                {
                    var distanceInCurve = 60 * mB + 20 / 2 + 40 + connectorsOffset;
                    var parameterInCurve = distanceInCurve / freeSpaceForConnectors;
                    var pointInLine = symmetryLine.PointAt(parameterInCurve);

                    curvesToConnect.AddRange(IrregularNarrowPartBuilder.GetMiddleConnectorB(pointInLine.X, pointInLine.Y, rotation));

                    holes.AddRange(IrregularNarrowPartBuilder.GetMiddleHoles(pointInLine.X, pointInLine.Y, thickness, rotation));
                }

                // SET SPLIT CURVES IN THE PREVIUS MODULES
                var axisLength = symmetryLine.Length;
                var parameterOffset = 3.750 / axisLength;

                for (int n = 0; n < posibleSplitParameter.Count; n++)
                {
                    var nextParameter = posibleSplitParameter[n];
                    var distanceToNextParameter = nextParameter * axisLength;

                    if ((distanceToNextParameter + distanceFromStartCorner + connectorsOffset + 10) >= maxCornerLength)
                    {
                        var pointToPlaceSplitCurve = symmetryLine.PointAt(nextParameter - parameterOffset);
                        var spliCurve = NarrowPartBuilder.GetSplitCurve(pointToPlaceSplitCurve.X, pointToPlaceSplitCurve.Y, thickness, rotation);

                        splitCurves.Add(spliCurve.ToNurbsCurve());

                        posibleSplitParameter.RemoveRange(0, n);

                        break;
                    }
                }

                var turnsFromEnd = 0;
                for (int n = posibleSplitParameter.Count - 1; n > 0; n--)
                {
                    turnsFromEnd++;
                    var nextParameter = posibleSplitParameter[n];
                    var distanceToNextParameter = (1 - nextParameter) * axisLength;

                    if ((distanceToNextParameter + distanceFromEndCornder + connectorsOffset + 10) >= maxCornerLength)
                    {
                        var pointToPlaceSplitCurve = symmetryLine.PointAt(nextParameter - parameterOffset);
                        var spliCurve = NarrowPartBuilder.GetSplitCurve(pointToPlaceSplitCurve.X, pointToPlaceSplitCurve.Y, thickness, rotation);

                        splitCurves.Add(spliCurve.ToNurbsCurve());

                        for (int a = 0; a < turnsFromEnd; a++)
                            posibleSplitParameter.RemoveAt(n);

                        break;
                    }
                }

                for (int s = 0; s < posibleSplitParameter.Count; s++)
                {
                    var parameter = posibleSplitParameter[s];

                    for (int n = s; n < (posibleSplitParameter.Count - 2); n++)
                    {
                        var nextParameter = posibleSplitParameter[n + 1];
                        var distanceToNextParameter = (nextParameter - parameter) * axisLength;

                        if ((distanceToNextParameter + 60) >= maxStraightLength)
                        {
                            var pointToPlaceSplitCurve = symmetryLine.PointAt(nextParameter - parameterOffset);
                            var spliCurve = NarrowPartBuilder.GetSplitCurve(pointToPlaceSplitCurve.X, pointToPlaceSplitCurve.Y, thickness, rotation);

                            splitCurves.Add(spliCurve.ToNurbsCurve());

                            s = n;
                            break;
                        }
                    }
                }

                // BUILD CAPS & LINES TO CORNER
                var connectorsOffsetDirection = unitDirection * connectorsOffset;
                var maxStartDistFromCorner = unitDirection * distanceFromStartCorner * 2;
                var maxEndDistFromCorner = unitDirection * distanceFromEndCornder * 2;

                if (isFirst)
                {
                    // BUILD START CAP
                    var endConnectorCurve = IrregularNarrowPartBuilder.GetEndConnector(startLinePoint.X, startLinePoint.Y, connectorsOffset - 10, connectorsOffset - 10, rotation);
                    curvesToConnect.Add(endConnectorCurve);

                    var endConnectorHoles = NarrowPartBuilder.GetEndHoles(startLinePoint.X, startLinePoint.Y, thickness, rotation);
                    holes.AddRange(endConnectorHoles);
                }
                else if (isLast)
                {
                    // BUILD END CAP
                    var endConnectorCurve = IrregularNarrowPartBuilder.GetEndConnector(endLinePoint.X, endLinePoint.Y, connectorsOffset - 10, connectorsOffset - 10, Math.PI + rotation);
                    curvesToConnect.Add(endConnectorCurve);

                    var endConnectorHoles = NarrowPartBuilder.GetEndHoles(endLinePoint.X, endLinePoint.Y, thickness, Math.PI + rotation);
                    holes.AddRange(endConnectorHoles);
                }

                if (!isFirst)
                {
                    var prevLineDirection = guideLines[i - 1].Direction;
                    var deviationWithPrevius = GetDeviationSign(lineDirection, prevLineDirection, Vector3d.ZAxis);

                    var startPoint1 = contourPoints[contourPoints.Length - 1 - i];
                    var cornerDir1 = deviationWithPrevius != 1 ? Vector3d.Zero : maxStartDistFromCorner;
                    var endPoint1 = startPoint1 + connectorsOffsetDirection + cornerDir1;
                    var line1 = new Line(startPoint1, endPoint1);

                    curvesToConnect.Add(line1.ToNurbsCurve());

                    var startPoint2 = contourPoints[i];
                    var cornerDir2 = deviationWithPrevius != -1 ? Vector3d.Zero : maxStartDistFromCorner;
                    var endPoint2 = startPoint2 + connectorsOffsetDirection + cornerDir2;
                    var line2 = new Line(startPoint2, endPoint2);

                    curvesToConnect.Add(line2.ToNurbsCurve());

                    var middlePoint = (startPoint1 + startPoint2) / 2;
                    var endHoles = IrregularNarrowPartBuilder.GetEndHoles(middlePoint.X, middlePoint.Y, thickness, rotation, -deviationWithPrevius * maxStartDistFromCorner.Length * .5);
                    holes.AddRange(endHoles);
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

                    var middlePoint = (startPoint3 + startPoint4) / 2;
                    var endHoles = IrregularNarrowPartBuilder.GetEndHoles(middlePoint.X, middlePoint.Y, thickness, rotation + Math.PI, -deviationWithNext * maxEndDistFromCorner.Length * .5);
                    holes.AddRange(endHoles);
                }
            }

            var contour = Curve.JoinCurves(curvesToConnect)[0];

            SplitPart(contour, splitCurves, holes, halfMills, index, NarrowPartBrachIndexes, out partCount);
        }

        private int GetDeviationSign(Vector3d currentVector, Vector3d referenceVector, Vector3d planeNormal)
        {
            var guideVector = Vector3d.CrossProduct(currentVector, planeNormal);
            var dotProduct = guideVector.X * referenceVector.X + guideVector.Y * referenceVector.Y + guideVector.Z * referenceVector.Z;
            return -Math.Sign(dotProduct);
        }

        private void SplitPart(Curve contour, List<Curve> splitCurves, List<Curve> holes, List<Curve> mills, int index, List<int> indexes, out int partCount)
        {
            if (splitCurves.Count == 0)
            {
                var path = new GH_Path(index);

                OutsideCuts.Add(contour, path);
                InsideCuts.AddRange(holes, path);
                HalfMills.AddRange(mills);

                indexes.Add(index);

                partCount = 1;

                return;
            }

            var splitParts = new List<Curve>();
            var splitCurveCandidateA = contour;
            var splitCurveCandidateB = contour;

            for (var i = 0; i < splitCurves.Count; i++)
            {
                var splitterCurve = splitCurves[i];

                var intersectionsA = Intersection.CurveCurve(splitCurveCandidateA, splitterCurve, .001, .001);

                CurveIntersections selectedIntersections;
                Curve selectedSplitCurve;

                if (intersectionsA.Count > 1)
                {
                    selectedIntersections = intersectionsA;
                    selectedSplitCurve = splitCurveCandidateA;

                    if (i > 0)
                        splitParts.Add(splitCurveCandidateB);
                }
                else
                {
                    var intersectionsB = Intersection.CurveCurve(splitCurveCandidateB, splitterCurve, .001, .001);

                    if (intersectionsB.Count == 0)
                        continue;

                    selectedIntersections = intersectionsB;
                    selectedSplitCurve = splitCurveCandidateB;

                    if (i > 0)
                        splitParts.Add(splitCurveCandidateA);
                }

                var splitParameters = new List<double>();
                foreach (var intersection in selectedIntersections)
                {
                    if (!intersection.IsPoint)
                        continue;

                    splitParameters.Add(intersection.ParameterA);
                }

                var curveParts = selectedSplitCurve.Split(splitParameters);

                var curvesPart1 = new Curve[2]
                {
                    curveParts[0],
                    splitterCurve
                };

                splitCurveCandidateA = Curve.JoinCurves(curvesPart1)[0];

                var curvesPart2 = new Curve[2]
                {
                    curveParts[1],
                    splitterCurve
                };

                splitCurveCandidateB = Curve.JoinCurves(curvesPart2)[0];
            }

            splitParts.Add(splitCurveCandidateA);
            splitParts.Add(splitCurveCandidateB);

            for (int i = 0; i < splitParts.Count; i++)
            {
                var splitPart = splitParts[i];
                OutsideCuts.Add(splitPart, new GH_Path(index + i));

                var holesInsideSplitPart = new List<Curve>();

                for (int j = holes.Count - 1; j >= 0; j--)
                {
                    var hole = holes[j];
                    var holePoint = hole.PointAtStart;

                    var containsHole = splitPart.Contains(holePoint, Plane.WorldXY, 0.001);

                    if (containsHole == PointContainment.Inside)
                    {
                        holesInsideSplitPart.Add(hole);
                        holes.RemoveAt(j);
                    }
                }

                InsideCuts.AddRange(holesInsideSplitPart, new GH_Path(index + i));

                var millsFromSplitPart = new List<Curve>();

                for (int j = mills.Count - 1; j >= 0; j--)
                {
                    var mill = mills[j];

                    var interections = Intersection.CurveCurve(mill, splitPart, 0.001, 0.001);

                    if (interections.Count > 0)
                    {
                        millsFromSplitPart.Add(mill);
                        mills.RemoveAt(j);
                    }
                }

                HalfMills.AddRange(millsFromSplitPart, new GH_Path(index + i));

                indexes.Add(index + i);
            }

            partCount = splitParts.Count;
        }
    }
}