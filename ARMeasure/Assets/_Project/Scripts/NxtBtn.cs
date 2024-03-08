using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NxtBtn : MonoBehaviour, IPointerClickHandler
{
    public int btnNumber;
    public void OnPointerClick(PointerEventData eventData)
    {
        LoginController.instance.SetActiveFlow2(btnNumber);
    }
}
