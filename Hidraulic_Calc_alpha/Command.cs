using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using static Hidraulic_Calc_alpha.Command;

namespace Hidraulic_Calc_alpha
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        static AddInId AddInId = new AddInId(new Guid("D570C57B-B981-402F-BB09-16D294EA3CB4"));
        private SelectedSystems SelectedSystems { get; set; }

        public class Tees
        {
            public ElementId ElementId { get; set; }
            public string Level { get; set; }
            public string SystemType { get; set; }

            public Tees(ElementId elementId, string level, string systemtype)
            {
                ElementId = elementId;
                Level = level;
                SystemType = systemtype;
            }

        }
        public Dictionary<ElementId, string> FindCollectors (Document doc, string selectedsystem)
        {
            Dictionary<ElementId, string> selectedcollectors = new Dictionary<ElementId, string>();
            Element startElement = null;
            IList<Element> virtualequipments = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeAccessory).WhereElementIsNotElementType().ToElements();
            foreach (Element virtualequipment in virtualequipments)
            {
                Family family = virtualequipment as Family;
                FamilyInstance familyInstance = virtualequipment as FamilyInstance;
                FamilySymbol familySymbol = familyInstance.Symbol;
                ElementType elementType = familySymbol as ElementType;
                string familyName = elementType.FamilyName;
                string start = "";
                if (familyInstance != null)
                {
                    if (familyName.Contains("Коллектор") || familyName.Contains("Этажный")|| familyName.Contains("470"))
                    {
                        if (familyInstance.LookupParameter("ADSK_Группирование").AsString()==null|| familyInstance.LookupParameter("ADSK_Группирование").AsString().Equals("0"))
                        
                        if (!selectedcollectors.ContainsKey(familyInstance.Id))
                        {
                            selectedcollectors.Add(familyInstance.Id, selectedsystem);

                        }
                    }

                }
            }
            return selectedcollectors;

        }


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
                    if (familyInstance.LookupParameter("Имя системы").AsString().Contains(selectedsystem))
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
        public Dictionary<ElementId, string> FindSecondaryStartConnector(Document doc, string selectedsystem)
        {
            Dictionary<ElementId, string> selectedstartconnectors = new Dictionary<ElementId, string>();
            Element startElement = null;
            IList<Element> virtualequipments = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType().ToElements();
            foreach (Element virtualequipment in virtualequipments)
            {
                FamilyInstance familyInstance = virtualequipment as FamilyInstance;
                string start = "2";
                if (familyInstance != null)
                {
                    if (familyInstance.LookupParameter("Имя системы").AsString().Contains(selectedsystem))
                    {
                        string check = familyInstance.LookupParameter("Старт_расчета").AsString();
                        if (check == start && !selectedstartconnectors.ContainsKey(familyInstance.Id))
                        {
                            selectedstartconnectors.Add(familyInstance.Id, selectedsystem);

                        }
                    }

                }
            }
            return selectedstartconnectors;
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
        public class NextElement
        {
             public ElementId Id { get; set; }
            public bool Reverse { get; set; }
            public List<ElementId> NextElements { get; set; }
            public  string Parameter { get; set; }

            public NextElement (ElementId elementId,List <ElementId> nextelements,  string parameter=null, bool reverse = false)
            {
                Id = elementId;
                NextElements = nextelements;
                Reverse = reverse;
                Parameter = parameter;
            }
        }
        public NextElement CheckCollector (Document doc, ElementId elementId)
        {
           
            Element element = doc.GetElement(elementId);
           
            List<ElementId> nextelements = new List<ElementId>();
            NextElement nextElement = new NextElement(elementId, nextelements);
            FamilyInstance familyInstance = element as FamilyInstance;
            MEPModel mEPModel = familyInstance.MEPModel;
            ConnectorSet connectorSet = mEPModel.ConnectorManager.Connectors;
            foreach (Connector connector in connectorSet)
            {
                string systemtype = connector.PipeSystemType.ToString();

                ConnectorSet nextconnectors = connector.AllRefs;
                    foreach (Connector nextconnector in nextconnectors)
                    {

                        if (doc.GetElement(nextconnector.Owner.Id) is PipingSystem)
                        {

                            continue;
                        }
                        else if (nextconnector.Owner.Id == elementId)
                        {

                            continue;
                        }
                        else if (nextconnectors.Size < 1)
                        { continue; }

                        else
                        {
                            if (nextconnector.Domain == Domain.DomainHvac || nextconnector.Domain == Domain.DomainPiping)
                            {
                                if (nextconnector.Owner != null)
                                {
                                    
                                    // тут про наличие параметра
                                    if (nextconnector.Owner.LookupParameter("ADSK_Группирование").AsString()==null  || nextconnector.Owner.LookupParameter("ADSK_Группирование").AsString().Equals("0"))
                                    {
                                    /* if (connector.PipeSystemType.ToString().Equals(systemtype))
                                     {*/
                                        if (nextconnector.Direction is FlowDirectionType.Out)
                                       
                                        {
                                            if (systemtype == "SupplyHydronic")
                                            {

                                                    nextelements.Add(nextconnector.Owner.Id);
                                                
                                            }
                                                
                                        }
                                        if (nextconnector.Direction is FlowDirectionType.In)
                                        
                                        {
                                            if (systemtype == "ReturnHydronic")
                                            {
                                                    nextelements.Add(nextconnector.Owner.Id);
                                               


                                            }
                                                
                                        }
                                      //  }
                                        
                                    }
                                    else
                                    {
                                        continue;

                                    }
                                    

                                }
                                
                            }
                        }


                    }
                
            }
            nextElement.Id = elementId;
            nextElement.Reverse = true;
            nextElement.NextElements = nextelements;
            

            return nextElement;
        }
        public ElementId FindNextElement(Document doc, ElementId elementId, Dictionary<ElementId, string> foundedelements, string systemtype)
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

                if (element is FamilyInstance)
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
                    double connectorflow = connector.Flow;
                    if (connector.PipeSystemType.ToString().Equals(systemtype))
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
                            else if (nextconnectors.Size < 1)
                            { continue; }

                            /*else if (nextconnectors.Size==1)
                            {
                                continue;
                            }*/



                            else
                            {



                                if (nextconnector.Domain == Domain.DomainHvac || nextconnector.Domain == Domain.DomainPiping)
                                {
                                    double nextconnectorfflow = nextconnector.Flow;
                                    if (systemtype == "SupplyHydronic")
                                    {
                                        if (nextconnector.Direction is FlowDirectionType.Out)
                                        {
                                            if (nextconnectorfflow > connectorflow || nextconnectorfflow == connectorflow)
                                            {
                                                foundedelementId = nextconnector.Owner.Id;
                                            }


                                        }
                                        else if (nextconnector.Direction is FlowDirectionType.Bidirectional && nextconnector.Flow != 0)
                                        {
                                            if (!nextconnector.Owner.Id.Equals(ownerId) || !foundedelements.ContainsKey(nextconnector.Owner.Id))
                                            {
                                                foundedelementId = nextconnector.Owner.Id;
                                            }
                                            else
                                            { continue; }

                                        }


                                    }
                                    if (systemtype == "ReturnHydronic")
                                    {
                                        if (nextconnector.Direction is FlowDirectionType.In)
                                        {


                                            foundedelementId = nextconnector.Owner.Id;



                                        }
                                        else if (nextconnector.Direction is FlowDirectionType.Bidirectional)
                                        {
                                            if (!nextconnector.Owner.Id.Equals(ownerId) || !foundedelements.ContainsKey(nextconnector.Owner.Id))
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
                    else { continue; }



                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"{ex.ToString()} \n {element.Id} не отработал ");

            }

            return foundedelementId;

        }
        public ElementId ReverseFindNextElement(Document doc, ElementId elementId, Dictionary<ElementId, string> foundedelements, string systemtype)
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

                if (element is FamilyInstance)
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
                    double connectorflow = connector.Flow;
                    if (connector.PipeSystemType.ToString().Equals(systemtype))
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
                            else if (nextconnectors.Size < 1)
                            { continue; }

                            /*else if (nextconnectors.Size==1)
                            {
                                continue;
                            }*/



                            else
                            {



                                if (nextconnector.Domain == Domain.DomainHvac || nextconnector.Domain == Domain.DomainPiping)
                                {
                                    double nextconnectorfflow = nextconnector.Flow;
                                    if (systemtype == "ReturnHydronic")
                                    {
                                        if (nextconnector.Direction is FlowDirectionType.Out)
                                        {
                                            if (nextconnectorfflow > connectorflow || nextconnectorfflow == connectorflow)
                                            {
                                                foundedelementId = nextconnector.Owner.Id;
                                            }


                                        }
                                        else if (nextconnector.Direction is FlowDirectionType.Bidirectional && nextconnector.Flow != 0)
                                        {
                                            if (!nextconnector.Owner.Id.Equals(ownerId) || !foundedelements.ContainsKey(nextconnector.Owner.Id))
                                            {
                                                foundedelementId = nextconnector.Owner.Id;
                                            }
                                            else
                                            { continue; }

                                        }


                                    }
                                    if (systemtype == "SupplyHydronic")
                                    {
                                        if (nextconnector.Direction is FlowDirectionType.In)
                                        {


                                            foundedelementId = nextconnector.Owner.Id;



                                        }
                                        else if (nextconnector.Direction is FlowDirectionType.Bidirectional)
                                        {
                                            if (!nextconnector.Owner.Id.Equals(ownerId) || !foundedelements.ContainsKey(nextconnector.Owner.Id))
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
                    else { continue; }



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
            List<Dictionary<ElementId, string>> listoffoundedelements = new List<Dictionary<ElementId, string>>();

            Dictionary<ElementId, string> virtualequipments = new Dictionary<ElementId, string>();
            List<Dictionary<ElementId, string>> secondaryelements = new List<Dictionary<ElementId, string>>();
            Dictionary <ElementId, string> collectors = new Dictionary<ElementId, string>();
            foreach (string system in selectedSystems.preparedsystems)
            {
                try
                {
                    var element = FindStartConnector(doc, system);
                    virtualequipments.Add(element.Id, system);
                    var secondaryelement = FindSecondaryStartConnector(doc, system);
                    secondaryelements.Add(secondaryelement);
                    collectors = FindCollectors(doc, system);
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


                Dictionary<ElementId, string> foundedelements = new Dictionary<ElementId, string>();
                foundedelements.Add(elementId, selectedsystem);
                Element element = doc.GetElement(elementId);
                string systemtype = GetSystemType(element);
                var foundedelement = FindNextElement(doc, elementId, foundedelements, systemtype);
                foundedelements.Add(foundedelement, selectedsystem);
                int index = foundedelements.Count - 1;
                ElementId nextelement = null;

                ElementId f = null;
                string name = "";
                int counter = 0;
                try
                {
                    do
                    {

                        nextelement = foundedelements.Last().Key;

                        f = FindNextElement(doc, nextelement, foundedelements, systemtype);
                        if (f != null)
                        {


                            if (!foundedelements.ContainsKey(f))
                            {

                                if (f != nextelement)
                                {
                                    foundedelements.Add(f, selectedsystem);
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
                    listoffoundedelements.Add(foundedelements);
                    //TaskDialog.Show("Res", selectedelement.Id.ToString());


                }
                catch (Exception ex)
                {

                }

            }
            List<List<Tees>> ListOftees = new List<List<Tees>>();
            List<Tees> tees1 = new List<Tees>();
            foreach (var foundedelements in listoffoundedelements)
            {
                int number = 0;
                string letter = "";

                double prev_area = 0;
                double prev_flow = 0;

                string text2 = string.Empty;
                foreach (var foundedelement2 in foundedelements)
                {
                    string selectedsystem = foundedelement2.Value;
                    string a = foundedelement2.Key.IntegerValue.ToString();
                    text2 += a + "\n";

                    if (foundedelement2.Key != null)
                    {
                        Element element2 = doc.GetElement(foundedelement2.Key);
                        if (foundedelement2.Key == foundedelements.First().Key)
                        {
                            letter = "_a";
                        }
                        else
                        {
                            letter = "";
                        }
                        if (element2 is FamilyInstance)
                        {

                            FamilyInstance familyInstance = element2 as FamilyInstance;

                            MEPModel mepmodel = familyInstance.MEPModel;
                           /* MechanicalFitting fitting = mepmodel as MechanicalFitting;
                            if (fitting != null && fitting.PartType == PartType.Tee)
                            {
                                Tees tees = new Tees(foundedelement2.Key, doc.GetElement(element2.LevelId).Name, foundedelement2.Value);
                                if (!tees1.Contains(tees))
                                {
                                    tees1.Add(tees);
                                }


                            }*/
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
                        if (element2 is Pipe)
                        {
                            Pipe familyInstance = element2 as Pipe;

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
                    ListOftees.Add(tees1);

                }
            }
            
            List<Dictionary<ElementId,string>> listoffoundedelements2 = new List<Dictionary<ElementId, string>>();
            foreach (var secondaryelement in secondaryelements)
            {
                foreach (var secelement in secondaryelement)
                {
                    string selectedsystem =secelement.Value.ToString();
                    Dictionary<ElementId, string> foundedelements = new Dictionary<ElementId, string>();

                    Element element = doc.GetElement(secelement.Key);
                    string systemtype = GetSystemType(element);
                    var foundedelement = FindNextElement(doc, secelement.Key, foundedelements, systemtype);
                    foundedelements.Add(foundedelement, secelement.Value);

                    ElementId nextelement = null;

                    ElementId f = null;
                    string name = "";
                    int counter = 0;
                    
                    try
                    {
                        do
                        {
                            int step = 0;
                            nextelement = foundedelements.Last().Key;

                            f = FindNextElement(doc, nextelement, foundedelements, systemtype);

                            if (f != null)
                            {
                                if (!foundedelements.ContainsKey(f))
                                {

                                    if (f != nextelement)
                                    {
                                        foundedelements.Add(f, selectedsystem);
                                    }

                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            /*foreach ( var foundedelements4 in  listoffoundedelements)
                            {
                                foreach ( var foundedelement4 in  foundedelements4.Keys)
                                {
                                    if (f==foundedelement4)
                                    { break; }
                                }
                            }*/
                            /*string param = null;
                            Element selelem = doc.GetElement(f);
                            if (selelem is FamilyInstance)
                            {
                                FamilyInstance fI = doc.GetElement(f) as FamilyInstance;
                                 param = fI.LookupParameter("ADSK_Группирование").AsString();
                            }
                            if (selelem is Pipe)
                            {
                                Pipe fI = doc.GetElement(f) as Pipe;
                                param = fI.LookupParameter("ADSK_Группирование").AsString();*/



                            /*if (param == null)
                            {*/

                            //}







                            else
                            {
                                break;

                            }

                            /*step++;
                            if (step ==1000)
                            { break; }*/

                            counter++;
                            if (counter == 1000)
                            {
                                TaskDialog.Show("R", $"проверь соединение на элементе { f.IntegerValue.ToString()}");
                                break;
                            }

                        }
                        
                        while (f != nextelement || f == null );
                        listoffoundedelements2.Add(foundedelements);
                    }
                    catch (Exception ex)
                    {

                    }

                }

                int rizer = 1;
                foreach (var listoffoundedelement in listoffoundedelements2)
                {
                   
                    int number = 0;
                    string letter = "";

                    double prev_area = 0;
                    double prev_flow = 0;
                    string text2 = string.Empty;
                    foreach (var element in listoffoundedelement)
                    {

                        string selectedsystem = element.Value;
                        string a = element.Key.IntegerValue.ToString();
                        text2 += a + "\n";

                        if (element.Key != null)
                        {
                            Element element2 = doc.GetElement(element.Key);
                           
                           
                            
                            if (element.Key == listoffoundedelement.First().Key)
                            {
                                letter = "_a";
                            }
                            else
                            {
                                letter = "";
                            }


                            if (element2 is FamilyInstance)
                            {

                                FamilyInstance familyInstance = element2 as FamilyInstance;

                                MEPModel mepmodel = familyInstance.MEPModel;
                                string param = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                try
                                {
                                    if (param.Contains("MainWay") && param != "")
                                    {
                                        break;
                                    }
                                }
                                catch { }
                                
                                
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
                                string resstring = $"{selectedsystem}_BranchWay_{rizer}_{number}_{letter}";

                                using (Transaction t = new Transaction(doc, "Branch"))
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
                            if (element2 is Pipe)
                            {
                                Pipe familyInstance = element2 as Pipe;
                                try
                                {
                                    string param = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                    if (param.Contains("MainWay") && param != "")
                                    {
                                        break;
                                    }
                                }
                                catch { }
                                
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
                                string resstring = $"{selectedsystem}_BranchWay_{rizer}_{number}{letter}";

                                using (Transaction t = new Transaction(doc, $"Branch"))
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
                    rizer++;
                }
                /*uidoc.Selection.SetElementIds(selectedelementIds);

                List<ElementId> selectedelementIds = new List<ElementId>();
                foreach (var listoffoundedelement in listoffoundedelements2)
                {
                    foreach (var element  in listoffoundedelement)
                    {
                        selectedelementIds.Add(element.Key);
                    }

                }
                uidoc.Selection.SetElementIds(selectedelementIds) ;*/
                List<Dictionary<ElementId,string>> listoffoundedelements4 = new List<Dictionary<ElementId,string>>();
                Dictionary<ElementId, string> foundedelements4 = new Dictionary<ElementId, string>();
                foreach (var collector in collectors)
                {
                    /*string selectedsystem = collector.Value.ToString();
                    ElementId elementId = collector.Key;


                   
                    foundedelements4.Add(elementId, selectedsystem);
                    Element element = doc.GetElement(elementId);
                    string systemtype = GetSystemType(element);
                    var foundedelement = FindNextElement(doc, elementId, foundedelements4, systemtype);
                    foundedelements4.Add(foundedelement, selectedsystem);
                    int index = foundedelements4.Count - 1;
                    ElementId nextelement = null;

                    ElementId f = null;
                    string name = "";
                    int counter = 0;
                    try
                    {
                        do
                        {

                            nextelement = foundedelements4.Last().Key;
                            string newparameter = "";
                            f = FindNextElement(doc, nextelement, foundedelements4, systemtype);
                            if (f != null)
                            {
                                
                                
                                if (!foundedelements4.ContainsKey(f))
                                {
                                    Element element4 = doc.GetElement(f);
                                    if (element4 is Pipe)
                                    {
                                        Pipe familyInstance = element4 as Pipe;
                                        string newparam = "";

                                        try
                                        {
                                            var level = doc.GetElement(f).LevelId;
                                            string levelname = doc.GetElement(level).Name;
                                            
                                            string param = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                            if (param.Contains("MainWay") || param.Contains("Branch"))
                                            {
                                                if (param.Contains("MainWay"))
                                                {

                                                    newparam = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                                    string[] strings = newparam.Split('_');
                                                    string system = strings[0];
                                                    string branch = "1";
                                                    string rizer4 = strings[2];
                                                    string branch4 = strings[3];
                                                    newparameter = system + "_" + branch + "_" + rizer4 + "_" + branch4;


                                                }
                                                else
                                                {
                                                    newparam = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                                    string[] strings = newparam.Split('_');
                                                    string system = strings[0];
                                                    string branch = strings[1];
                                                    string rizer4 = strings[2];
                                                    string branch4 = strings[3];
                                                    newparameter = system + "_" + branch + "_" + rizer4 + "_" + branch4;

                                                }

                                               
                                            }
                                        }
                                        catch { }
                                    }
                                    if (f is FamilyInstance)
                                    {
                                        string newparam = "";
                                        FamilyInstance familyInstance = element4 as FamilyInstance;
                                        try
                                        {
                                            var level = doc.GetElement(f).LevelId;
                                            string levelname = doc.GetElement(level).Name;
                                            
                                            string param = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                            if (param.Contains("MainWay") || param.Contains("Branch"))
                                            {
                                                if (param.Contains("MainWay"))
                                                {

                                                    newparam = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                                    string[] strings = newparam.Split('_');
                                                    string system = strings[0];
                                                    string branch = "1";
                                                    string rizer4 = strings[2];
                                                    string branch4 = strings[3];
                                                    newparameter = system + "_" + branch + "_" + rizer4 + "_" + branch4;


                                                }
                                                else
                                                {
                                                    newparam = familyInstance.LookupParameter("ADSK_Группирование").AsString();
                                                    string[] strings = newparam.Split('_');
                                                    string system = strings[0];
                                                    string branch = strings[1];
                                                    string rizer4 = strings[2];
                                                    string branch4 = strings[3];
                                                    newparameter = system + "_" + branch + "_" + rizer4 + "_" + branch4;

                                                }

                                                
                                            }
                                        }
                                        catch { }
                                    }

                                    if (f != nextelement)
                                    {
                                        foundedelements4.Add(f, newparameter);
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




                            counter++;
                            if (counter==1000)
                            { break; }

                        }
                        while (f != nextelement || f == null);
                       // listoffoundedelements4.Add(foundedelements);
                    }
                    catch { }*/
                }

                
            }
            string text5 = string.Empty;
            var checkedConnectors = new List<NextElement>();
            foreach (var fE in collectors)

            {

                ElementId elementId = fE.Key;
                var checkedConnector = CheckCollector(doc, elementId);
                if (checkedConnector != null)
                {

                    checkedConnectors.Add(checkedConnector);
                }



                //string newparam = fE.Value;



            }
           
            foreach (var chConnector in checkedConnectors)
            {
                List<ElementId> chNextElements = chConnector.NextElements;
                string b = "";
                foreach (var chNextElement in chNextElements)
                {
                    string c = chNextElement.ToString();
                    b += c+" ";
                   
                }
                string a = $"{chConnector.Id}: {b} \n";
                text5 += a;

            }

            TaskDialog.Show("Список элементов", text5);
            return Result.Succeeded;
        }
    }
}
