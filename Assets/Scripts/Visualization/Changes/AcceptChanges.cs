using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.MarkedDiagram;
using OALProgramControl;
using UnityEngine.UI.Extensions;


public class AcceptChanges : MonoBehaviour
{
    private static bool IsBulkAcceptButton(string objectName)
    {
        return objectName == "SuggestionsAcceptAllButton" ||
               objectName == "AcceptAllSuggestionsButton";
    }

    private static string GetSimplifiedRelationshipName(string gameObjectName)
    {
        int cloneIndex = gameObjectName.IndexOf("(Clone)");
        if (cloneIndex != -1)
        {
            gameObjectName = gameObjectName.Substring(cloneIndex + "(Clone)".Length);
        }
        
        gameObjectName = gameObjectName.Replace(" (UnityEngine.GameObject)", "");
        
        return gameObjectName.Trim();
    }

    private static bool IsMatchingRelationship(string currentObjectName, MarkingDecorator<CDRelationship> markedRelationship)
    {
        string simplifiedObjectName = GetSimplifiedRelationshipName(currentObjectName);
        string relationshipName = $"{markedRelationship.Inner.FromClass}->{markedRelationship.Inner.ToClass}";

        return string.Equals(simplifiedObjectName, relationshipName, System.StringComparison.Ordinal) ||
               simplifiedObjectName.EndsWith(relationshipName, System.StringComparison.Ordinal) ||
               simplifiedObjectName.IndexOf(relationshipName, System.StringComparison.Ordinal) >= 0;
    }

    private static void HideRelationshipSuggestionButtons(Transform relationTransform)
    {
        var changesContainer = relationTransform.Find("ChangesVisualization");
        if (changesContainer == null)
            return;

        var acceptButton = changesContainer.Find("AcceptButton");
        var deleteButton = changesContainer.Find("DeleteButton") ?? changesContainer.Find("DeclineButton");

        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
        changesContainer.gameObject.SetActive(false);
    }

    private static void HideMemberSuggestionButtons(Transform memberTransform)
    {
        var acceptButton = memberTransform.Find("VisualizationAcceptButton");
        var deleteButton = memberTransform.Find("VisualizationDeleteButton");

        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
    }

    private static void FinalizeMemberCreation(GameObject memberGo, string textChildName)
    {
        var textTransform = memberGo.transform.Find(textChildName);
        if (textTransform != null)
        {
            var text = textTransform.GetComponent<TMP_Text>() ?? textTransform.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.color = Color.black;
            }
        }

        HideMemberSuggestionButtons(memberGo.transform);
    }

    public static void SaveAllSuggestions()
    {
        var handlers = UnityEngine.Object.FindObjectsOfType<AcceptChanges>(true).ToList();
        foreach (var handler in handlers)
        {
            if (handler == null || handler.gameObject == null)
            {
                continue;
            }

            if (IsBulkAcceptButton(handler.gameObject.name))
            {
                continue;
            }

            handler.SaveChanges();
        }
    }

    public void SaveChanges()
    {
        GameObject currentObject = gameObject;
        string currentObjectName = gameObject.name;

        if (IsBulkAcceptButton(currentObjectName))
        {
            SaveAllSuggestions();
            return;
        }
        
        DiffResult currentDiff = DiagramPool.Instance.CurrentDiffResult;
        if (currentDiff == null)
        {
            // Debug.LogWarning("DiffResult not found in DiagramPool");
            return;
        }

        List<CDClassMarked> classesToRemove = new List<CDClassMarked>();
        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            if (markedClass.Inner.Name == currentObjectName && markedClass.CreateMark)
            {
                // Accept class creation - change color to blue and hide buttons
                Transform background2 = currentObject.transform.GetChild(1);
                background2.gameObject.GetComponent<Image>().color = new Color(0f, 0f, 1f, 0.5f);
                Transform button2 = currentObject.transform.GetChild(0).GetChild(0);
                Transform button = currentObject.transform.GetChild(0).GetChild(1);
                button.gameObject.SetActive(false);
                button2.gameObject.SetActive(false);
                
                classesToRemove.Add(markedClass);
                break;
            }
        }
        foreach (var cls in classesToRemove)
        {
            currentDiff.ClassPoolMarked.GetClassPool().Remove(cls);
            return;
        }

        classesToRemove.Clear();
        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            if (markedClass.Inner.Name == currentObjectName && markedClass.DeleteMark)
            {
                // Accept class deletion - destroy the object
                Destroy(currentObject);
                classesToRemove.Add(markedClass);
                break;
            }
        }
        foreach (var cls in classesToRemove)
        {
            currentDiff.ClassPoolMarked.GetClassPool().Remove(cls);
            return;
        }

        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            CDMethodMarked methodToRemove = null;
            foreach (var markedMethod in markedClass.WrappedMethods)
            {
                if (markedMethod.Inner.Name == currentObjectName && markedMethod.CreateMark)
                {
                    // Accept method creation - change text color to black and hide buttons
                    FinalizeMemberCreation(gameObject, "MethodText");
                    methodToRemove = markedMethod;
                    break;
                }
            }
            if (methodToRemove != null)
            {
                markedClass.WrappedMethods.Remove(methodToRemove);
                return;
            }
        }

        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            CDMethodMarked methodToRemove = null;
            foreach (var markedMethod in markedClass.WrappedMethods)
            {
                if (markedMethod.Inner.Name == currentObjectName && markedMethod.DeleteMark)
                {
                    // Accept method deletion - destroy the object
                    Destroy(gameObject);
                    methodToRemove = markedMethod;
                    break;
                }
            }
            if (methodToRemove != null)
            {
                markedClass.WrappedMethods.Remove(methodToRemove);
                return;
            }
        }

        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            MarkingDecorator<CDAttribute> attributeToRemove = null;
            foreach (var markedAttribute in markedClass.WrappedAttributes)
            {
                if (markedAttribute.Inner.Name == currentObjectName && markedAttribute.CreateMark)
                {
                    // Accept attribute creation - change text color to black and hide buttons
                    FinalizeMemberCreation(gameObject, "AttributeText");
                    attributeToRemove = markedAttribute;
                    break;
                }
            }
            if (attributeToRemove != null)
            {
                markedClass.WrappedAttributes.Remove(attributeToRemove);
                return;
            }
        }

        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            MarkingDecorator<CDAttribute> attributeToRemove = null;
            foreach (var markedAttribute in markedClass.WrappedAttributes)
            {
                if (markedAttribute.Inner.Name == currentObjectName && markedAttribute.DeleteMark)
                {
                    // Accept attribute deletion - destroy the object
                    Destroy(gameObject);
                    attributeToRemove = markedAttribute;
                    break;
                }
            }
            if (attributeToRemove != null)
            {
                markedClass.WrappedAttributes.Remove(attributeToRemove);
                return;
            }
        }

        MarkingDecorator<CDRelationship> relationshipToRemove = null;
        foreach (var markedRelationship in currentDiff.RelationshipPoolMarked.GetAllRelationships())
        {
            if (IsMatchingRelationship(currentObjectName, markedRelationship) && markedRelationship.CreateMark)
            {
                var line = currentObject.GetComponent<UILineRenderer>();
                if (line != null)
                {
                    line.color = Color.white;
                }

                HideRelationshipSuggestionButtons(currentObject.transform);
                relationshipToRemove = markedRelationship;
                break;
            }
        }
        if (relationshipToRemove != null)
        {
            currentDiff.RelationshipPoolMarked.Remove(relationshipToRemove);
            return;
        }

        relationshipToRemove = null;
        foreach (var markedRelationship in currentDiff.RelationshipPoolMarked.GetAllRelationships())
        {
            if (IsMatchingRelationship(currentObjectName, markedRelationship) && markedRelationship.DeleteMark)
            {
                // Accept relationship deletion - destroy the object
                Destroy(currentObject);
                relationshipToRemove = markedRelationship;
                break;
            }
        }
        if (relationshipToRemove != null)
        {
            currentDiff.RelationshipPoolMarked.Remove(relationshipToRemove);
            return;
        }
    }
}
