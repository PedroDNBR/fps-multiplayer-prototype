using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legs : BodyMember
{
    HealthManager healthManager;
    public void Awake()
    {
        healthManager = GetComponentInParent<HealthManager>();
    }

    public override void TakeDamage(int damage, ulong clienId)
    {
        healthManager.TakeDamageLegsServerRpc(damage, clienId);
    }
}
