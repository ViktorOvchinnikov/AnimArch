using System.Collections.Generic;
using OALProgramControl;

namespace Visualization.ClassDiagram.MarkedDiagram
{
    public class CDClassMarked : MarkingDecorator<CDClass>
    {
        public List<CDMethodMarked> WrappedMethods { get; set; }
        public List<MarkingDecorator<CDAttribute>> WrappedAttributes { get; set; }

        public CDClassMarked(CDClass inner) : base(inner)
        {
            this.Inner = inner;
            this.WrappedMethods = new List<CDMethodMarked>();
            this.WrappedAttributes = new List<MarkingDecorator<CDAttribute>>();
        }
    }
}