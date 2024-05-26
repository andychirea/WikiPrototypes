using Grasshopper.Kernel;
using System;

namespace WikiPrototypes
{
    public class StraightWallVolume_Component : GH_Component
    {
        public StraightWallVolume_Component()
          : base("Wall Volume", "WV",
              "Brep for a parametric straight wall",
              "WikiPrototypes", "Volumes")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Length", "L", "Wall length", GH_ParamAccess.item, 120);
            pManager.AddNumberParameter("Max Part Length", "M", "The maximum lenght of a splitContour", GH_ParamAccess.item, 250);
            pManager.AddNumberParameter("Thickness", "T", "The thickness of the panel", GH_ParamAccess.item, 1.8);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Parts", "P", "Volume of the parts", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var length = 120.0;
            var maxPartLength = 250.0;
            var thickness = 1.8;

            DA.GetData(0, ref length);
            DA.GetData(1, ref maxPartLength);
            DA.GetData(2, ref thickness);

            thickness = Math.Min(Math.Max(thickness, 1), 3);

            var straightWallVolume = new StraightWallVolume(length, maxPartLength, thickness);

            DA.SetDataList(0, straightWallVolume.Shapes);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.straight_wall_volume;

        public override Guid ComponentGuid
        {
            get { return new Guid("26954213-A51D-4F19-8A24-5FAC22FBACE7"); }
        }
    }
}