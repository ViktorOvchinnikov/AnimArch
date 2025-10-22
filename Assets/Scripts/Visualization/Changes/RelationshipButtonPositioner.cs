using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class RelationshipButtonPositioner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Distance from line in pixels. Use 0 to place buttons directly on the line.")]
    private float offsetFromLine = 0f;
    
    [SerializeField]
    [Tooltip("If true, buttons will be on the right/bottom side of the line. If false, on the left/top side.")]
    private bool invertOffset = false;
    
    private UILineRenderer lineRenderer;
    private Transform changesVisualizationContainer;
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<UILineRenderer>();
        }
        
        if (changesVisualizationContainer == null)
        {
            changesVisualizationContainer = transform.Find("ChangesVisualization");
        }
        
        if (lineRenderer == null)
        {
            Debug.LogWarning($"UILineRenderer not found on {gameObject.name}");
            return;
        }
        
        if (changesVisualizationContainer == null)
        {
            Debug.LogWarning($"ChangesVisualization container not found on {gameObject.name}");
            return;
        }
        
        PositionButtons();
    }
    
    void Update()
    {
        PositionButtons();
    }
    
    private void PositionButtons()
    {
        if (lineRenderer == null || changesVisualizationContainer == null) return;
        
        var points = lineRenderer.Points;
        if (points.Length < 2) return;
        
        var prev = points.First();
        var maxDistance = float.MinValue;
        Vector2 first = default;
        Vector2 second = default;
        
        foreach (var next in points.Skip(1))
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
        
        Vector2 centerPosition = Vector2.Lerp(first, second, 0.5f);
        
        // Calculate perpendicular offset to position buttons closer to the line
        Vector2 direction = (second - first).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        if (invertOffset)
        {
            perpendicular = -perpendicular;
        }
        Vector2 offset = perpendicular * offsetFromLine;
        
        changesVisualizationContainer.localPosition = centerPosition + offset;
        
        RotateButtonsToLine(first, second);
    }
    
    private void RotateButtonsToLine(Vector2 first, Vector2 second)
    {
        if (changesVisualizationContainer == null) return;
        
        Vector2 direction = (second - first).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        changesVisualizationContainer.localEulerAngles = new Vector3(0, 0, angle + 90f);
    }
    
    public void InitializeButtons()
    {
        if (changesVisualizationContainer == null) return;
        
        var acceptButton = changesVisualizationContainer.Find("AcceptButton");
        var declineButton = changesVisualizationContainer.Find("DeleteButton");
        
        if (acceptButton == null)
        {
            Debug.LogWarning($"AcceptButton not found in ChangesVisualization on {gameObject.name}");
        }
        
        if (declineButton == null)
        {
            Debug.LogWarning($"DeleteButton not found in ChangesVisualization on {gameObject.name}");
        }
    }
    
    
    
    public void ActivateButtons()
    {
        if (changesVisualizationContainer == null) return;
        
        var acceptButton = changesVisualizationContainer.Find("AcceptButton");
        var declineButton = changesVisualizationContainer.Find("DeleteButton");
        
        if (acceptButton != null) acceptButton.gameObject.SetActive(true);
        if (declineButton != null) declineButton.gameObject.SetActive(true);
    }
    
    public void DeactivateButtons()
    {
        if (changesVisualizationContainer == null) return;
        
        var acceptButton = changesVisualizationContainer.Find("AcceptButton");
        var declineButton = changesVisualizationContainer.Find("DeleteButton");
        
        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (declineButton != null) declineButton.gameObject.SetActive(false);
    }
    
    public void SetOffsetFromLine(float offset, bool invert = false)
    {
        offsetFromLine = offset;
        invertOffset = invert;
    }
    
}
