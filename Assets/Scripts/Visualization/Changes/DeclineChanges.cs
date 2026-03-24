using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.MarkedDiagram;
using OALProgramControl;
using UnityEngine.UI.Extensions;

public class DeclineChanges : MonoBehaviour
{
    private static bool IsBulkRejectButton(string objectName)
    {
        return objectName == "SuggestionsRejectAllButton" ||
               objectName == "RejectAllSuggestionsButton";
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

        return string.Equals(simplifiedObjectName, relationshipName, StringComparison.Ordinal) ||
               simplifiedObjectName.EndsWith(relationshipName, StringComparison.Ordinal) ||
               simplifiedObjectName.IndexOf(relationshipName, StringComparison.Ordinal) >= 0;
    }

    private static void HideMemberSuggestionButtons(Transform memberTransform)
    {
        var acceptButton = memberTransform.Find("VisualizationAcceptButton");
        var deleteButton = memberTransform.Find("VisualizationDeleteButton");

        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
    }

    private static void FinalizeMemberDeletionDecline(GameObject memberGo, string textChildName)
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

    public static void DeclineAllSuggestions()
    {
        var handlers = UnityEngine.Object.FindObjectsOfType<DeclineChanges>(true).ToList();
        foreach (var handler in handlers)
        {
            if (handler == null || handler.gameObject == null)
            {
                continue;
            }

            if (IsBulkRejectButton(handler.gameObject.name))
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

        if (IsBulkRejectButton(currentObjectName))
        {
            DeclineAllSuggestions();
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

        classesToRemove.Clear();
        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            if (markedClass.Inner.Name == currentObjectName && markedClass.DeleteMark)
            {
                // Decline class deletion - change color to blue and hide buttons
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

        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            CDMethodMarked methodToRemove = null;
            foreach (var markedMethod in markedClass.WrappedMethods)
            {
                if (markedMethod.Inner.Name == currentObjectName && markedMethod.CreateMark)
                {
                    // Decline method creation - destroy the object
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
            CDMethodMarked methodToRemove = null;
            foreach (var markedMethod in markedClass.WrappedMethods)
            {
                if (markedMethod.Inner.Name == currentObjectName && markedMethod.DeleteMark)
                {
                    // Decline method deletion - change text color to black and hide buttons
                    FinalizeMemberDeletionDecline(gameObject, "MethodText");
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
                    // Decline attribute creation - destroy the object
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

        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            MarkingDecorator<CDAttribute> attributeToRemove = null;
            foreach (var markedAttribute in markedClass.WrappedAttributes)
            {
                if (markedAttribute.Inner.Name == currentObjectName && markedAttribute.DeleteMark)
                {
                    // Decline attribute deletion - restore text color and hide buttons
                    FinalizeMemberDeletionDecline(gameObject, "AttributeText");
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
                // Decline relationship creation - destroy the object
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

        relationshipToRemove = null;
        foreach (var markedRelationship in currentDiff.RelationshipPoolMarked.GetAllRelationships())
        {
            if (IsMatchingRelationship(currentObjectName, markedRelationship) && markedRelationship.DeleteMark)
            {
                // Decline relationship deletion - change color to red and hide buttons
                var line = currentObject.GetComponent<UILineRenderer>();
                if (line != null)
                {
                    line.color = Color.white;
                }
                
                var acceptButton = currentObject.transform.Find($"ChangesVisualization/AcceptButton");
                var declineButton = currentObject.transform.Find($"ChangesVisualization/DeleteButton");
                if (acceptButton != null) acceptButton.gameObject.SetActive(false);
                if (declineButton != null) declineButton.gameObject.SetActive(false);
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
