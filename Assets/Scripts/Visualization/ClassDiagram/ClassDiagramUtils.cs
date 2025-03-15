using OALProgramControl;

namespace Visualization.ClassDiagram
{
    public static class ClassDiagramUtils
    {
        public static void ReloadClassDiagram()
        {
            Animation.Animation.Instance.CurrentProgramInstance.Reset();
            FileLoader.Instance.OpenDiagram();
        }
        public static void ReloadClassDiagram(ClassDiagramManager classDiagramData)
        {
            Animation.Animation.Instance.CurrentProgramInstance.Reset();
            new ClassDiagramBuilderFromMemory(classDiagramData).LoadDiagram();
        }
    }
}
