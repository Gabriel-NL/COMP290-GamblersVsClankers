using UnityEngine;

public class DragAndDropV2 : MonoBehaviour,IHoldableAndClickable
{
    private IHoldableAndClickable _holdableAndClickableImplementation;
    
    public void Hold()
    {
        Debug.Log("Hold on slot");
    }

    public void LeftClick()
    {
        Debug.Log("Left click on slot");
    }

    public void RightClick()
    {
        Debug.Log("Right click on slot");
    }
}
