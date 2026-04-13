using System.Collections;
using System.Linq;
using Microsoft.Msagl.Core.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Visualization.ClassDiagram;
using Visualization.UI;

namespace UMSAGL.Scripts
{
    public class UEdge : Unit
    {
        private const string DeleteButtonObjectName = "DeleteButton";
        private const string EditButtonObjectName = "EditButton";
        private const float EditButtonOffsetX = 34f;

        public GameObject startCap;
        public GameObject endCap;
        public GameObject deleteButton;
        public GameObject editButton;
        public bool dashed;
        public float segmentLength = 10f;

        private UILineRenderer _lineRenderer;
        private bool _dashed = false;
        private float _segmentLength = 0f;
        private Transform _deleteButtonTransform;
        private Transform _editButtonTransform;
        private Button _deleteButton;
        private Button _editButton;

        public Edge GraphEdge { get; set; }

        private float SegmentLength
        {
            get => _segmentLength;
            set
            {
                _segmentLength = segmentLength = Mathf.Max(value, 1.0f);
                Dashed = dashed;
            }
        }

        public float Width => Mathf.Max(_lineRenderer.lineThickness, 10f);

        private bool Dashed
        {
            get => _dashed;
            set
            {
                _dashed = dashed = value;
                Dash(value);
            }
        }

        public Vector2[] Points
        {
            get => _lineRenderer.Points;
            set
            {
                _lineRenderer.Points = value;
                Dashed = dashed;
                UpdateCaps();
            }
        }

        protected override void OnDestroy()
        {
            // TODO: client edge class?
            if (_graph)
                _graph.RemoveEdge(gameObject);
        }

        private void Dash(bool active)
        {
            if (active && Points.Length > 1)
            {
                _lineRenderer.LineList = true;
                _lineRenderer.ImproveResolution = ResolutionMode.PerLine;
                var prev = Points.First();
                var totalDistance = 0f;
                foreach (var next in Points.Skip(1))
                {
                    totalDistance += Vector2.Distance(prev, next);
                    prev = next;
                }

                _lineRenderer.Resoloution = totalDistance / segmentLength;
            }
            else
            {
                _lineRenderer.LineList = false;
                _lineRenderer.ImproveResolution = ResolutionMode.None;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _lineRenderer = GetComponent<UILineRenderer>();
            Points = _lineRenderer.Points;
            SegmentLength = segmentLength;
            UpdateCaps();
            SetupDeleteButton();
            SetupEditButton();
        }

        private static float CapAngle(Vector2 p1, Vector2 p2)
        {
            float eulerAngle;
            if (Mathf.Abs(p1.x - p2.x) > Mathf.Abs(p1.y - p2.y))
            {
                eulerAngle = p1.x > p2.x ? 180f : 0f;
            }
            else
            {
                eulerAngle = p1.y > p2.y ? 270f : 90f;
            }

            return eulerAngle;
        }

        private void UpdateCap(GameObject capPrefab, string capName, Vector2 targetPoint, Vector2 dirPoint)
        {
            if (Points.Length <= 1) return;
            var capTransform = transform.Find(capName);
            if (capPrefab != null)
            {
                if (capTransform == null)
                {
                    capTransform = Instantiate(capPrefab, transform).transform;
                    capTransform.name = capName;
                    if (capTransform.transform.childCount > 0)
                        StartCoroutine(QuickFix(capTransform.transform.GetChild(0).gameObject));
                    capTransform.GetComponentInChildren<UIPolygon>()?.SetAllDirty();
                    Canvas.ForceUpdateCanvases();
                    capTransform.gameObject.SetActive(true);
                }

                var angle = CapAngle(dirPoint, targetPoint);
                capTransform.localEulerAngles = new Vector3(0, 0, angle);
                capTransform.localPosition = new Vector3(targetPoint.x, targetPoint.y, 0);
            }
            else if (capTransform != null)
            {
                Destroy(capTransform.gameObject);
                Canvas.ForceUpdateCanvases();
            }
        }

        //Fix used to minimize relation displaying bug
        private IEnumerator QuickFix(GameObject g)
        {
            yield return new WaitForSeconds(0.05f);
            g.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            g.SetActive(true);
        }

        private void UpdateCaps()
        {
            if (Points.Length <= 1) return;
            UpdateCap(startCap, "StartCap", Points[0], Points[1]);
            UpdateCap(endCap, "EndCap", Points[^1], Points[^2]);
        }

        private void UpdateRelationButtonsPosition()
        {
            if (Points == null || Points.Length < 2)
            {
                return;
            }

            var prev = Points.First();
            var maxDistance = float.MinValue;
            Vector2 first = default;
            Vector2 second = default;

            foreach (var next in Points.Skip(1))
            {
                var dis = Vector2.Distance(prev, next);
                if (dis > maxDistance)
                {
                    maxDistance = dis;
                    first = prev;
                    second = next;
                }

                prev = next;
            }

            Vector3 centerPosition = Vector2.Lerp(first, second, 0.5f);
            if (_deleteButtonTransform != null)
            {
                _deleteButtonTransform.localPosition = centerPosition;
            }

            if (_editButtonTransform != null)
            {
                _editButtonTransform.localPosition = centerPosition + new Vector3(EditButtonOffsetX, 0f, 0f);
            }
        }

        private void Update()
        {
            if (SegmentLength != segmentLength)
            {
                SegmentLength = segmentLength;
            }
            else if (Dashed != dashed)
            {
                Dashed = dashed;
            }

            UpdateRelationButtonsPosition();
            UpdateEditButtonVisibility();
        }

        public void ChangeColor(Color c)
        {
            _lineRenderer.color = c;
            if (endCap == null && startCap == null) return;
            var p = GetComponentsInChildren<UIPolygon>();
            if (p != null)
            {
                foreach (var pol in p)
                {
                    pol.color = c;
                }
            }

            var lr = GetComponentsInChildren<UILineRenderer>();
            if (lr == null) return;
            foreach (var l in lr)
            {
                l.color = c;
            }
        }

        public void SetupDeleteButton()
        {
            if (_deleteButtonTransform != null)
            {
                return;
            }

            var deleteButtonGo = Instantiate(deleteButton, transform);
            deleteButtonGo.name = DeleteButtonObjectName;
            _deleteButtonTransform = deleteButtonGo.transform;

            _deleteButton = deleteButtonGo.transform.Find("DeleteButton")?.GetComponent<Button>();
            if (_deleteButton == null)
            {
                _deleteButton = deleteButtonGo.GetComponentInChildren<Button>(true);
            }

            if (_deleteButton != null)
            {
                _deleteButton.onClick.RemoveAllListeners();
                _deleteButton.onClick.AddListener(DeleteEdge);
                _deleteButton.gameObject.SetActive(UIEditorManager.Instance != null && UIEditorManager.Instance.active);
            }
        }

        private void SetupEditButton()
        {
            if (_editButtonTransform != null)
            {
                return;
            }

            var editButtonPrefab = editButton != null ? editButton : deleteButton;
            if (editButtonPrefab == null)
            {
                return;
            }

            var editButtonGo = Instantiate(editButtonPrefab, transform);
            editButtonGo.name = EditButtonObjectName;
            _editButtonTransform = editButtonGo.transform;

            _editButton = editButtonGo.transform.Find("EditButton")?.GetComponent<Button>();
            if (_editButton == null)
            {
                _editButton = editButtonGo.transform.Find("DeleteButton")?.GetComponent<Button>();
            }

            if (_editButton == null)
            {
                _editButton = editButtonGo.GetComponentInChildren<Button>(true);
            }

            if (_editButton == null)
            {
                return;
            }

            _editButton.gameObject.name = "EditRelationButton";
            //ApplyEditButtonStyle(_editButton);
            _editButton.onClick.RemoveAllListeners();
            _editButton.onClick.AddListener(EditEdge);
            _editButton.gameObject.SetActive(UIEditorManager.Instance != null && UIEditorManager.Instance.active);
        }
        
        private void UpdateEditButtonVisibility()
        {
            if (_editButton == null)
            {
                return;
            }

            bool isInEditMode = UIEditorManager.Instance != null && UIEditorManager.Instance.active;
            bool isSuggestionVisualized = IsSuggestionVisualizationActive();
            _editButton.gameObject.SetActive(isInEditMode && !isSuggestionVisualized);
        }

        private bool IsSuggestionVisualizationActive()
        {
            Transform changesContainer = transform.Find("ChangesVisualization");
            if (changesContainer == null)
            {
                return false;
            }

            Transform acceptButton = changesContainer.Find("AcceptButton");
            Transform declineButton = changesContainer.Find("DeleteButton");
            bool acceptVisible = acceptButton != null && acceptButton.gameObject.activeSelf;
            bool declineVisible = declineButton != null && declineButton.gameObject.activeSelf;
            return acceptVisible || declineVisible;
        }

        private void DeleteEdge()
        {
            UIEditorManager.Instance.confirmPopUp.ActivateCreation(delegate { UIEditorManager.Instance.mainEditor.DeleteRelation(gameObject); });
        }

        private void EditEdge()
        {
            UIEditorManager.Instance.BeginRelationTypeEdit(gameObject);
        }
    }
}
