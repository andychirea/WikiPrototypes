using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

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
            pManager.AddNumberParameter("Max Part Length", "M", "The maximum lenght of a part", GH_ParamAccess.item, 250);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Parts", "P", "Plans of the parts", GH_ParamAccess.tree);
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

            SolveNarrowSide(dataTree, 0, length, maxPartLength);
            SolveWidePart(dataTree, 1, length, maxPartLength);

            /*Parallel.Invoke(
                () => SolveNarrowSide(dataTree, 0, length, maxPartLength),
                () => SolveWidePart(dataTree, 1, length, maxPartLength));*/

            DA.SetDataTree(0, dataTree);
        }

        private void SolveNarrowSide(DataTree<Curve> dataTree, int index, double length, double maxPartLength)
        {
            var moduleACount = Math.Floor((length - 20 + 40 / 2) / 60);
            var moduleBCount = Math.Floor((length - 50 + 20 / 2) / 60);
            var rest = length - 10 - 10 - moduleACount * 40 - moduleBCount * 20;

            var branch = new RhinoList<Curve>();
            var curvesToConnect = new List<Curve>();

            var botCorner = NarrowPartBuilder.GetEndConnector(0, 0, false);
            var upCorner = NarrowPartBuilder.GetEndConnector(0, length, true);

            curvesToConnect.Add(botCorner);
            curvesToConnect.Add(upCorner);

            branch.AddRange(NarrowPartBuilder.GetEndHoles(0, 0, false));
            branch.AddRange(NarrowPartBuilder.GetEndHoles(0, length, true));

            for (int mA = 0; mA < moduleACount; mA++)
            {
                var posY = 60 * mA + 40 / 2 + 10;
                curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorA(0, posY));
            }

            for (int mB = 0; mB < moduleBCount; mB++)
            {
                var posY = 60 * mB + 20 / 2 + 50;
                curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorB(0, posY));
                branch.AddRange(NarrowPartBuilder.GetMiddleHoles(0, posY));
            }

            curvesToConnect.AddRange(NarrowPartBuilder.GetMiddleConnectorRest(0, length - 10 - rest * .5, rest, moduleACount <= moduleBCount));

            var contour = Curve.JoinCurves(curvesToConnect)[0];
            branch.Add(contour);

            var completeModulesCount = Math.Floor(length / 60);
            var lCounter = 0.0;

            for (int i = 0; i < completeModulesCount; i++)
            {
                if (i == 0)
                    lCounter += 30;
                else
                    lCounter += 60;

                if ((lCounter + 60) >= maxPartLength || (i == 1 && (maxPartLength + 30) < length))
                {
                    lCounter = 0.0;

                    branch.Add(NarrowPartBuilder.GetSplitCurve(0, 30 + 60 * i - 3.750));
                }
            }

            dataTree.AddRange(branch, new GH_Path(index));
        }

        private void SolveWidePart(DataTree<Curve> dataTree, int index, double length, double maxPartLength)
        {
            var moduleACount = Math.Floor((length - 20 + 40 / 2) / 60);
            var moduleBCount = Math.Floor((length - 50 + 20 / 2) / 60);
            var rest = length - 10 - 10 - moduleACount * 40 - moduleBCount * 20;

            var branch = new RhinoList<Curve>();
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
                branch.AddRange(WidePartBuilder.GetMiddleHoles(0, posY));
            }

            var contour = Curve.JoinCurves(curvesToConnect)[0];
            branch.Add(contour);

            var completeModulesCount = Math.Floor(length / 60);
            var lCounter = 0.0;

            for (int i = 0; i < completeModulesCount; i++)
            {
                if (i == 0)
                    lCounter += 30;
                else
                    lCounter += 60;

                if ((lCounter + 60) >= maxPartLength)
                {
                    lCounter = 0.0;

                    branch.Add(WidePartBuilder.GetSplitCurve(0, 30 + 60 * i));
                }
            }

            dataTree.AddRange(branch, new GH_Path(index));
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