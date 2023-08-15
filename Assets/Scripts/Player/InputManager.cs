using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float vertical;
    public float horizontal;

    public float mouseX;
    public float mouseY;

    public float deltaTime;

    public bool leftMouse;
    public bool rightMouse;

    public bool r;

    public Transform myTransform;

    public bool tab;
    public bool c;

    public bool e;
    public bool q;

    public float scroll;
    public bool lShift;

    public bool spaceBar;

    public void Init()
    {
        myTransform = transform;
    }

    public void HandleInputs()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        leftMouse = Input.GetButton("Fire1");
        rightMouse = Input.GetButton("Fire2");

        spaceBar = Input.GetKeyDown(KeyCode.Space);

        r = Input.GetKeyDown(KeyCode.R);

        tab = Input.GetKeyDown(KeyCode.Tab);

        c = Input.GetKey(KeyCode.C);

        e = Input.GetKey(KeyCode.E);
        q = Input.GetKey(KeyCode.Q);

        lShift = Input.GetKey(KeyCode.LeftShift);

        deltaTime = Time.deltaTime;

        scroll = Input.mouseScrollDelta.y;
    }
}
