using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeFirstLens : MonoBehaviour
{
    public Transform playerTarget;
    public Transform scope;

    // Update is called once per frame
    void Update()
    {
        if (playerTarget == null) return;
        Vector3 localPlayer = scope.InverseTransformPoint(playerTarget.position);

        Vector3 lookAtScope = scope.TransformPoint(new Vector3(-localPlayer.x, -localPlayer.y, -localPlayer.z));
        transform.LookAt(lookAtScope);
    }
}
