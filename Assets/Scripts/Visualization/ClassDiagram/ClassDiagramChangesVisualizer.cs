using System.Collections.Generic;
using System.Linq;
using OALProgramControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Editors;
using Visualization.ClassDiagram.MarkedDiagram;
using Visualization.ClassDiagram.Relations;
using Visualization.UI;

namespace Visualization.ClassDiagram
{
    public class ClassDiagramChangesVisualizer
    {
        private DiffResult diffResult;
        private MainEditor editor = UIEditorManager.Instance.mainEditor;
        
        public ClassDiagramChangesVisualizer(DiffResult diffResult)
        {
            this.diffResult = diffResult;
        }
        
        private static GameObject GetRelationshipGameObject(CDRelationship relationship)
        {
            string BuildRequest(string prefix) =>
                $"{prefix}(Clone){relationship.FromClass} (UnityEngine.GameObject)->{relationship.ToClass} (UnityEngine.GameObject)";

            GameObject go = GameObject.Find(BuildRequest("Generalization"));
            if (go != null)
                return go;
        
            go = GameObject.Find(BuildRequest("AssociationNone"));
            if (go != null)
                return go;

            go = GameObject.Find(BuildRequest("AssociationSD"));
            return go;
        }
        
        private static void SetClassColorAndButtons(string className, Color color)
        {
            GameObject myClass = GameObject.Find(className);
            if (myClass == null) return;

            Transform background = myClass.transform.GetChild(1);
            background.gameObject.GetComponent<Image>().color = color;

            Transform button1 = myClass.transform.GetChild(0).GetChild(0);
            Transform button2 = myClass.transform.GetChild(0).GetChild(1);

            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);
        }

        private static void ActivateRelationship(GameObject relationshipGo)
        {
            if (relationshipGo == null) return;
            
            var methodDeleteButton = relationshipGo.transform.Find($"DeleteButton/DeleteButton");
            
            methodDeleteButton.gameObject.SetActive(true);
            // methodEditButton.gameObject.SetActive(true);
        }

        private static void ActivateMethod(string className, string methodName, Color color)
        {
            GameObject classGo = GameObject.Find(className);
            if (classGo == null) return;
            
            var methodDeleteButton = classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{methodName}/DeleteButton");
            var methodEditButton = classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{methodName}/EditButton");
            var metodText =  classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{methodName}/MethodText");
            var component = metodText.gameObject.GetComponentInChildren<TMP_Text>().color = color;
            
            methodDeleteButton.gameObject.SetActive(true);
            methodEditButton.gameObject.SetActive(true);
        }
    
        private static void ActivateMethods(string className)
        {
            GameObject myClassMethod = GameObject.Find(className);
            if (myClassMethod == null) return;

            Transform allMethods = myClassMethod.transform.GetChild(1).GetChild(4).GetChild(0);
            for (int i = 0; i < allMethods.childCount; i++)
            {
                Transform method1 = allMethods.GetChild(i).GetChild(3);
                Transform method2 = allMethods.GetChild(i).GetChild(4);

                if (i % 2 == 0)
                {
                    allMethods.GetChild(i).GetChild(2).GetComponent<TMP_Text>().color = Color.red;
                }
                else
                {
                    allMethods.GetChild(i).GetChild(2).GetComponent<TMP_Text>().color = new Color(0.0f, 0.7f, 0.0f);
                }

                method1.gameObject.SetActive(true);
                method2.gameObject.SetActive(true);
            }
        }
        
        private static void HighlightRelationship(MarkingDecorator<CDRelationship> relationship)
        {
            GameObject relationshipGameObject = GetRelationshipGameObject(relationship.Inner);
            if (relationshipGameObject == null) return;
        
            var line = relationshipGameObject.GetComponent<UILineRenderer>();
            if (line != null)
            {
                if (relationship.DeleteMark)
                {
                    line.color = Color.red;
                }

                if (relationship.CreateMark)
                {
                    line.color = Color.green;
                }
            }
            ActivateRelationship(relationshipGameObject);
        }

        private void ProcessAttributes(CDClassMarked cdClass)
        {
            
        }
        
        private void ProcessMethods(CDClassMarked cdClass)
        {
            foreach (CDMethodMarked cdMethod in cdClass.WrappedMethods)
            {
                if (cdMethod.CreateMark)
                {
                    List<string> methodParameters = cdMethod.Inner.Parameters.Select(param => string.Format("{0} {1}", param.Type, param.Name)).ToList();
                    Method newMethod = new Method(cdMethod.Inner.Name, cdMethod.Inner.Name, cdMethod.Inner.ReturnType, methodParameters);
                    UIEditorManager.Instance.mainEditor.AddMethod(cdClass.Inner.Name, newMethod);
                    
                    ActivateMethod(cdClass.Inner.Name, cdMethod.Inner.Name, Color.green);
                }

                if (cdMethod.DeleteMark)
                {
                    ActivateMethod(cdClass.Inner.Name, cdMethod.Inner.Name, Color.red);
                }
            }
        }

        private void AddClass(CDClassMarked cdClass)
        {
            Class newClass = new Class ( cdClass.Inner.Name, cdClass.Inner.Name );
            editor.CreateNode(newClass);

            foreach (CDAttribute attributeData in cdClass.Inner.GetAttributes())
            {
                Attribute newAttribute = new Attribute(attributeData.Name, attributeData.Name, attributeData.Type);
                editor.AddAttribute(newClass.Name, newAttribute);
            }

            foreach (CDMethod methodData in cdClass.Inner.GetMethods())
            {
                List<string> methodParameters = methodData.Parameters.Select(param => string.Format("{0} {1}", param.Type, param.Name)).ToList();
                Method newMethod = new Method(methodData.Name, methodData.Name, methodData.ReturnType, methodParameters);
                editor.AddMethod(newClass.Name, newMethod);
            }
        }

        private void ProcessClasses()
        {
            foreach (CDClassMarked cdClass in diffResult.ClassPoolMarked.GetClassPool())
            {
                if (cdClass.CreateMark)
                {
                    AddClass(cdClass);
                    SetClassColorAndButtons(cdClass.Inner.Name, new Color(0f, 1f, 0f, 0.5f));
                }

                if (cdClass.DeleteMark)
                {
                    SetClassColorAndButtons(cdClass.Inner.Name, new Color(1f, 0f, 0f, 0.5f));
                }
                
                ProcessMethods(cdClass);
                ProcessAttributes(cdClass);
            }
            
           
        }
        
        private void ProcessRelations()
        {
            foreach (MarkingDecorator<CDRelationship> relationship in diffResult.RelationshipPoolMarked.GetAllRelationships())
            {
                if (relationship.CreateMark)
                {
                    Relation newRelation = new Relation
                    {
                        SourceModelName = relationship.Inner.FromClass,
                        TargetModelName = relationship.Inner.ToClass,
                    
                        // Hardcoded link type and direction
                        PropertiesEaType = "Association",
                        PropertiesDirection = "Source -> Destination"
                    };
                    UIEditorManager.Instance.mainEditor.CreateRelation(newRelation);
                    HighlightRelationship(relationship);
                }

                if (relationship.DeleteMark) HighlightRelationship(relationship);
            }
        }

        public void Visualize()
        {
            ProcessClasses();
            ProcessRelations();
        }
    }
}