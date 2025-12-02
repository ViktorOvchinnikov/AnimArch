using System;

namespace EditorChangesHistory
{
    public enum ChangeType
    {
        AddClass,
        RemoveClass,
        UpdateClass,
        AddMethod,
        RemoveMethod,
        UpdateMethod,
        AddAttribute,
        RemoveAttribute,
        UpdateAttribute,
        AddRelation,
        RemoveRelation,
    }

    public class DiagramChangeEvent
    {
        public ChangeType Type { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }

        public DiagramChangeEvent(ChangeType type, string data)
        {
            Type = type;
            Data = data;
            Timestamp = DateTime.Now;
        }
    }
}
