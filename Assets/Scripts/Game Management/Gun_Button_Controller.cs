using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Gun_Button_Controller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
	public UnityAction onPointerEnter;
	public UnityAction onPointerExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit?.Invoke();
    }
}
