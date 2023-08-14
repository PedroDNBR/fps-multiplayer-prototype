using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thorax : BodyMember
{
    HealthManager healthManager;
    public void Awake()
    {
        healthManager = GetComponentInParent<HealthManager>();
    }

    public override void TakeDamage(int damage)
    {
        healthManager.TakeDamageThoraxServerRpc(damage);
    }
}