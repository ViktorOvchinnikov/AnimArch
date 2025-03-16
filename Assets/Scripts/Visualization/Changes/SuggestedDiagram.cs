using System.Collections.Generic;
using Assets.Scripts.AnimationControl.UMLDiagram;
using OALProgramControl;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Visualization.Animation;
using System.Threading.Tasks;


public class SuggestedDiagram : MonoBehaviour
{
    public static async void Test()
    {
        PlantUMLBuilder plantUmlBuilder = new PlantUMLBuilder();
        string plantUMLString = plantUmlBuilder.GetDiagram();
        //Debug.Log(plantUMLString);
        ClassDiagramManager p = UMLParserBridge.Parse(plantUMLString);
        
        GPTMessage gptMessage = new GPTMessage();
        string systemPrompt = @"
                              Imagine you're an experienced software engineer. You will be given a UML diagram in the form of PlantUML code. Your task is to suggest a couple of small changes that the user would most likely want to make in the next step of their work. The changes don't have to be significant. Your limit on the number of changes: 3. Changes can be such as: 
                              - Adding/Removing relations/classes/methods or class attributes
                              - Changing the name of a class/method/attribute
                              Your answer should contain only PlantUML code. (starting with the @startuml tag and ending with @enduml). The PlantUML code is provided below.;
                              ";
        
        string fullPrompt = systemPrompt + "\n" + plantUMLString;
        
        GPTMessage message = new GPTMessage();

        string response = await message.SendMessage("Hello how are you?");
        
        Debug.Log($"Response GPT: {response}");
        
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
            
            if (i % 2 == 0 )
            {
              allMethods.GetChild(i).GetChild(2).GetComponent<TMP_Text>().color = Color.red;
            } else {
              allMethods.GetChild(i).GetChild(2).GetComponent<TMP_Text>().color = new Color(0.0f, 0.7f, 0.0f);
            }
            method1.gameObject.SetActive(true);
            method2.gameObject.SetActive(true);
        }
    }
}
