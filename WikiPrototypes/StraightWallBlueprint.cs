﻿using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;

namespace WikiPrototypes
{
    public class StraightWallBlueprint
    {
        public DataTree<Curve> OutsideCuts { get; private set; }
        public DataTree<Curve> InsideCuts { get; private set; }
        public DataTree<Curve> HalfMills { get; private set; }

        public List<int> NarrowPartBrachIndexes { get; private set; }
        public List<int> WidePartBrachIndexes { get; private set; }
        public List<int> TransversalPartBrachIndexes { get; private set; }

        public StraightWallBlueprint(double length, double maxPartLength, double thickness, double millingDiameter)
        {
            OutsideCuts = new DataTree<Curve>();
            InsideCuts = new DataTree<Curve>();
            HalfMills = new DataTree<Curve>();

            NarrowPartBrachIndexes = new List<int>();
            WidePartBrachIndexes = new List<int>();
            TransversalPartBrachIndexes = new List<int>();

            SolveNarrowPart(0, length, maxPartLength, thickness, millingDiameter, out var narrowPartCount);
            SolveWidePart(narrowPartCount, length, maxPartLength, thickness, millingDiameter, out var widePartCount);
            SolveTransversalParts(narrowPartCount + widePartCount, millingDiameter, length, out _);
        }

        private void SolveTransversalParts(int index, double millingDiameter, double length, out int partCount)
        {
            var posX = -60;

            var connectorLength = length - 20;
            var loopCount = Math.Floor(connectorLength / 60);
            var restOfSpace = connectorLength - loopCount * 60;
            var connectorCount = restOfSpace < 10 ? Math.Max(loopCount - 1, 0) : loopCount;
            partCount = (int)connectorCount;

            for (int i = 0; i < partCount; i++)
            {
                var contour = TransversalPartBuilder.GetContour();

                if (millingDiameter > 0)
                    contour = contour.Offset(Plane.WorldXY, millingDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp)[0];
                
                var translation = Transform.Translation(posX, 60 + 60 * i, 0);
                contour.Transform(translation);

                OutsideCuts.Add(contour, new GH_Path(index + i));
                InsideCuts.EnsurePath(index + i);
                HalfMills.EnsurePath(index + i);

                TransversalPartBrachIndexes.Add(index + i);
            }
        }

        private void SolveNarrowPart(int index, double length, double maxPartLength, double thickness, double millingDiameter, out int partCount)
        {
            var connectorLength = length - 20;
            var loopCount = Math.Floor(connectorLength / 60);
            var restOfSpace = connectorLength - loopCount * 60;
            var moduleACount = restOfSpace >= 40 ? loopCount + 1 : loopCount;
            var moduleBCount = restOfSpace < 10 ?  Math.Max(loopCount - 1, 0) : loopCount;
            var rest = length - 10 - 10 - moduleACount * 40 - moduleBCount * 20;

            var curvesToConnect = new List<Curve>();
            var holes = new List<Curve>();
            var splitCurves = new List<Curve>();

            curvesToConnect.Add(NarrowPartBuilder.GetEndConnector(0, 0, false));
            curvesToConnect.Add(NarrowPartBuilder.GetEndConnector(0, length, true));

            holes.AddRange(NarrowPartBuilder.GetEndHoles(0, 0, thickness, 0));
            holes.AddRange(NarrowPartBuilder.GetEndHoles(0, length, thickness, Math.PI));

            for (int mA = 0; mA < moduleACount; mA++)
            {
                var posY = 60 * mA + 40 / 2 + 10;
                curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorA(0, posY, thickness));
            }

            for (int mB = 0; mB < moduleBCount; mB++)
            {
                var posY = 60 * mB + 20 / 2 + 50;
                curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorB(0, posY));
                holes.AddRange(NarrowPartBuilder.GetMiddleHoles(0, posY, thickness));
            }

            curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorRest(0, length - 10 - rest * .5, rest, moduleACount <= moduleBCount));

            var contour = Curve.JoinCurves(curvesToConnect)[0];

            if (maxPartLength < length)
            {
                var splitSeparation = Math.Floor(maxPartLength / 60) * 60;
                var startY = 30 + 60 - 3.750;
                var completeModulesCount = Math.Floor((length - 10 - rest - startY) / splitSeparation);

                for (int i = 0; i <= completeModulesCount; i++)
                {
                    splitCurves.Add(NarrowPartBuilder.GetSplitCurve(0, startY + splitSeparation * i, thickness, 0));
                }
            }

            SplitPart(millingDiameter, contour, splitCurves, holes, new List<Curve>(), index, NarrowPartBrachIndexes, out partCount);
        }

        private void SolveWidePart(int index, double length, double maxPartLength, double thickness, double millingDiameter, out int partCount)
        {
            var posX = 60;

            var connectorLength = length - 20;
            var loopCount = Math.Floor(connectorLength / 60);
            var restOfSpace = connectorLength - loopCount * 60;
            var moduleACount = restOfSpace >= 40 ? loopCount + 1 : loopCount;
            var moduleBCount = restOfSpace < 10 ? Math.Max(loopCount - 1, 0) : loopCount;
            var rest = length - 10 - 10 - moduleACount * 40 - moduleBCount * 20;

            var splitCurves = new List<Curve>();
            var holes = new List<Curve>();
            var mills = new List<Curve>();
            var curvesToConnect = new List<Curve>();

            var botCorner = WidePartBuilder.GetEndConnector(posX, 0, 0, thickness, true, 0);
            var upCorner = WidePartBuilder.GetEndConnector(posX, length, rest, thickness, moduleACount > moduleBCount, Math.PI);

            mills.AddRange(WidePartBuilder.GetEndMill(posX, 0, thickness, 0));
            mills.AddRange(WidePartBuilder.GetEndMill(posX, length, thickness, Math.PI));

            curvesToConnect.Add(botCorner);
            curvesToConnect.Add(upCorner);

            for (int mA = 0; mA < moduleACount; mA++)
            {
                var posY = 60 * mA + 40 / 2 + 10;
                curvesToConnect.AddRange(WidePartBuilder.GetMiddleParallelConnector(posX, posY, thickness));

                mills.AddRange(WidePartBuilder.GetMiddleParallelMill(posX, posY));
            }

            for (int mB = 0; mB < moduleBCount; mB++)
            {
                var posY = 60 * mB + 20 / 2 + 50;
                curvesToConnect.AddRange(WidePartBuilder.GetMiddleSquareConnector(posX, posY, thickness));
                holes.AddRange(WidePartBuilder.GetMiddleHoles(posX, posY, thickness));

                mills.AddRange(WidePartBuilder.GetMiddleSquareMill(posX, posY, thickness));
            }

            var contour = Curve.JoinCurves(curvesToConnect)[0];

            if (maxPartLength < length)
            {
                var splitSeparation = Math.Floor(maxPartLength / 60) * 60;
                var startY = splitSeparation - 30;
                var completeModulesCount = Math.Floor((length - 10 - rest - startY) / splitSeparation);

                for (int i = 0; i <= completeModulesCount; i++)
                {
                    splitCurves.Add(WidePartBuilder.GetSplitCurve(posX, startY + splitSeparation * i, thickness));
                }
            }

            SplitPart(millingDiameter, contour, splitCurves, holes, mills, index, WidePartBrachIndexes, out partCount);
        }

        private void SplitPart(double millingDiameter, Curve contour, List<Curve> splitCurves, List<Curve> holes, List<Curve> mills, int index, List<int> indexes, out int partCount)
        {
            if (splitCurves.Count == 0)
            {
                var path = new GH_Path(index);

                if (millingDiameter > 0)
                {
                    contour = contour.Offset(Plane.WorldXY, millingDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp)[0];

                    for (int i = 0; i < holes.Count; i++)
                    {
                        holes[i] = holes[i].Offset(Plane.WorldXY, -millingDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp)[0];
                    }

                    for (int i = 0; i < mills.Count; i++)
                    {
                        mills[i] = mills[i].Offset(Plane.WorldXY, millingDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp)[0];
                    }
                }

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

                if (millingDiameter > 0)
                    splitPart = splitPart.Offset(Plane.WorldXY, millingDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp)[0];

                OutsideCuts.Add(splitPart, new GH_Path(index + i));

                var boundingBox = splitPart.GetBoundingBox(false);

                var holesInsideSplitPart = new List<Curve>();

                for (int j = holes.Count - 1; j >= 0; j--)
                {
                    var hole = holes[j];
                    var holePoint = hole.PointAtStart;

                    var containsHole = boundingBox.Contains(holePoint);

                    if (containsHole)
                    {
                        holesInsideSplitPart.Add(hole);
                        holes.RemoveAt(j);
                    }
                }

                if (millingDiameter > 0)
                {
                    for (int j = 0; j < holes.Count; j++)
                    {
                        holes[j] = holes[j].Offset(Plane.WorldXY, -millingDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp)[0];
                    }
                }

                InsideCuts.AddRange(holesInsideSplitPart, new GH_Path(index + i));

                var millsFromSplitPart = new List<Curve>();

                for (int j = mills.Count - 1; j >= 0; j--)
                {
                    var mill = mills[j];
                    var holePoint = mill.PointAtEnd;

                    var containsHole = boundingBox.Contains(holePoint);

                    if (containsHole)
                    {
                        millsFromSplitPart.Add(mill);
                        mills.RemoveAt(j);
                    }
                }

                if (millingDiameter > 0)
                {
                    for (int j = 0; j < mills.Count; j++)
                    {
                        mills[j] = mills[j].Offset(Plane.WorldXY, millingDiameter / 2, 0.001, CurveOffsetCornerStyle.Sharp)[0];
                    }
                }

                HalfMills.AddRange(millsFromSplitPart, new GH_Path(index + i));

                indexes.Add(index + i);
            }

            partCount = splitParts.Count;
        }
    }
}