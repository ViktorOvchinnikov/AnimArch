using System;
using System.Linq;
using AnimArch.Extensions;
using UMSAGL.Scripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Editors;
using Visualization.ClassDiagram.Relations;
using Visualization.UI;
using Attribute = Visualization.ClassDiagram.ClassComponents.Attribute;

namespace Visualization.Networking
{
    [RequireComponent(typeof(NetworkObject))]
    public class Spawner : NetworkSingleton<Spawner>
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        [ClientRpc]
        public void SetNetworkObjectNameClientRpc(string name, ulong networkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            objects[networkId].name = name;
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreateClassServerRpc(string className, string id)
        {
            if (IsClient && !IsHost)
                return;

            var newClass = new Class(className, id);
            UIEditorManager.Instance.mainEditor.CreateNode(newClass, false);
        }

        [ClientRpc]
        public void SetClassNameClientRpc(string name, ulong networkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[networkId];
            var go = obj.GetComponent<NetworkObject>().gameObject;
            go.name = name;
            var visualEditor = new VisualEditor();
            visualEditor.UpdateNodeName(go);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateClassNameServerRpc(string oldName, string newName)
        {
            if (IsClient && !IsHost)
                return;
            UIEditorManager.Instance.mainEditor.UpdateNodeName(oldName, newName, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeleteClassServerRpc(string className)
        {
            if (IsClient && !IsHost)
                return;
            UIEditorManager.Instance.mainEditor.DeleteNode(className, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreateRelationServerRpc(string fromClass, string toClass, string realtionType)
        {
            if (IsClient && !IsHost)
                return;

            var type = realtionType.Split();

            var relation = new Relation
            {
                SourceModelName = fromClass,
                TargetModelName = toClass,
                PropertiesEaType = type.Length > 1 ? type[1] : type[0],
                PropertiesDirection = type.Length > 1 ? "none" : "Source -> Destination"
            };
            UIEditorManager.Instance.mainEditor.CreateRelation(relation, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeleteRelationServerRpc(ulong relationNetworkId)
        {
            if (IsClient && !IsHost)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[relationNetworkId];
            var classGo = obj.GetComponent<NetworkObject>().gameObject;
            UIEditorManager.Instance.mainEditor.DeleteRelation(classGo, false);
        }

        [ServerRpc(RequireOwnership = false)]

        public void AddAttributeServerRpc(string targetClass, string attributeName, string type)
        {
            if (IsClient && !IsHost)
                return;

            var attribute = new Attribute
            {
                Name = attributeName,
                Type = type,
                Id = Guid.NewGuid().ToString()
            };
            UIEditorManager.Instance.mainEditor.AddAttribute(targetClass, attribute, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateAttributeServerRpc(string targetClass, string oldAttribute, string attributeName, string type)
        {
            if (IsClient && !IsHost)
                return;
            var attribute = new Attribute
            {
                Name = attributeName,
                Type = type
            };
            UIEditorManager.Instance.mainEditor.UpdateAttribute(targetClass, oldAttribute, attribute, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeleteAttributeServerRpc(string className, string attributeName)
        {
            if (IsClient && !IsHost)
                return;

            UIEditorManager.Instance.mainEditor.DeleteAttribute(className, attributeName, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMethodServerRpc(string targetClass, string methodName, string methodReturnValue, string methodArguments)
        {
            if (IsClient && !IsHost)
                return;

            var newMethod = new Method
            {
                Name = methodName,
                ReturnValue = methodReturnValue,
                arguments = methodArguments.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new(),
                Id = Guid.NewGuid().ToString()
        };
            UIEditorManager.Instance.mainEditor.AddMethod(targetClass, newMethod, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMethodServerRpc(string targetClass, string oldMethod, string methodName, string methodReturnValue, string methodArguments)
        {
            if (IsClient && !IsHost)
                return;

            var newMethod = new Method
            {
                Name = methodName,
                ReturnValue = methodReturnValue,
                arguments = methodArguments.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new()
            };
            UIEditorManager.Instance.mainEditor.UpdateMethod(targetClass, oldMethod, newMethod, false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeleteMethodServerRpc(string className, string methodName)
        {
            if (IsClient && !IsHost)
                return;

            UIEditorManager.Instance.mainEditor.DeleteMethod(className, methodName, false);
        }

        [ClientRpc]
        public void AddAttributeClientRpc(string attributeName, string attributeText, ulong classNetworkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[classNetworkId];
            var classGo = obj.GetComponent<NetworkObject>().gameObject;
            var visualEditor = new VisualEditorClient();
            visualEditor.AddAttribute(attributeName, attributeText, classGo);
        }

        [ClientRpc]
        public void UpdateAttributeClientRpc(string oldAttributeName, string newAttributeName, string attributeText, ulong classNetworkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[classNetworkId];
            var classGo = obj.GetComponent<NetworkObject>().gameObject;
            var visualEditor = new VisualEditorClient();
            visualEditor.UpdateAttribute(oldAttributeName, newAttributeName, attributeText, classGo);
        }

        [ClientRpc]
        public void DeleteAttributeClientRpc(string attributeName, ulong classNetworkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[classNetworkId];
            var classGo = obj.GetComponent<NetworkObject>().gameObject;
            var visualEditor = new VisualEditorClient();
            visualEditor.DeleteAttribute(attributeName, classGo);
        }

        [ClientRpc]
        public void AddMethodClientRpc(string methodName, string methodText, ulong classNetworkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[classNetworkId];
            var classGo = obj.GetComponent<NetworkObject>().gameObject;
            var visualEditor = new VisualEditorClient();
            visualEditor.AddMethod(methodName, methodText, classGo);
        }

        [ClientRpc]
        public void UpdateMethodClientRpc(string oldMethodName, string newMethodName, string methodText, ulong classNetworkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[classNetworkId];
            var classGo = obj.GetComponent<NetworkObject>().gameObject;
            var visualEditor = new VisualEditorClient();
            visualEditor.UpdateMethod(oldMethodName, newMethodName, methodText, classGo);
        }

        [ClientRpc]
        public void DeleteMethodClientRpc(string methodName, ulong classNetworkId)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            var obj = objects[classNetworkId];
            var classGo = obj.GetComponent<NetworkObject>().gameObject;
            var visualEditor = new VisualEditorClient();
            visualEditor.DeleteMethod(methodName, classGo);
        }

        [ClientRpc]
        public void SetButtonsActiveClientRpc(bool active = true)
        {
            if (IsServer)
                return;
            DiagramPool.Instance.ClassDiagram.graph.GetComponentsInChildren<GraphicRaycaster>()
                .ForEach(x => x.enabled = active);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreateGraphServerRpc()
        {
            if (IsClient && !IsHost)
                return;
            UIEditorManager.Instance.InitializeCreation();
            GraphCreatedClientRpc();
            GameObject.Find("EditBtn").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("Create").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("Edit (1)").GetComponentInChildren<Button>().interactable = true;
        }


        [ClientRpc]
        public void GraphCreatedClientRpc()
        {
            if (IsServer)
                return;
            DiagramPool.Instance.ClassDiagram.graph = GameObject.Find("Graph").GetComponent<Graph>();
            DiagramPool.Instance.ClassDiagram.graph.enabled = false;

            GameObject.Find("EditBtn").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("Create").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("Edit (1)").GetComponentInChildren<Button>().interactable = true;
        }

        [ClientRpc]
        public void SetLinePointsClientRpc(ulong networkId, Vector2[] points)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            Debug.Assert(objects[networkId].GetComponent<UILineRenderer>());
            var lineRenderer = objects[networkId].GetComponent<UILineRenderer>();
            lineRenderer.Points = points;
        }

        [ClientRpc]
        public void SetLineResolutionClientRpc(ulong networkId, float resolution)
        {
            if (IsServer)
                return;
            var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
            Debug.Assert(objects[networkId].GetComponent<UILineRenderer>());
            var lineRenderer = objects[networkId].GetComponent<UILineRenderer>();
            lineRenderer.Resoloution = resolution;
        }
    }
}
