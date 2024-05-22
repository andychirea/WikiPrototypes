using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

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
            pManager.AddNumberParameter("Max Limb Length", "M", "The maximum lenght of a part", GH_ParamAccess.item, 250);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Parts", "P", "Plans of the parts", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curve = (Curve)null;
            var maxPartLength = 250.0;

            DA.GetData(0, ref curve);
            DA.GetData(1, ref maxPartLength);

            if (!curve.IsPlanar())
                return;

            if (curve.TryGetPlane(out var plane))
            {
                var normal = plane.Normal;

                if (normal.Z < 0)
                    plane.Flip();

                normal = plane.Normal;

                if (normal != Vector3d.ZAxis)
                {
                    var transformOrientation = Transform.PlaneToPlane(plane, Plane.WorldXY);
                    curve.Transform(transformOrientation);
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
                return;

            var guideLines = new Line[segments.Length];

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                guideLines[i] = new Line(segment.PointAtStart, segment.PointAtEnd);
            }

            var shapeResult = IrregularSidePartBuilder.GetShapes(guideLines);

            var result = new List<Curve>();
            result.AddRange(shapeResult);

            DA.SetDataList(0, result);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.irregular_wall_plan;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("7659B03B-17AF-411B-A982-F0DBD287CD03"); }
        }
    }
}