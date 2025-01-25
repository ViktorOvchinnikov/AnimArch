using UnityEngine; 
using UnityEngine.UI;


public class AcceptChanges : MonoBehaviour
{
    public void SaveChanges()
    {
        GameObject currentObject = gameObject;
        Transform background2 = currentObject.transform.GetChild(0);
        background2.gameObject.GetComponent<Image>().color = new Color(0f, 0f, 1f, 0.5f);
        Transform button2 = currentObject.transform.GetChild(0).GetChild(0).GetChild(0);
        Transform button = currentObject.transform.GetChild(0).GetChild(0).GetChild(1);
        button.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
    }
}