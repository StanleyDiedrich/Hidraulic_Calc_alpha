using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Hidraulic_Calc_alpha 
{
    public class Command : IExternalCommand
    {
        static AddInId AddInId = new AddInId(new Guid("D570C57B-B981-402F-BB09-16D294EA3CB4"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;


            return Result.Succeeded;
        }
    }
}
