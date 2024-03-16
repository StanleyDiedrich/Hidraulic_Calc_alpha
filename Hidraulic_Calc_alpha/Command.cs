using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace Hidraulic_Calc_alpha 
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        static AddInId AddInId = new AddInId(new Guid("D570C57B-B981-402F-BB09-16D294EA3CB4"));
        private SelectedSystems SelectedSystems {  get; set; }

        public Element FindStartConnector(Document doc, string selectedsystem)
        {
            Element startElement = null;
            IList<Element> virtualequipments = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType().ToElements();
            foreach (Element virtualequipment in virtualequipments)
            {
                FamilyInstance familyInstance = virtualequipment as FamilyInstance;
                string start = "1";
                if (familyInstance != null)
                {
                    if (familyInstance.LookupParameter("Имя системы").AsString().Contains( selectedsystem))
                    {
                        string check = familyInstance.LookupParameter("Старт_расчета").AsString();
                        if (check == start)
                        {
                            startElement = virtualequipment;
                        }
                    }

                }
            }
            return startElement;
        }
        public string GetSystemType(Element startelement)
        {
            string systemtype = "";
            if (startelement != null)
            {
                FamilyInstance fI = startelement as FamilyInstance;
                MEPModel mepModel = fI.MEPModel;
                ConnectorSet connectorSet = mepModel.ConnectorManager.Connectors;
                foreach (Connector connector in connectorSet)
                {
                    systemtype = connector.PipeSystemType.ToString();

                }


            }
            return systemtype;
        }
        public ElementId FindNextElement(Document doc, ElementId elementId, List<ElementId> foundedelements, string systemtype)
        {

            ElementId ownerId = elementId;
            Element element = doc.GetElement(ownerId);
            Element foundedElement = null;
            MEPModel mepModel = null;
            ConnectorSet connectorSet = null;
            ElementId foundedelementId = null;
            double maxvolume = 0;
            try
            {

                if (element is FamilyInstance )
                {
                    FamilyInstance FI = element as FamilyInstance;
                    mepModel = FI.MEPModel;
                    connectorSet = mepModel.ConnectorManager.Connectors;

                }
               
                    
                if (element is Pipe)
                {
                   Pipe pipe = element as Pipe;
                    connectorSet = pipe.ConnectorManager.Connectors;
                }
                if (element is FlexPipe)
                {
                    FlexDuct pipe = element as FlexDuct;
                    connectorSet = pipe.ConnectorManager.Connectors;
                }

                foreach (Connector connector in connectorSet)
                {
                    ConnectorSet nextconnectors = connector.AllRefs;
                    foreach (Connector nextconnector in nextconnectors)
                    {

                        if (doc.GetElement(nextconnector.Owner.Id) is PipingSystem)
                        {

                            continue;
                        }
                        else if (nextconnector.Owner.Id == ownerId)
                        {

                            continue;
                        }
                        else if (nextconnectors.Size <1)
                        { continue; }

                        /*else if (nextconnectors.Size==1)
                        {
                            continue;
                        }*/



                        else
                        {
                            if (nextconnector.Domain == Domain.DomainHvac || nextconnector.Domain== Domain.DomainPiping)
                            {
                                if (systemtype == "SupplyHydronic")
                                {
                                    if (nextconnector.Direction is FlowDirectionType.Out && nextconnector.Flow != 0)
                                    {
                                        foundedelementId = nextconnector.Owner.Id;
                                    }
                                    else if (nextconnector.Direction is FlowDirectionType.Bidirectional && nextconnector.Flow != 0)
                                    {
                                        if (!nextconnector.Owner.Id.Equals(ownerId) || !foundedelements.Contains(nextconnector.Owner.Id))
                                        {
                                            foundedelementId = nextconnector.Owner.Id;
                                        }
                                        else
                                        { continue; }

                                    }


                                }
                                if (systemtype == "ReturnHydronic")
                                {
                                    if (nextconnector.Direction is FlowDirectionType.In && nextconnector.Flow != 0)
                                    {
                                        foundedelementId = nextconnector.Owner.Id;
                                    }
                                    else if (nextconnector.Direction is FlowDirectionType.Bidirectional && nextconnector.Flow != 0)
                                    {
                                        if (!nextconnector.Owner.Id.Equals(ownerId) || !foundedelements.Contains(nextconnector.Owner.Id))
                                        {
                                            foundedelementId = nextconnector.Owner.Id;
                                        }
                                        else
                                        { continue; }

                                    }


                                }

                            }
                            else
                            { continue; }

                            


                        }







                    }
                }



            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"{ex.ToString()} \n {element.Id} не отработал ");

            }

            return foundedelementId;

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Form1 window = new Form1(doc);
            window.ShowDialog();
            SelectedSystems selectedSystems = new SelectedSystems();
            selectedSystems.preparedsystems = window._selectedsystems.preparedsystems;
            string text = string.Empty;
            /*foreach (string a in selectedSystems.preparedsystems)
            {
                if (!text.Contains(a))
                {
                    text += a + '\n';
                }


            }
            TaskDialog.Show("Смотри че", $"{text}\n");*/
            var count = 0;
           Dictionary<ElementId, string > virtualequipments = new Dictionary<ElementId, string>();
            foreach (string system in selectedSystems.preparedsystems)
            {
                try
                {
                    var element = FindStartConnector(doc, system);
                    virtualequipments.Add(element.Id, system);
                }
                catch
                {
                    TaskDialog.Show("Ошибка стартового элемента", "Проверь параметр Старт_расчета. В поле укажи 1. Если не сработало, проверь совпадает ли число выбранных систем с числом стартовых коннекторов ");
                }
                
                
            }
            foreach (var virtualequipment in virtualequipments)
            {
                string selectedsystem = virtualequipment.Value.ToString();
                ElementId elementId = virtualequipment.Key;


                List<ElementId> foundedelements = new List<ElementId>();
                foundedelements.Add(elementId);
                Element element = doc.GetElement(elementId);
                string systemtype = GetSystemType(element);
                var foundedelement = FindNextElement(doc, elementId, foundedelements, systemtype);
                foundedelements.Add(foundedelement);
                int index = foundedelements.Count - 1;
                ElementId nextelement = null;

                ElementId f = null;
                string name = "";
                int counter = 0;
                try
                {
                    do
                    {

                        nextelement = foundedelements.Last();

                        f = FindNextElement(doc, nextelement, foundedelements, systemtype);
                        if (f != null)
                        {


                            if (!foundedelements.Contains(f))
                            {

                                if (f != nextelement)
                                {
                                    foundedelements.Add(f);
                                }
                                else
                                {
                                    continue;
                                }
                            }



                        }
                        else
                        {
                            break;

                        }






                    }
                    while (f != nextelement || f == null);
                    //TaskDialog.Show("Res", selectedelement.Id.ToString());


                }
                catch (Exception ex)
                {

                }
                int number = 0;
                string letter = "";

                double prev_area = 0;
                double prev_flow = 0;

                string text2 = string.Empty;
               
                foreach (var foundedelement2 in foundedelements)
                {
                    string a = foundedelement2.IntegerValue.ToString();
                    text2 += a+"\n";

                    if (foundedelement2 != null)
                    {
                        Element element2 = doc.GetElement(foundedelement2);
                        if (foundedelement2 == foundedelements.First())
                        {
                            letter = "_a";
                        }
                        else
                        {
                            letter = "";
                        }
                        if (element is FamilyInstance)
                        {
                            FamilyInstance familyInstance = element as FamilyInstance;
                            MEPModel mepmodel = familyInstance.MEPModel;
                            ConnectorSet connectorSet = mepmodel.ConnectorManager.Connectors;
                            double area = 0;
                            double flow = 0;
                            foreach (Connector connector in connectorSet)
                            {

                                if (connector.Shape == ConnectorProfileType.Round)
                                {
                                    area = Math.PI * Math.Pow(connector.Radius, 2);
                                    flow = connector.Flow;
                                }
                                else
                                {
                                    area = connector.Width * connector.Height;
                                    flow = connector.Flow;
                                }



                            }
                            if (prev_area != area || prev_flow != flow)
                            {
                                number++;
                                prev_area = area;
                                prev_flow = flow;

                            }
                            string resstring = $"{selectedsystem}_MainWay_{number}_{letter}";

                            using (Transaction t = new Transaction(doc, "MainBranch"))
                            {
                                try
                                {
                                    t.Start();

                                    familyInstance.LookupParameter("ADSK_Группирование").Set(resstring);
                                    t.Commit();
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        if (element is Pipe)
                        {
                            Pipe familyInstance = element as Pipe;

                            ConnectorSet connectorSet = familyInstance.ConnectorManager.Connectors;
                            double area = 0;
                            double flow = 0;
                            foreach (Connector connector in connectorSet)
                            {

                                if (connector.Shape == ConnectorProfileType.Round)
                                {
                                    area = Math.PI * Math.Pow(connector.Radius, 2);
                                    flow = connector.Flow;
                                }
                                else
                                {
                                    area = connector.Width * connector.Height;
                                    flow = connector.Flow;
                                }


                            }
                            if (prev_area != area || prev_flow != flow)
                            {
                                number++;
                                prev_area = area;
                                prev_flow = flow;

                            }
                            string resstring = $"{selectedsystem}_MainWay_{number}{letter}";

                            using (Transaction t = new Transaction(doc, $"MainBranch"))
                            {
                                try
                                {
                                    t.Start();
                                    familyInstance.LookupParameter("ADSK_Группирование").Set(resstring);
                                    t.Commit();
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }
                        }
                    }


                }
                TaskDialog.Show($"Вот че ", text2);
            }

            return Result.Succeeded;
        }
    }
}
