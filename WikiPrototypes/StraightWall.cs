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
          : base("StraightWall", "Nickname",
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

            var moduleACount = Math.Floor((length - 13.385 + 33.23 / 2) / 60);
            var moduleBCount = Math.Floor((length - 46.615 + 26.77 / 2) / 60);
            var rest = length - 10 - 13.385 - moduleACount * 33.23 - moduleBCount * 26.77;

            var branch = new RhinoList<Curve>();
            var curvesToConnect = new List<Curve>();

            var botCorner = PartSectionBuilder.GetEndConnector(0, 0, 3.385, 0);
            var upCorner = PartSectionBuilder.GetEndConnector(0, length, rest, Math.PI);

            curvesToConnect.Add(botCorner);
            curvesToConnect.Add(upCorner);

            for (int mA = 0; mA < moduleACount; mA++)
            {
                var posY = 60 * mA + 33.23 / 2 + 13.385;
                curvesToConnect.AddRange(PartSectionBuilder.GetMiddleParallelConnector(0, posY));
            }

            for (int mB = 0; mB < moduleBCount; mB++)
            {
                var posY = 60 * mB + 26.77 / 2 + 46.615;
                curvesToConnect.AddRange(PartSectionBuilder.GetMiddleSquareConnector(0, posY));
                branch.AddRange(PartSectionBuilder.GetMiddleHoles(0, posY));
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

                    branch.Add(PartSectionBuilder.GetSplitCurve(0, 30 + 60 * i));
                }
            }

            var treeResult = new DataTree<Curve>();
            treeResult.AddRange(branch, new GH_Path(0));

            DA.SetDataTree(0, treeResult);
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