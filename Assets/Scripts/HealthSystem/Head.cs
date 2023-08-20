using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : BodyMember
{
    HealthManager healthManager;
    public void Awake()
    {
        healthManager = GetComponentInParent<HealthManager>();
    }

    public override void TakeDamage(int damage, ulong clienId)
    {
        healthManager.TakeDamageHeadServerRpc(damage, clienId);
    }
}
