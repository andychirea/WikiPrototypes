using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WikiPrototypes
{
    public class StraightWall : GH_Component
    {
        public StraightWall()
          : base("Straight Wall", "SWall",
              "A parametric straight wall",
              "WikiPrototypes", "Walls")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Length", "L", "Wall length", GH_ParamAccess.item, 120);
            pManager.AddNumberParameter("Max Part Length", "M", "The maximum lenght of a splitContour", GH_ParamAccess.item, 250);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Parts", "P", "Plans of the parts", GH_ParamAccess.tree);
            pManager.AddBrepParameter("3D Parts", "3D", "Breps of the parts", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var length = 120.0;
            var maxPartLength = 250.0;

            DA.GetData(0, ref length);
            DA.GetData(1, ref maxPartLength);

            length = Math.Max(length, 60);
            maxPartLength = Math.Max(maxPartLength, 60);

            var dataTree = new DataTree<Curve>();

            SolveNarrowPart(dataTree, 0, length, maxPartLength, out var narrowPartCount);
            SolveWidePart(dataTree, narrowPartCount, length, maxPartLength, out var widePartCount);

            DA.SetDataTree(0, dataTree);

            var totalPartCount = narrowPartCount + widePartCount;
            var shapes = new Brep[totalPartCount];

            for (int i = 0; i < totalPartCount; i++)
            {
                var curves = dataTree.Branch(i);
                var contour = curves[0];
                var holes = new Curve[curves.Count - 1];
                
                for (var j = 1; j < curves.Count; j++)
                {
                    holes[j - 1] = curves[j];
                }

                shapes[i] = Get3DShape(contour, holes);
            }

            //Parallel.Invoke(
            //    () => shapes[0] = Get3DShape(narrowContour, narrowHoles),
            //    () => shapes[1] = Get3DShape(wideContour, wideHoles)
            //    );

            DA.SetDataList(1, shapes);
        }

        private Brep Get3DShape(Curve contour, IEnumerable<Curve> holes)
        {
            var surface = Brep.CreateTrimmedPlane(Plane.WorldXY, contour);

            foreach (var hole in holes)
            {
                var holeBoundaries = new Curve[]
                {
                    hole
                };
                surface.Loops.AddPlanarFaceLoop(0, BrepLoopType.Inner, holeBoundaries);
            }

            var extrusion = surface.Faces[0].CreateExtrusion(new Line(0, 0, 0, 0, 0, 1.8).ToNurbsCurve(), true);

            return extrusion;
        }

        private void SolveNarrowPart(DataTree<Curve> dataTree, int index, double length, double maxPartLength, out int partCount)
        {
            var moduleACount = Math.Floor((length - 20 + 40 / 2) / 60);
            var moduleBCount = Math.Floor((length - 50 + 20 / 2) / 60);
            var rest = length - 10 - 10 - moduleACount * 40 - moduleBCount * 20;

            var curvesToConnect = new List<Curve>();
            var holes = new List<Curve>();
            var splitCurves = new List<Curve>();

            curvesToConnect.Add(NarrowPartBuilder.GetEndConnector(0, 0, false));
            curvesToConnect.Add(NarrowPartBuilder.GetEndConnector(0, length, true));

            holes.AddRange(NarrowPartBuilder.GetEndHoles(0, 0, false));
            holes.AddRange(NarrowPartBuilder.GetEndHoles(0, length, true));

            for (int mA = 0; mA < moduleACount; mA++)
            {
                var posY = 60 * mA + 40 / 2 + 10;
                curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorA(0, posY));
            }

            for (int mB = 0; mB < moduleBCount; mB++)
            {
                var posY = 60 * mB + 20 / 2 + 50;
                curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorB(0, posY));
                holes.AddRange(NarrowPartBuilder.GetMiddleHoles(0, posY));
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
                    splitCurves.Add(NarrowPartBuilder.GetSplitCurve(0, startY + splitSeparation * i));
                }
            }

            SplitPart(dataTree, contour, splitCurves, holes, index, out partCount);
        }

        private void SolveWidePart(DataTree<Curve> dataTree, int index, double length, double maxPartLength, out int partCount)
        {
            var moduleACount = Math.Floor((length - 20 + 40 / 2) / 60);
            var moduleBCount = Math.Floor((length - 50 + 20 / 2) / 60);
            var rest = length - 10 - 10 - moduleACount * 40 - moduleBCount * 20;

            var splitCurves = new List<Curve>();
            var holes = new List<Curve>();
            var curvesToConnect = new List<Curve>();

            var botCorner = WidePartBuilder.GetEndConnector(0, 0, 0, true, 0);
            var upCorner = WidePartBuilder.GetEndConnector(0, length, rest, moduleACount > moduleBCount, Math.PI);

            curvesToConnect.Add(botCorner);
            curvesToConnect.Add(upCorner);

            for (int mA = 0; mA < moduleACount; mA++)
            {
                var posY = 60 * mA + 40 / 2 + 10;
                curvesToConnect.AddRange(WidePartBuilder.GetMiddleParallelConnector(0, posY));
            }

            for (int mB = 0; mB < moduleBCount; mB++)
            {
                var posY = 60 * mB + 20 / 2 + 50;
                curvesToConnect.AddRange(WidePartBuilder.GetMiddleSquareConnector(0, posY));
                holes.AddRange(WidePartBuilder.GetMiddleHoles(0, posY));
            }

            var contour = Curve.JoinCurves(curvesToConnect)[0];
            holes.Add(contour);

            if (maxPartLength < length)
            {
                var splitSeparation = Math.Floor(maxPartLength / 60) * 60;
                var startY = splitSeparation - 30;
                var completeModulesCount = Math.Floor((length - 10 - rest - startY) / splitSeparation);

                for (int i = 0; i <= completeModulesCount; i++)
                {
                    splitCurves.Add(WidePartBuilder.GetSplitCurve(0, startY + splitSeparation * i));
                }
            }

            SplitPart(dataTree, contour, splitCurves, holes, index, out partCount);
        }

        private void SplitPart(DataTree<Curve> dataTree, Curve contour, List<Curve> splitCurves, List<Curve> holes, int index, out int partCount)
        {
            if (splitCurves.Count == 0)
            {
                var complete = new RhinoList<Curve>
                {
                    contour
                };

                dataTree.AddRange(complete, new GH_Path(index));

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

                var curveList = new List<Curve>
                {
                    splitPart
                };

                for (int j = holes.Count - 1; j >= 0; j--)
                {
                    var hole = holes[j];
                    var holePoint = hole.PointAtStart;

                    var containsHole = splitPart.Contains(holePoint, Plane.WorldXY, 0.001);

                    if (containsHole == PointContainment.Inside)
                    {
                        curveList.Add(hole);
                        holes.RemoveAt(j);
                    }
                }

                dataTree.AddRange(curveList, new GH_Path(index + i));
            }

            partCount = splitParts.Count;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("065D6821-BFBC-4F57-ABE4-14BE20E41CC5"); }
        }
    }
}