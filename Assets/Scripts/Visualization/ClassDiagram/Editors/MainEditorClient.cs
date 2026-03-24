using Unity.Netcode;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Relations;
using Visualization.Networking;

namespace Visualization.ClassDiagram.Editors
{
    public class MainEditorClient : MainEditor
    {
        public MainEditorClient(IVisualEditor visualEditor) : base(visualEditor)
        {
        }

        public override void CreateNode(Class newClass, bool trackChanges = true)
        {
            Spawner.Instance.CreateClassServerRpc(newClass.Name, newClass.Id);
        }

        public override void DeleteNode(string className, bool trackChanges = true)
        {
            Spawner.Instance.DeleteClassServerRpc(className);
        }

        public override void UpdateNodeName(string oldName, string newName, bool trackChanges = true)
        {
            Spawner.Instance.UpdateClassNameServerRpc(oldName, newName);
        }

        public override void CreateRelation(Relation relation, bool trackChanges = true)
        {
            Spawner.Instance.CreateRelationServerRpc(relation.SourceModelName, relation.TargetModelName, relation.PropertiesEaType);
        }

        public override void DeleteRelation(GameObject relation, bool trackChanges = true)
        {
            var relationNetworkId = relation.GetComponent<NetworkObject>().NetworkObjectId;
            Spawner.Instance.DeleteRelationServerRpc(relationNetworkId);
        }

        public override void AddAttribute(string targetClass, Attribute attribute, bool trackChanges = true)
        {
            Spawner.Instance.AddAttributeServerRpc(targetClass, attribute.Name, attribute.Type);
        }

        public override void UpdateAttribute(string targetClass, string oldAttribute, Attribute newAttribute, bool trackChanges = true)
        {
            Spawner.Instance.UpdateAttributeServerRpc(targetClass, oldAttribute, newAttribute.Name, newAttribute.Type);
        }

        public override void DeleteAttribute(string className, string attributeName, bool trackChanges = true)
        {
            Spawner.Instance.DeleteAttributeServerRpc(className, attributeName);
        }

        public override void AddMethod(string targetClass, Method method, bool trackChanges = true)
        {
            string arguments = string.Join(",", method.arguments);
            Spawner.Instance.AddMethodServerRpc(targetClass, method.Name, method.ReturnValue, arguments);
        }

        public override void UpdateMethod(string targetClass, string oldMethod, Method newMethod, bool trackChanges = true)
        {
            string arguments = string.Join(",", newMethod.arguments);
            Spawner.Instance.UpdateMethodServerRpc(targetClass, oldMethod, newMethod.Name, newMethod.ReturnValue, arguments);
        }

        public override void DeleteMethod(string className, string methodName, bool trackChanges = true)
        {
            Spawner.Instance.DeleteMethodServerRpc(className, methodName);
        }
    }
}
