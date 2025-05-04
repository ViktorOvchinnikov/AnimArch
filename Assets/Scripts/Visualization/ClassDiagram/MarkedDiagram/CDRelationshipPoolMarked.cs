using System.Collections.Generic;
using System.Linq;
using OALProgramControl;

namespace Visualization.ClassDiagram.MarkedDiagram
{
    public class CDRelationshipPoolMarked
    {
        private List<MarkingDecorator<CDRelationship>> RelationshipPool { get; }

        public CDRelationshipPoolMarked()
        {
            RelationshipPool = new List<MarkingDecorator<CDRelationship>>();
        }

        public void Add(MarkingDecorator<CDRelationship> relationship)
        {
            RelationshipPool.Add(relationship);
        }
        
        public List<MarkingDecorator<CDRelationship>> GetAllRelationships() => RelationshipPool.ToList();
        
    }
}