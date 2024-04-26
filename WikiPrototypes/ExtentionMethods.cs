using Rhino.Geometry;
using System.Collections.Generic;

namespace WikiPrototypes
{
    internal static class ExtentionMethods
    {
        public static NurbsCurve[] ToNurbsCurves(this Curve[] curves)
        {
            var nurbsCurves = new NurbsCurve[curves.Length];

            for (int i = 0; i < curves.Length; i++)
            {
                nurbsCurves[i] = curves[i].ToNurbsCurve();
            }

            return nurbsCurves;
        }

        public static NurbsCurve[] ToNurbsCurves(this List<Curve> curves)
        {
            var nurbsCurves = new NurbsCurve[curves.Count];

            for (int i = 0; i < curves.Count; i++)
            {
                nurbsCurves[i] = curves[i].ToNurbsCurve();
            }

            return nurbsCurves;
        }
    }
}