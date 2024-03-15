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
        
        public SelectedSystems _selectedsystems { get; set; }
        
        public Form1(Autodesk.Revit.DB.Document document)
        {
            
            Doc = document;
            _selectedsystems = new SelectedSystems();
            
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
                    foreach (Parameter parameter in newpipe.Parameters)
                    {
                        if (parameter.Definition.Name.Equals("Сокращение для системы"))
                        {
                            string system = parameter.AsString();
                            if (system != null)
                            {
                                if (!systemnames.Contains(system))
                                {
                                    systemnames.Add(system);
                                }
                            }
                        }
                    }
                    
                    

                }
               // IList<string> selectedsystems = new List<string>();
                foreach (string system in systemnames)
                {
                    systemBox.Items.Add(system);
                }
            }
            catch
            {
                TaskDialog.Show("Error", "Loading of systems is failed");
            }

            systemBox.SelectionMode = SelectionMode.MultiExtended;
        }

        private void systemBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedSystems selectedSystems = new SelectedSystems();
            selectedSystems.systems = new List<string>();
            var selectedObjectCollection =  systemBox.SelectedItems;

            foreach (Object sOC in selectedObjectCollection)
            {
                SelectedSystem system = new SelectedSystem(sOC.ToString());
                system.selected = true;
                _selectedsystems.systems.Add(system.name.ToString());


            }
            


        }

        private void Start_btn_Click(object sender, EventArgs e)
        {
            //string text = "";
            _selectedsystems.preparedsystems = new List<string>();
            foreach (var sys in _selectedsystems.systems)
            {
                if (!_selectedsystems.preparedsystems.Contains(sys))
                {
                    _selectedsystems.preparedsystems.Add(sys);
                }
            }
            this.Close();
            //var newlist2 = newlist.Distinct();
            /*foreach (string a in newlist)
            {
                if (!text.Contains(a))
                {
                    text += a + '\n';
                }
                
                
            }*/
            //TaskDialog.Show("Смотри че", $"{newlist2.ToString()}\n");
        }
    }
}
