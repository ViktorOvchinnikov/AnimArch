using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Visualization.ClassDiagram.ClassComponents
{
    [Serializable]
    public class Class
    {
        public string Name;
        [FormerlySerializedAs("XmiId")] public string Id;
        public string Visibility;
        public string NameSpc;
        public string Geometry;
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public string Type;
        public List<Attribute> Attributes;
        public List<Method> Methods;
        public ClassHighlightSubject HighlightSubject { get; private set;}

        public Class()
        {
            this.HighlightSubject = new ClassHighlightSubject();
        }

        public Class(string id)
        {
            Name = "NewClass_" + id;
            Id = id;
            Type = "uml:Class";
            Attributes = new List<Attribute>();
            Methods = new List<Method>();
            this.HighlightSubject = new ClassHighlightSubject();

        }
        public Class(string name, string id)
        {
            Name = name;
            Id = id;
            Type = "uml:Class";
            Attributes = new List<Attribute>();
            Methods = new List<Method>();
            this.HighlightSubject = new ClassHighlightSubject();
        }

        protected bool Equals(Class other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Class)obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
        public Class Copy()
        {
            return new Class
            {
                Name = this.Name,
                Id = this.Id,
                Visibility = this.Visibility,
                NameSpc = this.NameSpc,
                Geometry = this.Geometry,
                Left = this.Left,
                Right = this.Right,
                Top = this.Top,
                Bottom = this.Bottom,
                Type = this.Type,
                Attributes = this.Attributes != null ? new List<Attribute>(this.Attributes) : new List<Attribute>(), // Check for null
                Methods = this.Methods != null ? new List<Method>(this.Methods) : new List<Method>(), // Check for null
                HighlightSubject = this.HighlightSubject != null ? new ClassHighlightSubject() : null // Check for null or initialize as needed
            };
        }

    }
}
