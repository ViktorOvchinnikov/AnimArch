using UnityEngine;
public class ChangeManager : MonoBehaviour
{    
    public void AcceptChange(Change change)
    {
        if (change == null) return;
        change.Accept();
        if (change.ChangeIsDeletion)
        {
            //delete object
        }
        Debug.Log($"Change '{change.Name}' of type '{change.Type}' has been ACCEPTED.");
    }

    public void RejectChange(Change change)
    {
        if (change == null) return;
        change.Reject();
        Debug.Log($"Change '{change.Name}' of type '{change.Type}' has been REJECTED.");
    }
}
