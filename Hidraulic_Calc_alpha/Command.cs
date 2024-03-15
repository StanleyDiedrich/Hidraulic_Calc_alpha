using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Hidraulic_Calc_alpha 
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        static AddInId AddInId = new AddInId(new Guid("D570C57B-B981-402F-BB09-16D294EA3CB4"));
        private SelectedSystems SelectedSystems {  get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Form1 window = new Form1(doc);
            window.ShowDialog();
            SelectedSystems selectedSystems = new SelectedSystems();
            selectedSystems.preparedsystems = window._selectedsystems.preparedsystems;
            string text = string.Empty;
            foreach (string a in selectedSystems.preparedsystems)
            {
                if (!text.Contains(a))
                {
                    text += a + '\n';
                }


            }
            TaskDialog.Show("Смотри че", $"{text}\n");

            return Result.Succeeded;
        }
    }
}
