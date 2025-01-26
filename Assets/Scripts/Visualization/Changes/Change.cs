public class Change
{
    public ChangeType Type { get; set; }
    public string Name { get; set; }
    public bool IsAccepted { get; private set; }
    public bool IsRejected { get; private set; }

    public Change(ChangeType type, string name, string description)
    {
        Type = type;
        Name = name;
        IsAccepted = false;
        IsRejected = false;
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
