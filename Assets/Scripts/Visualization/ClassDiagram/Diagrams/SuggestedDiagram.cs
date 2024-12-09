using System.Collections.Generic;
using TMPro;
using UMSAGL.Scripts;
using UnityEngine;
using Visualization;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.ClassDiagram.Diagrams;
using Visualization.ClassDiagram.Relations;
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
    }
  }
}
