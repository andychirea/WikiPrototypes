using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace WikiPrototypes
{
    public class Wall : GH_Component
    {
        public Wall()
          : base("Wall", "SW",
              "A parametric wall",
              "WikiPrototypes", "Walls")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Length", "L", "Wall length", GH_ParamAccess.item, 120);
            pManager.AddBooleanParameter("Modulate", "M", "Modulate the wall to 30cm", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Contour", "C", "Contour plans", GH_ParamAccess.list);
            pManager.AddCurveParameter("CutOuts", "CO", "Cut out plans", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var length = 120.0;
            var modulate = false;

            if (!DA.GetData(0, ref length))
                length = 120;
            if (!DA.GetData(1, ref modulate))
                modulate = false;

            if (modulate)
                length = Math.Floor(length / 30) * 30;

            GetFrontCurves(length, out var frontContour, out var frontCutOffs);
            GetSideCurves(length, out var sideContour, out var sideCutOffs);

            var contourOutputs = new List<Curve>
            {
                frontContour,
                sideContour
            };

            var cutOffOutputs = new List<Curve>();
            cutOffOutputs.AddRange(frontCutOffs);
            cutOffOutputs.AddRange(sideCutOffs);

            DA.SetDataList(0, contourOutputs);
            DA.SetDataList(1, frontCutOffs);
        }

        private void GetFrontCurves(double length, out NurbsCurve contour, out NurbsCurve[] cutOffs)
        {
            var xExtends = 30;

            var cornerA = new Point3d(-xExtends, 0.001, 0);
            var cornerB = new Point3d(xExtends, length - 0.001, 0);

            var rectangle = new Rectangle3d(Plane.WorldXY, cornerA, cornerB).ToNurbsCurve();
            var outerCutOffs = new List<Curve>();
            var innerCutOffs = new List<Curve>();

            outerCutOffs.AddRange(GeometryHelper.GetEndCutOuts(length));

            var connectors1Count = Math.Floor(length / 60);
            for (int i = 0; i < connectors1Count; i++)
            {
                outerCutOffs.AddRange(GeometryHelper.GetConnectorModule1(0, i * 60));
            }

            var connectors2Count = Math.Floor((length - 30) / 60);
            for (int i = 0; i < connectors2Count; i++)
            {
                outerCutOffs.AddRange(GeometryHelper.GetConnectorModule2(0, i * 60));
                innerCutOffs.AddRange(GeometryHelper.GetMiddleCutOuts(0, i * 60));
            }

            contour = outerCutOffs[0].ToNurbsCurve();

            //var output = Curve.CreateBooleanDifference(rectangle, outerCutOffs);

            /*if (output.Length > 0)
                contour = output[0].ToNurbsCurve();
            else 
                contour = null;*/

            cutOffs = innerCutOffs.ToNurbsCurves();
        }

        private void GetSideCurves(double length, out Curve contour, out List<Curve> cutOffs)
        {
            var xExtends = 15.9;
            var rot90 = Math.PI * .5;

            var cornerA = new Point3d(-xExtends, 0, 0);
            var cornerB = new Point3d(xExtends, length, 0);

            var rectangle = new Rectangle3d(Plane.WorldXY, cornerA, cornerB).ToNurbsCurve();
            var cutOutsList = new List<Curve>();

            cutOutsList.AddRange(new Curve[4]
            {
                GeometryHelper.GetHCutOff(6, 3.60, -5, 0, 0),
                GeometryHelper.GetHCutOff(6, 3.60, +7, 0, 0),
                GeometryHelper.GetHCutOff(6, 3.60, -5, length, 0),
                GeometryHelper.GetHCutOff(6, 3.60, +7, length, 0),
            });

            var connectorsCount = Math.Floor(length / 60);
            for (int i = 0; i < connectorsCount; i++)
            {
                cutOutsList.AddRange(GeometryHelper.GetConnectorModule(0, i * 60));
            }

            var rest = length - connectorsCount * 60 - 20;
            if (rest >= 10)
            {
                cutOutsList.AddRange(new Curve[2]
                {
                    GeometryHelper.GetHCutOff(rest, 10,      -15.9, 10 + rest * .5 + connectorsCount * 60, rot90),
                    GeometryHelper.GetHCutOff(rest, 3.60,    +15.9, 10 + rest * .5 + connectorsCount * 60, rot90),
                });
            }

            var output = Curve.CreateBooleanDifference(rectangle, cutOutsList);

            contour = output[0];

            // *** RECORTES ***

            var perfsList = new List<Curve>();

            var endPerfs = new Curve[4]
            {
                GeometryHelper.GetRCutOff(2.519, 0.45, -11.35, 5.24, rot90),
                GeometryHelper.GetRCutOff(2.519, 0.45, +14.55, 5.24, rot90),
                GeometryHelper.GetRCutOff(2.519, 0.45, -11.35, length - 5.24, rot90),
                GeometryHelper.GetRCutOff(2.519, 0.45, +14.55, length - 5.24, rot90),
            };

            perfsList.AddRange(endPerfs);

            for (int i = 1; i <= connectorsCount; i++)
            {
                if (connectorsCount == i && rest < 10)
                    break;

                var yPos = i * 60;
                var strangeShape = new Curve[2]
                {
                    GeometryHelper.GetHCutOff(5, 1.2, -11.5, yPos, rot90),
                    GeometryHelper.GetSCutOff(1.2, 6.2, -12.1, yPos).ToNurbsCurve(),
                };
                var modulatedPerfs = new Curve[3]
                {
                    GeometryHelper.GetHCutOff(5, 1.8, 1.596, yPos, 0),
                    Curve.CreateBooleanUnion(strangeShape)[0],
                    GeometryHelper.GetRCutOff(5, 0.45, 14.55, yPos, rot90),
                };

                perfsList.AddRange(modulatedPerfs);
            }

            cutOffs = perfsList;
        }

        private void GetTransversalContour(out Curve contour)
        {
            var points = new Point3d[]
            {
                new Point3d(30.0, 2.5, 0),
                new Point3d(27.6, 2.5, 0),
                new Point3d(27.6, 3.7, 0),
                new Point3d(28.2, 3.7, 0),
                new Point3d(28.2, 12.5, 0),
                new Point3d(7.20, 12.5, 0),
                new Point3d(7.20, 11.9, 0),
                new Point3d(6.00, 11.9, 0),
                new Point3d(6.00, 14.3, 0),

                new Point3d(-6.00, 14.3, 0),
                new Point3d(-6.00, 11.9, 0),
                new Point3d(-7.20, 11.9, 0),
                new Point3d(-7.20, 12.5, 0),
                new Point3d(-28.2, 12.5, 0),
                new Point3d(-28.2, 3.7, 0),
                new Point3d(-27.6, 3.7, 0),
                new Point3d(-27.6, 2.5, 0),
                new Point3d(-30.0, 2.5, 0),

                new Point3d(-30.0, -2.5, 0),
                new Point3d(-27.6, -2.5, 0),
                new Point3d(-27.6, -3.7, 0),
                new Point3d(-28.2, -3.7, 0),
                new Point3d(-28.2, -12.5, 0),
                new Point3d(-7.20, -12.5, 0),
                new Point3d(-7.20, -11.9, 0),
                new Point3d(-6.00, -11.9, 0),
                new Point3d(-6.00, -14.3, 0),

                new Point3d(6.00, -14.3, 0),
                new Point3d(6.00, -11.9, 0),
                new Point3d(7.20, -11.9, 0),
                new Point3d(7.20, -12.5, 0),
                new Point3d(28.2, -12.5, 0),
                new Point3d(28.2, -3.7, 0),
                new Point3d(27.6, -3.7, 0),
                new Point3d(27.6, -2.5, 0),
                new Point3d(30.0, -2.5, 0),

                new Point3d(30.0, 2.5, 0),
            };

            contour = new PolylineCurve(points);
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
            get { return new Guid("BBD6D56A-F85F-4B58-93FE-2EFB406800BD"); }
        }
    }
}