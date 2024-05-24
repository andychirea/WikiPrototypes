using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public class IrregularWallBlueprint_Component : GH_Component
    {
        public IrregularWallBlueprint_Component()
          : base("Irregular Wall", "IWall",
              "A parametric irregular wall",
              "WikiPrototypes", "Blueprints")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "L", "Wall guide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Straight Length", "MS", "The maximum lenght of a straight part", GH_ParamAccess.item, 250);
            pManager.AddNumberParameter("Max Corner Length", "MC", "The maximum lenght of a limb from a corner", GH_ParamAccess.item, 100);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Ourside Cuts", "O", "Guidlines for outside cuts", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Inside Cuts", "I", "Guidelines for inside cuts", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Half Mills", "HM", "Shape of half mills", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curve = (Curve)null;
            var maxPartLength = 250.0;
            var maxCornerLength = 100.0;

            DA.GetData(0, ref curve);
            DA.GetData(1, ref maxPartLength);
            DA.GetData(2, ref maxCornerLength);

            var irregularWallBlueprint = new IrregularWallBlueprint(curve, maxPartLength, maxCornerLength);

            DA.SetDataTree(0, irregularWallBlueprint.OutsideCuts);
            DA.SetDataTree(1, irregularWallBlueprint.InsideCuts);
            DA.SetDataTree(2, irregularWallBlueprint.HalfMills);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.irregular_wall_plan;

        public override Guid ComponentGuid
        {
            get { return new Guid("7659B03B-17AF-411B-A982-F0DBD287CD03"); }
        }
    }
}