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
using EditorChangesHistory;

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
            
            var positioner = relationshipGo.GetComponent<RelationshipButtonPositioner>();
            if (positioner == null)
            {
                positioner = relationshipGo.AddComponent<RelationshipButtonPositioner>();
                positioner.Initialize();
            }
            
            var changesContainer = relationshipGo.transform.Find("ChangesVisualization");
            if (changesContainer != null)
            {
                changesContainer.gameObject.SetActive(true);
                
                var acceptButton = changesContainer.Find("AcceptButton");
                var deleteButton = changesContainer.Find("DeleteButton");
                
                if (acceptButton != null)
                {
                    acceptButton.gameObject.SetActive(true);
                }
                
                if (deleteButton != null)
                {
                    deleteButton.gameObject.SetActive(true);
                }
            }
        }

        private static void ActivateMethod(string className, string methodName, Color color)
        {
            GameObject classGo = GameObject.Find(className);
            if (classGo == null) return;
            
            var methodDeleteButton = classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{methodName}/VisualizationAcceptButton");
            var methodEditButton = classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{methodName}/VisualizationDeleteButton");
            var metodText =  classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{methodName}/MethodText");
            var component = metodText.gameObject.GetComponentInChildren<TMP_Text>().color = color;
            
            methodDeleteButton.gameObject.SetActive(true);
            methodEditButton.gameObject.SetActive(true);
        }

        private static void ActivateAttribute(string className, string attributeName, Color color)
        {
            GameObject classGo = GameObject.Find(className);
            if (classGo == null) return;

            var attributeDeleteButton = classGo.transform.Find($"Background/Attributes/AttributeLayoutGroup/{attributeName}/VisualizationAcceptButton");
            var attributeEditButton = classGo.transform.Find($"Background/Attributes/AttributeLayoutGroup/{attributeName}/VisualizationDeleteButton");
            var attributeText = classGo.transform.Find($"Background/Attributes/AttributeLayoutGroup/{attributeName}/AttributeText");

            if (attributeText != null)
            {
                attributeText.gameObject.GetComponentInChildren<TMP_Text>().color = color;
            }

            if (attributeDeleteButton != null)
            {
                attributeDeleteButton.gameObject.SetActive(true);
            }

            if (attributeEditButton != null)
            {
                attributeEditButton.gameObject.SetActive(true);
            }
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
            foreach (MarkingDecorator<CDAttribute> cdAttribute in cdClass.WrappedAttributes)
            {
                if (cdAttribute.CreateMark)
                {
                    Attribute newAttribute = new Attribute(cdAttribute.Inner.Name, cdAttribute.Inner.Name, cdAttribute.Inner.Type);
                    UIEditorManager.Instance.mainEditor.AddAttribute(cdClass.Inner.Name, newAttribute, false);

                    ActivateAttribute(cdClass.Inner.Name, cdAttribute.Inner.Name, Color.green);
                }

                if (cdAttribute.DeleteMark)
                {
                    ActivateAttribute(cdClass.Inner.Name, cdAttribute.Inner.Name, Color.red);
                }
            }
        }
        
        private void ProcessMethods(CDClassMarked cdClass)
        {
            foreach (CDMethodMarked cdMethod in cdClass.WrappedMethods)
            {
                if (cdMethod.CreateMark)
                {
                    List<string> methodParameters = cdMethod.Inner.Parameters.Select(param => string.Format("{0} {1}", param.Type, param.Name)).ToList();
                    Method newMethod = new Method(cdMethod.Inner.Name, cdMethod.Inner.Name, cdMethod.Inner.ReturnType, methodParameters);
                    UIEditorManager.Instance.mainEditor.AddMethod(cdClass.Inner.Name, newMethod, false);
                    
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
            editor.CreateNode(newClass, false);

            foreach (CDAttribute attributeData in cdClass.Inner.GetAttributes())
            {
                Attribute newAttribute = new Attribute(attributeData.Name, attributeData.Name, attributeData.Type);
                editor.AddAttribute(newClass.Name, newAttribute, false);
            }

            foreach (CDMethod methodData in cdClass.Inner.GetMethods())
            {
                List<string> methodParameters = methodData.Parameters.Select(param => string.Format("{0} {1}", param.Type, param.Name)).ToList();
                Method newMethod = new Method(methodData.Name, methodData.Name, methodData.ReturnType, methodParameters);
                editor.AddMethod(newClass.Name, newMethod, false);
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
                    UIEditorManager.Instance.mainEditor.CreateRelation(newRelation, false);
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

        public void CleanupSuggestions()
        {
            if (diffResult == null) return;

            UXEventLogger.DebugLog("suggestions_cleanup", new
            {
                classesCount = diffResult.ClassPoolMarked.GetClassPool().Count,
                relationshipsCount = diffResult.RelationshipPoolMarked.GetAllRelationships().Count
            });
            
            // Cleanup classes
            foreach (CDClassMarked cdClass in diffResult.ClassPoolMarked.GetClassPool())
            {
                if (cdClass.CreateMark)
                {
                    editor.DeleteNode(cdClass.Inner.Name, false);
                }
                else if (cdClass.DeleteMark)
                {
                    // Restore original color for classes marked for deletion
                    GameObject classGo = GameObject.Find(cdClass.Inner.Name);
                    if (classGo != null)
                    {
                        Transform background = classGo.transform.GetChild(1);
                        background.gameObject.GetComponent<Image>().color = Color.white;
                        
                        Transform button1 = classGo.transform.GetChild(0).GetChild(0);
                        Transform button2 = classGo.transform.GetChild(0).GetChild(1);
                        button1.gameObject.SetActive(false);
                        button2.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // Cleanup modified methods
                    foreach (CDMethodMarked cdMethod in cdClass.WrappedMethods)
                    {
                        if (cdMethod.CreateMark)
                        {
                            editor.DeleteMethod(cdClass.Inner.Name, cdMethod.Inner.Name, false);
                        }
                        else if (cdMethod.DeleteMark)
                        {
                            // Restore method color and hide buttons
                            GameObject classGo = GameObject.Find(cdClass.Inner.Name);
                            if (classGo != null)
                            {
                                var methodDeleteButton = classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{cdMethod.Inner.Name}/VisualizationAcceptButton");
                                var methodEditButton = classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{cdMethod.Inner.Name}/VisualizationDeleteButton");
                                var methodText = classGo.transform.Find($"Background/Methods/MethodLayoutGroup/{cdMethod.Inner.Name}/MethodText");
                                
                                if (methodText != null)
                                {
                                    methodText.gameObject.GetComponentInChildren<TMP_Text>().color = Color.black;
                                }
                                if (methodDeleteButton != null) methodDeleteButton.gameObject.SetActive(false);
                                if (methodEditButton != null) methodEditButton.gameObject.SetActive(false);
                            }
                        }
                    }

                    // Cleanup modified attributes
                    foreach (MarkingDecorator<CDAttribute> cdAttribute in cdClass.WrappedAttributes)
                    {
                        if (cdAttribute.CreateMark)
                        {
                            editor.DeleteAttribute(cdClass.Inner.Name, cdAttribute.Inner.Name, false);
                        }
                        else if (cdAttribute.DeleteMark)
                        {
                            // Restore attribute color and hide buttons
                            GameObject classGo = GameObject.Find(cdClass.Inner.Name);
                            if (classGo != null)
                            {
                                var attributeDeleteButton = classGo.transform.Find($"Background/Attributes/AttributeLayoutGroup/{cdAttribute.Inner.Name}/VisualizationAcceptButton");
                                var attributeEditButton = classGo.transform.Find($"Background/Attributes/AttributeLayoutGroup/{cdAttribute.Inner.Name}/VisualizationDeleteButton");
                                var attributeText = classGo.transform.Find($"Background/Attributes/AttributeLayoutGroup/{cdAttribute.Inner.Name}/AttributeText");

                                if (attributeText != null)
                                {
                                    attributeText.gameObject.GetComponentInChildren<TMP_Text>().color = Color.black;
                                }

                                if (attributeDeleteButton != null) attributeDeleteButton.gameObject.SetActive(false);
                                if (attributeEditButton != null) attributeEditButton.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
            
            // Cleanup relationships
            foreach (MarkingDecorator<CDRelationship> relationship in diffResult.RelationshipPoolMarked.GetAllRelationships())
            {
                GameObject relationshipGo = GetRelationshipGameObject(relationship.Inner);
                if (relationshipGo == null) continue;
                
                if (relationship.CreateMark)
                {
                    editor.DeleteRelation(relationshipGo, false);
                }
                else if (relationship.DeleteMark)
                {
                    // Restore relationship color
                    var line = relationshipGo.GetComponent<UILineRenderer>();
                    if (line != null)
                    {
                        line.color = Color.white;
                    }
                    
                    // Hide buttons
                    var changesContainer = relationshipGo.transform.Find("ChangesVisualization");
                    if (changesContainer != null)
                    {
                        changesContainer.gameObject.SetActive(false);
                    }
                }
            }
            
            Debug.Log("CleanupSuggestions called.");
        }
    }
}
