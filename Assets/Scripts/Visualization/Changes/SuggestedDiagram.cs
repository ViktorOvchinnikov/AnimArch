using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SuggestedDiagram : MonoBehaviour
{
    public static void Test()
    {
        SetClassColorAndButtons("suggested_class", new Color(0f, 1f, 0f, 0.5f));
        SetClassColorAndButtons("HumanWarrior", new Color(1f, 0f, 0f, 0.5f));
        ActivateMethods("HumanRanger");
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

    private static void ActivateMethods(string className)
    {
        GameObject myClassMethod = GameObject.Find(className);
        if (myClassMethod == null) return;
        
        Transform allMethods = myClassMethod.transform.GetChild(1).GetChild(4).GetChild(0);
        for (int i = 0; i < allMethods.childCount; i++)
        {
            Transform method1 = allMethods.GetChild(i).GetChild(3);
            Transform method2 = allMethods.GetChild(i).GetChild(4);
            
            method1.gameObject.SetActive(true);
            method2.gameObject.SetActive(true);
        }
    }
}
