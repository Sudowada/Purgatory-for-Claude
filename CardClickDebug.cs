using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickDebug : MonoBehaviour, 
    IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData e)
    {
        Debug.Log("Mouse entered card: " + gameObject.name);
    }

    public void OnPointerClick(PointerEventData e)
    {
        Debug.Log("Card clicked: " + gameObject.name);
    }
}