using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour
{

    private void Start()
    {
        Toggle(false);
    }

    public void SetDragImage(Sprite sprite)
    {
        GetComponent<Image>().sprite = sprite;
    }

    private void Update()
    {
        transform.position = Mouse.current.position.ReadValue();
    }

    public void Toggle(bool enable)
    {
        if (enable)
        {
            transform.position = Mouse.current.position.ReadValue();
        }
        gameObject.SetActive(enable);
    }
}
