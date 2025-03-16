using UMSAGL.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Visualization.ClassDiagram.Editors;
using Visualization.UI;

namespace Visualization.ClassDiagram
{
    public abstract class IClassDiagramBuilder
    {
        public IVisualEditor visualEditor;

        public IClassDiagramBuilder()
        {
            visualEditor = VisualEditorFactory.Create();
        }
        public void LoadDiagram()
        {
            CreateGraph();
            MakeNetworkedGraph();
            FillDiagram();
            PositionClasses();
            SuggestedDiagram.Test();
        }
        public abstract void FillDiagram();
        public abstract void PositionClasses();
        public virtual void MakeNetworkedGraph() { }
        public virtual void CreateGraph()
        {
            UIEditorManager.Instance.mainEditor.ClearDiagram();
            var graphGo = GameObject.Instantiate(DiagramPool.Instance.graphPrefab);
            graphGo.name = "Graph";

            DiagramPool.Instance.ClassDiagram.graph = graphGo.GetComponent<Graph>();
            DiagramPool.Instance.ClassDiagram.graph.nodePrefab = DiagramPool.Instance.classPrefab;
            GameObject.Find("DiagramPanel/Buttons/Edit").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("AnimationPanel/Buttons/Load").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("AnimationPanel/Buttons/Create").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("MaskingPanel/Buttons/Load").GetComponentInChildren<Button>().interactable = true;
        }
        protected void RenderClassesAuto()
        {
            DiagramPool.Instance.ClassDiagram.graph.Layout();
        }
    }
}
