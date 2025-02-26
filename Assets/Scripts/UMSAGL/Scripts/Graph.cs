using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnimArch.Extensions;
using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Visualization.UI;
using Object = UnityEngine.Object;

namespace UMSAGL.Scripts
{
    public class Graph : MonoBehaviour
    {
        public GameObject nodePrefab;
        public GameObject edgePrefab;
        public float factor = 0.2f;
        public Vector2 margins;

        protected GeometryGraph _graph;
        private LayoutAlgorithmSettings _settings;

        private Task _graphTask;
        private bool _reroute;
        private bool _redraw;
        private bool _reposition;
        private bool _relayout;

        public Transform units;
        public GameObject panel;

        public Vector2 Rect
        {
            get
            {
                _graph.UpdateBoundingBox();
                var size = _graph.BoundingBox.Size;
                return new Vector2(ToUnitySpace((float)size.Width) + margins.x,
                    ToUnitySpace((float)size.Height) + margins.y);
            }
        }

        public void Center()
        {
            if (!units && !TryGetUnits())
                return;
            _graph.UpdateBoundingBox();
            units.localPosition =
                new Vector3(ToUnitySpace(_graph.BoundingBox.Center.X), ToUnitySpace(_graph.BoundingBox.Center.Y)) * -1.0f;
        }

        private bool TryGetUnits()
        {
            if (!transform.Find("Units"))
                return false;
            else
            {
                units = transform.Find("Units");
                return true;
            }
        }

        public GameObject AddNode()
        {
            // TODO: depending on type of node use another prefab
            var go = Instantiate(nodePrefab, units);
            if (UIEditorManager.Instance.active)
                go.GetComponentsInChildren<Button>(includeInactive: true).ForEach(x => x.gameObject.SetActive(true));

            //Following step required otherwise Size will return wrong rect
            Canvas.ForceUpdateCanvases();

            var unode = go.GetComponent<UNode>();
            var w = ToGraphSpace(unode.Size.width);
            var h = ToGraphSpace(unode.Size.height);

            var node = new Node(CurveFactory.CreateRectangle(w, h, new Point()))
            {
                UserData = go
            };
            unode.GraphNode = node;
            _graph.Nodes.Add(node);

            return go;
        }

        public virtual void RemoveNode(GameObject node)
        {
            var graphNode = node.GetComponent<UNode>().GraphNode;
            foreach (var edge in graphNode.Edges)
            {
                Destroy((GameObject)edge.UserData);
                //in MSAGL edges are automatically removed, only UnityObjects have to be removed
            }

            _graph.Nodes.Remove(graphNode);
            Destroy(node);
        }

        public void AddEdge(GameObject from, GameObject to)
        {
            AddEdge(from, to, edgePrefab);
        }

        public GameObject AddEdge(GameObject from, GameObject to, GameObject prefab)
        {
            var go = Instantiate(prefab, units);
            go.transform.SetSiblingIndex(0);
            var uEdge = go.GetComponent<UEdge>();

            var edge = new Edge(from.GetComponent<UNode>().GraphNode, to.GetComponent<UNode>().GraphNode)
            {
                LineWidth = ToGraphSpace(uEdge.Width),
                UserData = go
            };
            go.name += edge.ToString();
            uEdge.GraphEdge = edge;
            _graph.Edges.Add(edge);
            // We cannot wait (i.e. use UpdateGraph()), or the edge will be drawn too late
            LayoutHelpers.RouteAndLabelEdges(_graph, _settings, _graph.Edges);
            RedrawEdges();
            return go;
        }

        public void RemoveEdge(GameObject edge)
        {
            _graph.Edges.Remove(edge.GetComponent<UEdge>().GraphEdge);
            Destroy(edge);
        }

        private double ToGraphSpace(float x)
        {
            return x / factor;
        }

        protected float ToUnitySpace(double x)
        {
            return (float)x * factor;
        }

        protected virtual void Awake()
        {
            _graph = new GeometryGraph();
            units = transform.Find("Units"); //extra object to center graph

            panel = GameObject.Find("Panel");

            _settings = new SugiyamaLayoutSettings();
            _settings.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.RectilinearToCenter;
            GetComponent<Canvas>().worldCamera = Camera.main;
        }

        private void PositionNodes()
        {
            foreach (var node in _graph.Nodes)
            {
                var go = (GameObject)node.UserData;
                go.transform.localPosition = new Vector3(ToUnitySpace(node.Center.X), ToUnitySpace(node.Center.Y), 0.0f);
            }
        }

        private void UpdateNodes()
        {
            Canvas.ForceUpdateCanvases();
            foreach (var node in _graph.Nodes)
            {
                var go = (GameObject)node.UserData;
                var localPosition = go.transform.localPosition;
                node.Center = new Point(ToGraphSpace(localPosition.x),
                    ToGraphSpace(localPosition.y));
                var unode = go.GetComponent<UNode>();
                node.BoundingBox = new Rectangle(new Size(ToGraphSpace(unode.Size.width), ToGraphSpace(unode.Size.height)),
                    node.Center);
            }
        }

        protected virtual void RedrawEdges()
        {
            foreach (var edge in _graph.Edges)
            {
                var vertices = new List<Vector2>();
                var go = (GameObject)edge.UserData;
                
                // Dumb fix to display endCaps
                go.transform.GetComponentsInChildren<UIPolygon>().ForEach(x =>
                    x.GetComponent<RectTransform>().sizeDelta = new Vector2 (20.01f, 20.01f));
                
                go.transform.GetComponentsInChildren<UIPolygon>().ForEach(x =>
                    x.GetComponent<RectTransform>().sizeDelta = new Vector2 (20, 20));
                
                
                switch (edge.Curve)
                {
                    case Curve curve:
                    {
                        var p = curve[curve.ParStart];
                        vertices.Add(new Vector3(ToUnitySpace(p.X), ToUnitySpace(p.Y), 0));
                        foreach (var seg in curve.Segments)
                        {
                            p = seg[seg.ParEnd];
                            vertices.Add(new Vector3(ToUnitySpace(p.X), ToUnitySpace(p.Y), 0));
                        }

                        break;
                    }
                    case LineSegment ls:
                    {
                        var p = ls.Start;
                        vertices.Add(new Vector3(ToUnitySpace(p.X), ToUnitySpace(p.Y)));
                        p = ls.End;
                        vertices.Add(new Vector3(ToUnitySpace(p.X), ToUnitySpace(p.Y)));
                        break;
                    }
                }

                go.GetComponent<UEdge>().Points = vertices.ToArray();
            }
        }

        private static async void Forget(Task t, Action f = null)
        {
            try
            {
                await t;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            f?.Invoke();
        }

        public void UpdateGraph()
        {
            _reroute = true;
        }

        public void Layout()
        {
            _relayout = true;
        }

        private void Update()
        {
            if (_reposition)
            {
                PositionNodes();
                Center();
                _reposition = false;
            }

            if (_redraw)
            {
                RedrawEdges();
                _redraw = false;
            }

            if (_relayout)
            {
                if (_graphTask == null)
                {
                    UpdateNodes();
                    _graphTask = Task.Run(() =>
                    {
                        LayoutHelpers.CalculateLayout(_graph, _settings, null);
                        LayoutHelpers.RouteAndLabelEdges(_graph, _settings, _graph.Edges);
                    });
                    Forget(_graphTask, () =>
                    {
                        _graphTask = null;
                        _reposition = true;
                        _redraw = true;
                    });
                    _relayout = false;
                }
            }

            if (!_reroute) return;
            if (_graphTask != null) return;
            UpdateNodes();
            _graphTask = Task.Run(() => LayoutHelpers.RouteAndLabelEdges(_graph, _settings, _graph.Edges));
            Forget(_graphTask, () =>
            {
                _graphTask = null;
                _redraw = true;
            });
            _reroute = false;
        }
    }
}
