using System.Collections.Generic;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Editors;
using Visualization.ClassDiagram.Relations;
using Visualization.UI;
using OALProgramControl;
using System.Linq;

namespace Visualization.ClassDiagram
{
    public class ClassDiagramBuilderFromMemory : IClassDiagramBuilder
    {
        private readonly ClassDiagramManager ClassDiagramData;

        public ClassDiagramBuilderFromMemory(ClassDiagramManager classDiagramData)
        {
            ClassDiagramData = classDiagramData;
        }

        public override void FillDiagram()
        {
            MainEditor editor = UIEditorManager.Instance.mainEditor;

            Class newClass;
            Attribute newAttribute;
            Method newMethod;

            foreach (CDClass classData in ClassDiagramData.cdClassPool.Classes)
            {
                newClass = new Class ( classData.Name, classData.Name );
                editor.CreateNode(newClass);

                foreach (CDAttribute attributeData in classData.GetAttributes())
                {
                    newAttribute = new Attribute(attributeData.Name, attributeData.Name, attributeData.Type);
                    editor.AddAttribute(newClass.Name, newAttribute);
                }

                foreach (CDMethod methodData in classData.GetMethods())
                {
                    List<string> methodParameters = methodData.Parameters.Select(param => string.Format("{0} {1}", param.Type, param.Name)).ToList();
                    newMethod = new Method(methodData.Name, methodData.Name, methodData.ReturnType, methodParameters);
                    editor.AddMethod(newMethod.Name, newMethod);
                }
            }

            Relation newRelation;
            foreach (CDRelationship relationshipData in ClassDiagramData.cdRelationshipPool.GetAllRelationships())
            {
                newRelation = new Relation
                {
                    SourceModelName = relationshipData.FromClass,
                    TargetModelName = relationshipData.ToClass,
                    OALName = relationshipData.RelationshipName
                };
                UIEditorManager.Instance.mainEditor.CreateRelation(newRelation);
            }
        }

        public override void PositionClasses()
        {
            RenderClassesAuto();
        }
    }
}
