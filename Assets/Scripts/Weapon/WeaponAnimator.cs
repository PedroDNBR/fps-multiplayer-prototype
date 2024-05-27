using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{

    public AudioClip insertMagazineSound;
    public AudioClip removeMagazineSound;

    public AudioClip cokingChargingHandleSound;

    public Transform magazine;
    public Transform chargingHandle;

    public void PlayInsertMagazineSound()
    {
        AudioSource.PlayClipAtPoint(insertMagazineSound, magazine.position);
    }

    public void PlayRemoveMagazineSound()
    {
        AudioSource.PlayClipAtPoint(removeMagazineSound, magazine.position);
    }

    public void PlayCokingChargingHandleSound()
    {
        AudioSource.PlayClipAtPoint(cokingChargingHandleSound, chargingHandle.position);
    }
}
