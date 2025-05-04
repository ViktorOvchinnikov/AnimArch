namespace Visualization.ClassDiagram
{
    public class MarkingDecorator<T>
    {
        public T Inner;

        public bool UpdateMark;
        public bool DeleteMark;
        public bool CreateMark;
        
        public MarkingDecorator(T inner)
        {
            Inner = inner;
        }

        public void SetUpdateMark()
        {
            this.UpdateMark = true;
        }
        
        public void SetDeleteMark()
        {
            this.DeleteMark = true;
        }
        
        public void SetCreateMark()
        {
            this.CreateMark = true;
        }
    }
}