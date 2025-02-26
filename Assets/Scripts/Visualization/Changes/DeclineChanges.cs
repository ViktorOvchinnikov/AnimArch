using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

public class DeclineChanges : MonoBehaviour
{
    public List<Change> Changes = new List<Change>();

     private void Start()
    {
        Changes.Add(new Change(ChangeType.Class, "HumanWarrior", true));
        Changes.Add(new Change(ChangeType.Class, "suggested_class", false));
        Changes.Add(new Change(ChangeType.Method, "Attack", true));
        Changes.Add(new Change(ChangeType.Method, "SneakAttack", true));
        Changes.Add(new Change(ChangeType.Method, "Hide", false));
        Changes.Add(new Change(ChangeType.Method, "HumanRanger", false));
    }
    public void SaveChanges()
    {
        GameObject currentObject = gameObject;
        String className = gameObject.name;
        foreach (Change change in Changes)
        {
            if (change.Name == gameObject.name)
            {
                if (change.Type == ChangeType.Class)
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
                } else if (change.Type == ChangeType.Method)
                {
                    if (change.ChangeIsDeletion == true)
                    {
                        gameObject.transform.GetChild(2).GetComponent<TMP_Text>().color = Color.black;
                        gameObject.transform.GetChild(3).gameObject.SetActive(false);
                        gameObject.transform.GetChild(4).gameObject.SetActive(false);
                    } else 
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}