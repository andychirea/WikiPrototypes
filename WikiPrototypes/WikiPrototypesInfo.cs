using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace WikiPrototypes
{
    public class WikiPrototypesInfo : GH_AssemblyInfo
    {
        public override string Name => "WikiPrototypes";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("2eddad45-d23e-40c9-8e5c-48ed1efde7e0");

        //Return a string identifying you or your company.
        public override string AuthorName => "Andrei Chirea";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "chirea.andrei@gmail.com";
    }
}