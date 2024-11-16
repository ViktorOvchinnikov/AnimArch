using System.Collections.Generic;
using System.IO;
using Parsers;
using UMSAGL.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Visualization.Animation;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Editors;
using Visualization.ClassDiagram.Relations;
using Visualization.UI;

namespace Visualization.ClassDiagram
{
    public class ClassDiagramBuilder : IClassDiagramBuilder
    {
        protected void ParseData()
        {
            ClassDiagramLoaderBase diagramLoader = new ClassDiagramLoaderBase();
            var parser = diagramLoader.LoadDiagram(AnimationData.Instance.GetDiagramPath());
            var classList = parser.ParseClasses() ?? new List<Class>();
            var relationList = parser.ParseRelations() ?? new List<Relation>();

            if (classList.Count == 0)
                return;
            //Parse all data to our List of "Class" objects

            var plantUmlBuilder = new System.Text.StringBuilder();
            plantUmlBuilder.AppendLine("@startuml");

            // Spracovanie tried
            foreach (var currentClass in classList)
            {

                currentClass.Name = currentClass.Name.Replace(" ", "_");
                plantUmlBuilder.AppendLine($"class {currentClass.Name} {{");

                UIEditorManager.Instance.mainEditor.CreateNode(currentClass);
                var classInDiagram = DiagramPool.Instance.ClassDiagram.FindClassByName(currentClass.Name);
                if (classInDiagram.ClassInfo == null)
                    continue;
                
                currentClass.Attributes ??= new List<Attribute>();
                foreach (var attribute in currentClass.Attributes)
                {
                    plantUmlBuilder.AppendLine($"    + {attribute.Name} : {attribute.Type}");
                    UIEditorManager.Instance.mainEditor.AddAttribute(currentClass.Name, attribute);
                }


                currentClass.Methods ??= new List<Method>();
                foreach (var method in currentClass.Methods)
                {
                    plantUmlBuilder.AppendLine($"    + {method.Name}() : {method.ReturnValue}");
                    UIEditorManager.Instance.mainEditor.AddMethod(currentClass.Name, method);
                }
                
                currentClass.Top *= -1;

                plantUmlBuilder.AppendLine("}"); 

                
                
            }


            foreach (var relation in relationList)
            {
                plantUmlBuilder.AppendLine($"{relation.SourceModelName} --> {relation.TargetModelName}");
                UIEditorManager.Instance.mainEditor.CreateRelation(relation);
            }

            plantUmlBuilder.AppendLine("@enduml");


            Debug.Log(plantUmlBuilder.ToString());
        }


        public override void CreateGraph()
        {
            UIEditorManager.Instance.mainEditor.ClearDiagram();
            var graphGo = GameObject.Instantiate(DiagramPool.Instance.graphPrefab);
            graphGo.name = "Graph";

            DiagramPool.Instance.ClassDiagram.graph = graphGo.GetComponent<Graph>();
            DiagramPool.Instance.ClassDiagram.graph.nodePrefab = DiagramPool.Instance.classPrefab;
            GameObject.Find("DiagramPanel/Buttons/Edit").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("AnimationPanel/Buttons/Load").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("AnimationPanel/Buttons/Create").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("MaskingPanel/Buttons/Load").GetComponentInChildren<Button>().interactable = true;
        }

        //Auto arrange objects in space
        public void RenderClassesAuto()
        {
            DiagramPool.Instance.ClassDiagram.graph.Layout();
        }

        //Set layout as close as possible to EA layout
        private void RenderClassesManual()
        {
            foreach (var classInDiagram in DiagramPool.Instance.ClassDiagram.Classes)
            {
                var x = classInDiagram.ParsedClass.Left * 1.25f;
                var y = classInDiagram.ParsedClass.Top * 1.25f;
                var z = classInDiagram.VisualObject.GetComponent<RectTransform>().position.z;
                visualEditor.SetPosition(classInDiagram.ParsedClass.Name, new Vector3(x, y, z));
            }
        }

        public override void LoadDiagram()
        {
            CreateGraph();
            MakeNetworkedGraph();
            var k = 0;
            // A trick used to skip empty diagrams in XMI file from EA
            while (DiagramPool.Instance.ClassDiagram.Classes.Count < 1 && k < 10)
            {
                ParseData();
                k++;
                AnimationData.Instance.diagramId++;
            }

            RenderClassesManual();
        }
    }
}
