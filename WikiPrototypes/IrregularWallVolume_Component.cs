using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace WikiPrototypes
{
    public class IrregularWallVolume_Component : GH_Component
    {
        public IrregularWallVolume_Component()
          : base("Irregular Wall Volume", "IWV",
              "Brep for a parametric irregular wall",
              "WikiPrototypes", "Volumes")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve guide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Straight Length", "MS", "The maximum lenght of a striaght part", GH_ParamAccess.item, 250);
            pManager.AddNumberParameter("Max Corner Length", "MC", "The maximum lenght of a corner part", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Thickness", "T", "The thickness of the panel", GH_ParamAccess.item, 1.8);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Parts", "P", "Volume of the parts", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curve = (Curve)null;
            var maxStraightLength = 250.0;
            var maxCornerLength = 100.0;
            var thickness = 1.8;

            DA.GetData(0, ref curve);
            DA.GetData(1, ref maxStraightLength);
            DA.GetData(2, ref maxCornerLength);
            DA.GetData(3, ref thickness);

            if (curve == null)
                return;

            maxStraightLength = Math.Max(maxStraightLength, 120);
            maxCornerLength = Math.Max(maxCornerLength, 10);
            thickness = Math.Min(Math.Max(thickness, 1), 3);

            var straightWallVolume = new IrregularWallVolume(curve, maxStraightLength, maxCornerLength, thickness);

            DA.SetDataList(0, straightWallVolume.Shapes);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.irregular_wall_volume;

        public override Guid ComponentGuid
        {
            get { return new Guid("E3CEA19D-8C9D-424F-8173-C471C9C938E4"); }
        }
    }
}