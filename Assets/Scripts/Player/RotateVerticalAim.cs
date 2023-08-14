using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateVerticalAim : MonoBehaviour
{
    public InputManager inputManager;

    float xRotation;
    // Start is called before the first frame update
    public void Init()
    {
    }

    public void Update()
    {
        float mouseY = inputManager.mouseY * 100 * inputManager.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -260, -120);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
