using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Relations;
using OALProgramControl;

namespace Visualization.Animation
{
    public class PlantUMLBuilder
    {
        private StringBuilder umlBuilder;

        public PlantUMLBuilder()
        {
            umlBuilder = new StringBuilder();
        }

        public void PrintDiagram() {
            Debug.Log("[PLANT_UML]" + GetDiagram());
        }

        public string GetDiagram()
        {
            umlBuilder = new StringBuilder();
            // Automaticky vytvoriť celý diagram
            StartDiagram();
            AddClassesFromAnimation();
            AddRelationsFromAnimation();
            EndDiagram();

            return umlBuilder.ToString();
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


                currentClass.Attributes ??= new List<Visualization.ClassDiagram.ClassComponents.Attribute>();
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
            var relationList = Animation.Instance.classDiagram.GetRelationList(); //TODO Preptočiť všetko na "Animation.Instance.CurrentProgramInstance"

            //adding relations from OALProgram
            OALProgram programInstance = Animation.Instance.CurrentProgramInstance;

            var relationshipSpace = programInstance.RelationshipSpace;
            var allRepationships = relationshipSpace.GetAllRelationshipsTupples();

            foreach (var tuple in allRepationships)
            {
                var relation = relationshipSpace.GetRelationshipByName(tuple.Item1);
                var relName = relation.RelationshipName;
                var fromClass = relation.FromClass;
                var toClass = relation.ToClass;
                Debug.Log("[PLANT_UML] relation = " + relName + ", from = " + fromClass + ", to = " + toClass);
            }
        
            //adding relations from ClassDiagram
            foreach (var relation in relationList)
            {
                var source = relation.SourceModelName.Replace(" ", "_");
                var target = relation.TargetModelName.Replace(" ", "_");
                var relationType = relation.PropertiesEaType;
                var relationDirection = relation.PropertiesDirection; //nechať zakomentovaný swtich, !!! všetko asociácia !!! 
				//Ak od seba triedy dedia, tak typ Generalization| dedenie CDClass ( - super class)
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
                    "Generalization" => "<|--", //Naopak - TODO
                    "Implements" => "<|..",
                    _ => "--"
                };


                umlBuilder.AppendLine($"{source} {relationArrow} {target}");
            }
        }

    }
}
