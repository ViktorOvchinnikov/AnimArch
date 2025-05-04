using System.Collections.Generic;
using OALProgramControl;

namespace Visualization.ClassDiagram.MarkedDiagram
{
    public class CDClassPoolMarked
    {
        private List<CDClassMarked> CDClassPool { get; }

        public CDClassPoolMarked()
        {
            CDClassPool = new List<CDClassMarked>();
        }

        public void Add(CDClassMarked _class)
        {
            CDClassPool.Add(_class);
        }

        public List<CDClassMarked> GetClassPool()
        {
            return CDClassPool;
        }
    }
}