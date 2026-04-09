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
using EditorChangesHistory;

public class DeclineChanges : MonoBehaviour
{
    private static void LogSuggestionReject(
        string targetType,
        string changeType,
        string targetName,
        string ownerClass = null,
        string fromClass = null,
        string toClass = null)
    {
        UXEventLogger.DebugLog("suggestion_reject", new
        {
            targetType,
            changeType,
            targetName,
            ownerClass,
            fromClass,
            toClass
        });
    }

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
        UXEventLogger.DebugLog("suggestion_reject_all_clicked", new
        {
            handlersCount = handlers.Count
        });

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
            UXEventLogger.DebugLog("suggestion_reject_skipped", new
            {
                reason = "current_diff_missing",
                objectName = currentObjectName
            });
            return;
        }

        List<CDClassMarked> classesToRemove = new List<CDClassMarked>();
        foreach (var markedClass in currentDiff.ClassPoolMarked.GetClassPool())
        {
            if (markedClass.Inner.Name == currentObjectName && markedClass.CreateMark)
            {
                LogSuggestionReject("class", "create", markedClass.Inner.Name);
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
                LogSuggestionReject("class", "delete", markedClass.Inner.Name);
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
                    LogSuggestionReject("method", "create", markedMethod.Inner.Name, markedClass.Inner.Name);
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
                    LogSuggestionReject("method", "delete", markedMethod.Inner.Name, markedClass.Inner.Name);
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
                    LogSuggestionReject("attribute", "create", markedAttribute.Inner.Name, markedClass.Inner.Name);
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
                    LogSuggestionReject("attribute", "delete", markedAttribute.Inner.Name, markedClass.Inner.Name);
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
                LogSuggestionReject(
                    "relation",
                    "create",
                    $"{markedRelationship.Inner.FromClass}->{markedRelationship.Inner.ToClass}",
                    fromClass: markedRelationship.Inner.FromClass,
                    toClass: markedRelationship.Inner.ToClass);

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
                LogSuggestionReject(
                    "relation",
                    "delete",
                    $"{markedRelationship.Inner.FromClass}->{markedRelationship.Inner.ToClass}",
                    fromClass: markedRelationship.Inner.FromClass,
                    toClass: markedRelationship.Inner.ToClass);

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
