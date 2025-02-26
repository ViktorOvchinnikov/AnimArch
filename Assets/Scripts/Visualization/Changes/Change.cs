public class Change
{
    public ChangeType Type { get; set; }
    public string Name { get; set; }
    public bool ChangeIsDeletion { get; private set; }

    public bool IsAccepted { get; private set; }
    public bool IsRejected { get; private set; }

    public Change(ChangeType type, string name, bool changeIsDeletion)
    {
        Type = type;
        Name = name;
        IsAccepted = false;
        IsRejected = false;
        ChangeIsDeletion = changeIsDeletion;
    }

    public void Accept()
    {
        IsAccepted = true;
        IsRejected = false;
    }

    public void Reject()
    {
        IsRejected = true;
        IsAccepted = false;
    }
}
