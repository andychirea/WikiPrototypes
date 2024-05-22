using Rhino.Geometry;
using System.Collections.Generic;

namespace WikiPrototypes
{
    public class StraightWallVolume
    {
        public Brep[] Shapes { get; private set; }

        public StraightWallVolume(double lenght, double maxPartLength, double thickness)
        {
            var blueprint = new StraightWallBlueprint(lenght, maxPartLength);

            var outsideCuts = blueprint.OutsideCuts;
            var insideCuts = blueprint.InsideCuts;

            var totalPartCount = outsideCuts.BranchCount;
            var rawShapes = new Brep[totalPartCount];
            var shapes = new List<Brep>();

            for (int i = 0; i < totalPartCount; i++)
            {
                var contour = outsideCuts.Branch(i)[0];
                var holes = insideCuts.Branch(i);

                rawShapes[i] = Get3DShape(contour, holes, thickness);
            }

            var narrowPartReferencePlane = new Plane(new Point3d(0, 0, thickness * .5), Vector3d.XAxis, Vector3d.ZAxis);
            var narrowPartFinalRightPlane = new Plane(new Point3d(29.1, 0, 0), -Vector3d.YAxis, -Vector3d.XAxis);
            var narrowPartFinalLeftPlane = new Plane(new Point3d(-29.1, 0, 0), -Vector3d.YAxis, -Vector3d.XAxis);
            var narrowPartRightTranform = Transform.PlaneToPlane(narrowPartReferencePlane, narrowPartFinalRightPlane);
            var narrowPartLeftTranform = Transform.PlaneToPlane(narrowPartReferencePlane, narrowPartFinalLeftPlane);

            var narrowPartIndexes = blueprint.NarrowPartBrachIndexes;

            foreach (var index in narrowPartIndexes)
            {
                var narrowRightShape = rawShapes[index];
                var narrowLeftShape = narrowRightShape.DuplicateBrep();

                narrowRightShape.Transform(narrowPartRightTranform);
                narrowLeftShape.Transform(narrowPartLeftTranform);

                shapes.Add(narrowRightShape);
                shapes.Add(narrowLeftShape);
            }

            var widePartReferencePlane = new Plane(new Point3d(60, 0, thickness * .5), Vector3d.XAxis, Vector3d.ZAxis);
            var widePartFinalRightPlane = new Plane(new Point3d(0, -13.4, 0), Vector3d.XAxis, -Vector3d.YAxis);
            var widePartFinalLeftPlane = new Plane(new Point3d(0, 13.4, 0), Vector3d.XAxis, -Vector3d.YAxis);
            var widePartRightTranform = Transform.PlaneToPlane(widePartReferencePlane, widePartFinalRightPlane);
            var widePartLeftTranform = Transform.PlaneToPlane(widePartReferencePlane, widePartFinalLeftPlane);

            var widePartIndexes = blueprint.WidePartBrachIndexes;

            foreach (var index in widePartIndexes)
            {
                var wideRightShape = rawShapes[index];
                var wideLeftShape = wideRightShape.DuplicateBrep();

                wideRightShape.Transform(widePartRightTranform);
                wideLeftShape.Transform(widePartLeftTranform);

                shapes.Add(wideRightShape);
                shapes.Add(wideLeftShape);
            }
            
            var transPartIndexes = blueprint.TransversalPartBrachIndexes;

            for (int i = 0; i < transPartIndexes.Count; i++)
            {
                int index = transPartIndexes[i];
                var transShape = rawShapes[index];

                var transPartReferencePlane = new Plane(new Point3d(-60, 60 + 60 * i, thickness * .5), Vector3d.XAxis, Vector3d.YAxis);
                var transPartFinalPlane = new Plane(new Point3d(0, 0, 60 + 60 * i), Vector3d.XAxis, Vector3d.YAxis);
                var transPartTranform = Transform.PlaneToPlane(transPartReferencePlane, transPartFinalPlane);

                transShape.Transform(transPartTranform);

                shapes.Add(transShape);
            }

            Shapes = shapes.ToArray();
        }

        private Brep Get3DShape(Curve contour, IEnumerable<Curve> holes, double thickness)
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

            var extrusion = surface.Faces[0].CreateExtrusion(new Line(0, 0, 0, 0, 0, thickness).ToNurbsCurve(), true);

            return extrusion;
        }
    }
}