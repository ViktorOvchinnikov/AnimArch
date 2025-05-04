using System.Collections.Generic;
using OALProgramControl;

namespace Visualization.ClassDiagram.MarkedDiagram
{
    public class CDMethodMarked : MarkingDecorator<CDMethod>
    {
        public List<MarkingDecorator<CDParameter>> wrappedParameters { get; set; }
        
        public CDMethodMarked(CDMethod inner) : base(inner)
        {
            this.Inner = inner;
        }
    }
}