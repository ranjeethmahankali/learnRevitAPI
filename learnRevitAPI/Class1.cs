using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace dAlchemy{
    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_IOSModelGroups);
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }

    public class ribbonPanel : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = application.CreateRibbonPanel("dAlchemy");

            //get the path of this plugin assembly
            string path = Assembly.GetExecutingAssembly().Location;

            //create data to attach to a button later.
            PushButtonData data = new PushButtonData("pluginCommand","Command",path,"dAlchemy.FilterClass");
            //create a button with the above data
            PushButton button = panel.AddItem(data) as PushButton;
            //this is the tooltip for the button
            button.ToolTip = "This is just a test plugin";

            //adding another button in the same way
            /*
            PushButtonData data2 = new PushButtonData("secondCommand", "Filter", path, "dAlchemy.FilterClass");
            PushButton button2 = panel.AddItem(data2) as PushButton;
            button2.ToolTip = "This is just a test plugin";
            */
            //this is how you load an icon for that button
            //Uri uriImage = new Uri(@"D:\Sample\HelloWorld\bin\Debug\39-Globe_32x32.png");
            //BitmapImage largeImage = new BitmapImage(uriImage);
            //button.LargeImage = largeImage;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            //do any cleanup if required
            return Result.Succeeded;
        }
    }

    //this is the command that came with the demo code
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class PlaceGroup : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            //Define a Reference object to accept the pick result.
            Reference pickedRef = null;

            //Pick a group
            Selection sel = uiApp.ActiveUIDocument.Selection;
            //this filter makes sure the user only selects groups
            GroupPickFilter filter = new GroupPickFilter();
            pickedRef = sel.PickObject(ObjectType.Element, filter, "Please select a group");
            Element elem = doc.GetElement(pickedRef);
            Group group = null;
            //trying to cast the selected element to a group and complaining about it if it fails
            try
            {
                group = elem as Group;
            }
            catch (Exception e)
            {
                message = "Failed to convert the selection to group: " + e.Message;
                elements.Insert(elem);
                return Result.Failed;
            }

            //Pick a point
            XYZ point = sel.PickPoint("Please pick a point to place group");

            //Place the group
            Transaction trans = new Transaction(doc);
            trans.Start("Lab");
            doc.Create.PlaceGroup(point, group.GroupType);
            trans.Commit();

            return Result.Succeeded;
        }
    }
    
    //this is the command that came with the demo code
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class FilterClass : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> walls = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            GeometryElement geomElem = walls[0].get_Geometry(new Options());
            foreach(GeometryObject obj in geomElem)
            {
                Solid wallSolid = obj as Solid;
                Curve cur = wallSolid.Edges.get_Item(0).AsCurve();
                cur.GetEndPoint(0);
            }

            return Result.Succeeded;
        }
    }
}