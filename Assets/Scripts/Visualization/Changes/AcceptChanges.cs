using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.MarkedDiagram;
using OALProgramControl;
using UnityEngine.UI.Extensions;


public class AcceptChanges : MonoBehaviour
{
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
    public void SaveChanges()
    {
        GameObject currentObject = gameObject;
        string currentObjectName = gameObject.name;
        
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
                    gameObject.transform.GetChild(2).GetComponent<TMP_Text>().color = Color.black;
                    gameObject.transform.GetChild(3).gameObject.SetActive(false);
                    gameObject.transform.GetChild(4).gameObject.SetActive(false);
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

        MarkingDecorator<CDRelationship> relationshipToRemove = null;
        foreach (var markedRelationship in currentDiff.RelationshipPoolMarked.GetAllRelationships())
        {
            string relationshipName = $"{markedRelationship.Inner.FromClass}->{markedRelationship.Inner.ToClass}";
            string simplifiedObjectName = GetSimplifiedRelationshipName(currentObjectName);
            // Debug.Log($"Comparing relationship: '{simplifiedObjectName}' vs '{relationshipName}'");
            if (simplifiedObjectName == relationshipName && markedRelationship.CreateMark)
            {
                var line = currentObject.GetComponent<UILineRenderer>();
                if (line != null)
                {
                    line.color = Color.white;
                }
                
                var acceptButton = currentObject.transform.Find($"ChangesVisualization/AcceptButton");
                var declineButton = currentObject.transform.Find($"ChangesVisualization/DeclineButton");
                if (acceptButton != null) acceptButton.gameObject.SetActive(false);
                if (declineButton != null) declineButton.gameObject.SetActive(false);
                relationshipToRemove = markedRelationship;
                break;
            }
        }
        if (relationshipToRemove != null)
        {
            currentDiff.RelationshipPoolMarked.GetAllRelationships().Remove(relationshipToRemove);
            return;
        }

        relationshipToRemove = null;
        foreach (var markedRelationship in currentDiff.RelationshipPoolMarked.GetAllRelationships())
        {
            string relationshipName = $"{markedRelationship.Inner.FromClass}->{markedRelationship.Inner.ToClass}";
            string simplifiedObjectName = GetSimplifiedRelationshipName(currentObjectName);
            if (simplifiedObjectName == relationshipName && markedRelationship.DeleteMark)
            {
                // Accept relationship deletion - destroy the object
                Destroy(currentObject);
                relationshipToRemove = markedRelationship;
                break;
            }
        }
        if (relationshipToRemove != null)
        {
            currentDiff.RelationshipPoolMarked.GetAllRelationships().Remove(relationshipToRemove);
            return;
        }
    }
}