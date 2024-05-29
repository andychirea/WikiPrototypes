using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WikiPrototypes
{
    public class IrregularWallVolume
    {
        public Brep[] Shapes { get; private set; }

        public IrregularWallVolume(Curve curve, double maxStraightLength, double maxCornerLength, double thickness)
        {
            var blueprint = new IrregularWallBlueprint(curve, maxStraightLength, maxCornerLength, thickness, 0.0);

            var outsideCuts = blueprint.OutsideCuts;
            var insideCuts = blueprint.InsideCuts;

            var totalPartCount = outsideCuts.BranchCount;
            var rawShapes = new Brep[totalPartCount];
            var shapes = new List<Brep>();

            var actions = new Action[totalPartCount];

            for (int i = 0; i < totalPartCount; i++)
            {
                int index = i;
                actions[index] = () =>
                {
                    var contour = outsideCuts.Branch(index)[0];
                    var holes = insideCuts.Branch(index);

                    rawShapes[index] = Get3DShape(contour, holes, thickness);
                };
            }

            Parallel.Invoke(actions);

            var firstGuidePoint = blueprint.GuideLines[0].PointAt(0);
            var guideLinePlane = blueprint.GuideLinePlane;
            var narrowPartReferencePlane = new Plane(firstGuidePoint + new Point3d(0, 0, thickness * .5), Vector3d.XAxis, Vector3d.YAxis);
            var narrowPartFinalRightPlane = new Plane(guideLinePlane.Origin + guideLinePlane.ZAxis * (30 - thickness / 2), guideLinePlane.XAxis, guideLinePlane.YAxis);
            var narrowPartFinalLeftPlane = new Plane(guideLinePlane.Origin - guideLinePlane.ZAxis * (30 - thickness / 2), guideLinePlane.XAxis, guideLinePlane.YAxis);
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

            var widePartIndexes = blueprint.WidePartBrachIndexes;
            var widePartGuideIndexes = blueprint.WidePartGuideLineIndex;
            var widePartPositions = blueprint.WidePartPositions;
            var widePartCount = widePartIndexes.Count;

            var guideLineData = blueprint.GuideLineData;
            var startOffsets = guideLineData.OffsetsAtStart;
            var devations = guideLineData.DeviationSigns;

            blueprint.GuideLineTransform.TryGetInverse(out var inverseTransform);

            for (int i = 0; i < widePartCount; i++)
            {
                var guideLineIndex = widePartGuideIndexes[i];
                var sign = Math.Sign(guideLineIndex);

                if (sign < 0)
                    guideLineIndex++;
                else
                    guideLineIndex--;

                guideLineIndex *= sign;

                if (guideLineIndex < 0 || guideLineIndex >= blueprint.GuideLines.Length)
                    continue;

                var guideLine = blueprint.GuideLines[guideLineIndex];
                var point = guideLine.PointAt(0);
                var direction = guideLine.Direction;
                direction.Transform(inverseTransform);
                point.Transform(inverseTransform);

                var newPlane = new Plane(guideLinePlane.Origin, guideLinePlane.ZAxis, direction);

                var referencePos = widePartPositions[i] + new Point3d(0, 0, thickness * .5);

                var deviationSign = guideLineIndex == 0 ? 0 : devations[guideLineIndex - 1];
                var guideOffset = startOffsets[guideLineIndex];
                var guideOffsetVector = -newPlane.YAxis * guideOffset * sign * deviationSign;
                
                var widePartReferencePlane = new Plane(referencePos, Vector3d.XAxis, Vector3d.YAxis);
                var widePartFinalPlane = new Plane(point - sign * newPlane.ZAxis * (14.3 - thickness / 2) + guideOffsetVector, newPlane.XAxis, newPlane.YAxis);
                var widePartTranform = Transform.PlaneToPlane(widePartReferencePlane, widePartFinalPlane);

                var index = widePartIndexes[i];
                var wideShape = rawShapes[index];

                wideShape.Transform(widePartTranform);

                shapes.Add(wideShape);
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