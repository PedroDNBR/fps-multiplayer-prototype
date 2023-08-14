using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arms : BodyMember
{
    HealthManager healthManager;
    public void Awake()
    {
        healthManager = GetComponentInParent<HealthManager>();
    }

    public override void TakeDamage(int damage)
    {
        healthManager.TakeDamageArmsServerRpc(damage);
    }
}