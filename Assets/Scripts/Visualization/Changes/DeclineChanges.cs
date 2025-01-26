using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class DeclineChanges : MonoBehaviour
{
    public List<Change> Changes = new List<Change>();

     private void Start()
    {
        Changes.Add(new Change(ChangeType.Class, "HumanWarrior", true));
        Changes.Add(new Change(ChangeType.Class, "suggested_class", false));
    }
    public void SaveChanges()
    {
        GameObject currentObject = gameObject;
        String className = gameObject.name;
        foreach (Change change in Changes)
    {
        if (change.Type == ChangeType.Class && change.Name == className)
        {
            if (change.ChangeIsDeletion == true)
            {
                Transform background2 = currentObject.transform.GetChild(1);
                background2.gameObject.GetComponent<Image>().color = new Color(0f, 0f, 1f, 0.5f);
                Transform button2 = currentObject.transform.GetChild(0).GetChild(0);
                Transform button = currentObject.transform.GetChild(0).GetChild(1);
                button.gameObject.SetActive(false);
                button2.gameObject.SetActive(false);
            } else 
            {
                Destroy(currentObject);
            }
            
        }
    }
       
    }
}