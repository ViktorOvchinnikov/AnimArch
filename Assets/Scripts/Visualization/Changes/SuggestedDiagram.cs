using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

namespace Visualization.ClassDiagram.Diagrams
{
  public class SuggestedDiagram : Diagram
  {
    public static IEnumerable<string> GetDifference(IEnumerable<string> originalClassNames, IEnumerable<string> copiedClassNames)
    {
        // Return items in copiedClassNames that are not in originalClassNames
        return copiedClassNames.Except(originalClassNames);
    }
    public static void Test() {
        IEnumerable<string> originalClassNames = new List<string>();
        originalClassNames = DiagramPool.Instance.ClassDiagram.GetClassList().Select(x => x.Name);
        
        // Copy class list
        List<string> suggestedClassNames = originalClassNames.ToList();
        suggestedClassNames.Add("New class");

        var difference = GetDifference(originalClassNames, suggestedClassNames);
        
        // Iterate through the class names and log each one
        foreach (var className in difference)
        {
            Debug.Log($"Class Name: {className}");
        }
        GameObject myClass = GameObject.Find("suggested_class");
        Transform background = myClass.transform.GetChild(0);
        background.gameObject.GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.5f);
        Transform button01 = myClass.transform.GetChild(0).GetChild(0).GetChild(0);
        Transform button = myClass.transform.GetChild(0).GetChild(0).GetChild(1);
        button.gameObject.SetActive(true);
        button01.gameObject.SetActive(true);
        Debug.Log(myClass);

        GameObject myClassToRemove = GameObject.Find("HumanWarrior");
        Transform background2 = myClassToRemove.transform.GetChild(0);
        background2.gameObject.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.5f);
        Transform button2 = myClassToRemove.transform.GetChild(0).GetChild(0).GetChild(0);
        Transform button02 = myClassToRemove.transform.GetChild(0).GetChild(0).GetChild(1);
        button2.gameObject.SetActive(true);
        button02.gameObject.SetActive(true);

    }
  }
}
