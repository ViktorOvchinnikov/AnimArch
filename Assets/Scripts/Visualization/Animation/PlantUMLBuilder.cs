using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Relations;

namespace Visualization.Animation
{
    public class PlantUMLBuilder
    {
        private StringBuilder umlBuilder;

        public PlantUMLBuilder()
        {
            umlBuilder = new StringBuilder();
        }

        public string GetDiagram()
        {
            // Automaticky vytvoriť celý diagram
            StartDiagram();
            AddClassesFromAnimation();
            AddRelationsFromAnimation();
            EndDiagram();

            // Výpis do konzolky
            var diagram = umlBuilder.ToString();
            Debug.Log(diagram);

            return diagram;
        }

        private void StartDiagram()
        {
            umlBuilder.Clear();
            umlBuilder.AppendLine("@startuml");
        }

        private void EndDiagram()
        {
            umlBuilder.AppendLine("@enduml");
        }

        private void AddClassesFromAnimation()
        {
            var classList = Animation.Instance.classDiagram.GetClassList();

            foreach (var currentClass in classList)
            {
                var className = currentClass.Name.Replace(" ", "_");
                umlBuilder.AppendLine($"class {className} {{");


                currentClass.Attributes ??= new List<Attribute>();
                foreach (var attribute in currentClass.Attributes)
                {
                    umlBuilder.AppendLine($"    + {attribute.Name} : {attribute.Type}");
                }


                currentClass.Methods ??= new List<Method>();
                foreach (var method in currentClass.Methods)
                {
                    string arguments = "";
                    if (method.arguments != null && method.arguments.Count > 0)
                    {
                        arguments = string.Join(", ", method.arguments);
                    }
                    umlBuilder.AppendLine($"    + {method.Name}({arguments}) : {method.ReturnValue}");
                }

                umlBuilder.AppendLine("}"); 
            }
        }

        private void AddRelationsFromAnimation()
        {
            var relationList = Animation.Instance.classDiagram.GetRelationList();

            foreach (var relation in relationList)
            {
                var source = relation.SourceModelName.Replace(" ", "_");
                var target = relation.TargetModelName.Replace(" ", "_");
                var relationType = relation.PropertiesEaType;
                var relationDirection = relation.PropertiesDirection;

                string relationArrow = relationType switch
                {
                    "Realisation" => "<|..",
                    "Association" => relationDirection switch
                    {
                        "Source -> Destination" => "-->",
                        "Destination -> Source" => "<--",
                        "Bi-Directional" => "<-->",
                        _ => "--"
                    },
                    "Dependency" => "..>",
                    "Generalization" => "<|--",
                    "Implements" => "<|..",
                    _ => "--"
                };


                umlBuilder.AppendLine($"{source} {relationArrow} {target}");
            }
        }

    }
}
