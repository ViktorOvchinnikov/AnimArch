using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.MarkedDiagram;
using OALProgramControl;
using UnityEngine.UI.Extensions;
using EditorChangesHistory;


public class AcceptChanges : MonoBehaviour
{
    private static void LogSuggestionAccept(
        string targetType,
        string changeType,
        string targetName,
        string ownerClass = null,
        string fromClass = null,
        string toClass = null)
    {
        UXEventLogger.DebugLog("suggestion_accept", new
        {
            targetType,
            changeType,
            targetName,
            ownerClass,
            fromClass,
            toClass
        });
    }

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

    private static Color GetDefaultClassBackgroundColor()
    {
        GameObject classPrefab = DiagramPool.Instance != null ? DiagramPool.Instance.classPrefab : null;
        if (classPrefab == null)
        {
            return Color.white;
        }

        Transform background = classPrefab.transform.Find("Background");
        if (background == null && classPrefab.transform.childCount > 1)
        {
            background = classPrefab.transform.GetChild(1);
        }

        Image backgroundImage = background != null ? background.GetComponent<Image>() : null;
        return backgroundImage != null ? backgroundImage.color : Color.white;
    }

    private static void FinalizeClassCreation(GameObject classGo)
    {
        if (classGo == null)
        {
            return;
        }

        Transform background = classGo.transform.Find("Background");
        if (background == null && classGo.transform.childCount > 1)
        {
            background = classGo.transform.GetChild(1);
        }

        Image backgroundImage = background != null ? background.GetComponent<Image>() : null;
        if (backgroundImage != null)
        {
            backgroundImage.color = GetDefaultClassBackgroundColor();
        }

        if (classGo.transform.childCount > 0)
        {
            Transform controls = classGo.transform.GetChild(0);
            if (controls.childCount > 0)
            {
                controls.GetChild(0).gameObject.SetActive(false);
            }
            if (controls.childCount > 1)
            {
                controls.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public static void SaveAllSuggestions()
    {
        var handlers = UnityEngine.Object.FindObjectsOfType<AcceptChanges>(true).ToList();
        UXEventLogger.DebugLog("suggestion_accept_all_clicked", new
        {
            handlersCount = handlers.Count
        });

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
            UXEventLogger.DebugLog("suggestion_accept_skipped", new
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
                LogSuggestionAccept("class", "create", markedClass.Inner.Name);
                FinalizeClassCreation(currentObject);
                
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
                LogSuggestionAccept("class", "delete", markedClass.Inner.Name);
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
                    LogSuggestionAccept("method", "create", markedMethod.Inner.Name, markedClass.Inner.Name);
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
                    LogSuggestionAccept("method", "delete", markedMethod.Inner.Name, markedClass.Inner.Name);
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
                    LogSuggestionAccept("attribute", "create", markedAttribute.Inner.Name, markedClass.Inner.Name);
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
                    LogSuggestionAccept("attribute", "delete", markedAttribute.Inner.Name, markedClass.Inner.Name);
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
                LogSuggestionAccept(
                    "relation",
                    "create",
                    $"{markedRelationship.Inner.FromClass}->{markedRelationship.Inner.ToClass}",
                    fromClass: markedRelationship.Inner.FromClass,
                    toClass: markedRelationship.Inner.ToClass);

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
                LogSuggestionAccept(
                    "relation",
                    "delete",
                    $"{markedRelationship.Inner.FromClass}->{markedRelationship.Inner.ToClass}",
                    fromClass: markedRelationship.Inner.FromClass,
                    toClass: markedRelationship.Inner.ToClass);

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
