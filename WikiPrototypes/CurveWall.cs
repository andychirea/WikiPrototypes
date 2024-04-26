using Grasshopper.Kernel;
using System;

namespace WikiPrototypes
{
    public class CurveWall : GH_Component
    {
        public CurveWall()
          : base("Curve Wall", "CW",
              "A parametric curve wall",
              "WikiPrototypes", "Walls")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("13149F26-FB8C-46AF-8BE0-E3A6EB7CBC68"); }
        }
    }
}