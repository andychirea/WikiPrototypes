using Grasshopper.Kernel;
using System;

namespace WikiPrototypes
{
    public class StraightWallBlueprint_Component : GH_Component
    {
        public StraightWallBlueprint_Component()
          : base("Wall", "Wall",
              "Blueprint for a parametric straight wall",
              "WikiPrototypes", "Blueprints")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Length", "L", "Wall length", GH_ParamAccess.item, 120);
            pManager.AddNumberParameter("Max Part Length", "M", "The maximum lenght of a splitContour", GH_ParamAccess.item, 250);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Ourside Cuts", "O", "Guidlines for outside cuts", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Inside Cuts", "I", "Guidelines for inside cuts", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Half Mills", "HM", "Shape of half mills", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var length = 120.0;
            var maxPartLength = 250.0;

            DA.GetData(0, ref length);
            DA.GetData(1, ref maxPartLength);

            length = Math.Max(length, 60);
            maxPartLength = Math.Max(maxPartLength, 60);

            var wallBlueprint = new StraightWallBlueprint(length, maxPartLength);

            DA.SetDataTree(0, wallBlueprint.OutsideCuts);
            DA.SetDataTree(1, wallBlueprint.InsideCuts);
            DA.SetDataTree(2, wallBlueprint.HalfMills);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.straight_wall_plan;

        public override Guid ComponentGuid
        {
            get { return new Guid("065D6821-BFBC-4F57-ABE4-14BE20E41CC5"); }
        }
    }
}