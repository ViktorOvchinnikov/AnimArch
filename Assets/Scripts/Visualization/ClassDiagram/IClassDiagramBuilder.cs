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
            SetButtonInteractable("DiagramPanel/Buttons/Edit", true);
            SetButtonInteractable("AnimationPanel/Buttons/Load", true);
            SetButtonInteractable("AnimationPanel/Buttons/Create", true);
            SetButtonInteractable("MaskingPanel/Buttons/Load", true);
        }

        private static void SetButtonInteractable(string path, bool interactable)
        {
            GameObject buttonObject = GameObject.Find(path);
            if (buttonObject == null)
            {
                return;
            }

            Button button = buttonObject.GetComponentInChildren<Button>(includeInactive: true);
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
        protected void RenderClassesAuto()
        {
            DiagramPool.Instance.ClassDiagram.graph.Layout();
        }
    }
}
