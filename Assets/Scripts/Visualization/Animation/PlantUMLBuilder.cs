using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Relations;
using OALProgramControl;
using System.Linq;

namespace Visualization.Animation
{
    public class PlantUMLBuilder
    {
        private StringBuilder umlBuilder;
		private OALProgram programInstance;

        public PlantUMLBuilder(OALProgram programInstance)
        {
            this.programInstance = programInstance;
            umlBuilder = new StringBuilder();
        }
		public PlantUMLBuilder()
        {
            umlBuilder = new StringBuilder();
        }

        public void PrintDiagram() {
            Debug.Log(new StringBuilder()
                .AppendLine("[PLANT_UML]")
                .Append(GetDiagram())
                .ToString());

        }

        public string GetDiagram()
        {
            umlBuilder = new StringBuilder();
            StartDiagram();
            AddClassesFromAnimation();
            AddRelationsFromAnimation();
            EndDiagram();

            return umlBuilder.ToString();
        }

        private void StartDiagram()
        {
			if (programInstance == null)
            {
				this.programInstance = Animation.Instance.CurrentProgramInstance;
            }
            umlBuilder.Clear();
            umlBuilder.AppendLine("@startuml");
        }

        private void EndDiagram()
        {
            umlBuilder.AppendLine("@enduml");
        }

        private void AddClassesFromAnimation()
        {
            List<CDClass> classList = this.programInstance.ExecutionSpace.Classes;

            foreach (CDClass currentClass in classList)
            {
                string className = currentClass.Name.Replace(" ", "_");
                umlBuilder.AppendLine($"class {className} {{");


                List<CDAttribute> attributes = currentClass.GetAttributes(true);
                if (attributes != null && attributes.Count > 0)
                {
                    foreach (CDAttribute attribute in attributes)
                    {
                        umlBuilder.AppendLine($"    + {attribute.Name} : {attribute.Type}");
                    }
                }


                List<CDMethod> methods = currentClass.GetMethods(true);
                if (methods != null && methods.Count > 0)
                {
                    foreach (CDMethod method in methods)
                    {
                        string arguments = method.Parameters != null && method.Parameters.Count > 0
                            ? string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))
                            : string.Empty;

                        umlBuilder.AppendLine($"    + {method.Name}({arguments}) : {method.ReturnType}");
                    }
                }

                umlBuilder.AppendLine("}");
            }
        }


        private void AddRelationsFromAnimation()
        {
            CDRelationshipPool relationshipSpace = this.programInstance.RelationshipSpace; 
            List<CDClass> classList = this.programInstance.ExecutionSpace.Classes;
            List<(string relationshipName, long fromClassId, long toClassId)> allRelationships = relationshipSpace.GetAllRelationshipsTupples();

            foreach ((string relationshipName, long fromClassId, long toClassId) in allRelationships)
            {
                CDRelationship relation = relationshipSpace.GetRelationshipByName(relationshipName);

                if (relation == null)
                {
                    Debug.LogWarning($"[PLANT_UML] Relationship {relationshipName} not found in RelationshipSpace.");
                    continue;
                }

                string relName = relation.RelationshipName;
                string fromClassName = relation.FromClass?.Replace(" ", "_");
                string toClassName = relation.ToClass?.Replace(" ", "_");

                if (fromClassName == null || toClassName == null)
                {
                    Debug.LogWarning($"[PLANT_UML] Missing class names for relationship {relName}.");
                    continue;
                }


                
                foreach (CDClass from_temp in classList)
                {
                    if (from_temp.Name == relation.FromClass) //Overenie, že sa jedná o triedu z relácie
                    {
                        foreach (CDClass to_temp in classList)
                        {
                            if (to_temp.Name == relation.ToClass)
                            {
                                if (to_temp.SubClasses != null && to_temp.SubClasses.Contains(from_temp))
                                {

                                    umlBuilder.AppendLine($"{from_temp.Name.Replace(" ", "_")} --|> {to_temp.Name.Replace(" ", "_")}");
                                    //Debug.Log($"[DEBUG] {from_temp.Name} is a SubClass of {to_temp.Name} -> Generalization");
                                }
                                else
                                {

                                    umlBuilder.AppendLine($"{from_temp.Name.Replace(" ", "_")} --> {to_temp.Name.Replace(" ", "_")}");
                                    //Debug.Log($"[DEBUG] {from_temp.Name} is NOT a SubClass of {to_temp.Name} -> Association");
                                }
                            }
                        }
                    }
                }
                
            }
        }




    }
}
