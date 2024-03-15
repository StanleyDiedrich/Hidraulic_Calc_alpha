using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace Hidraulic_Calc_alpha
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        public Form1(Autodesk.Revit.DB.Document document)
        {
            
            Doc = document;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                IList<string> systemnames = new List<string>();
                IList<Element> pipes = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsNotElementType().ToElements();
                foreach (Element pipe in pipes)
                {
                    var newpipe = pipe as Pipe;
                    var fI = newpipe as MEPCurve;
                    string system = fI.MEPSystem.Name;
                    if (system != null)
                    {
                        if (!systemnames.Contains(system))
                        {
                            systemnames.Add(system);
                        }
                    }

                }
                IList<string> selectedsystems = new List<string>();
                foreach (string system in systemnames)
                {
                    systemBox.Items.Add(system);
                }
            }
            catch
            {
                TaskDialog.Show("Error", "Loading of systems is failed");
            }
           
        }
    }
}
