using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.VFX;

public class WeaponPrefab : MonoBehaviour
{
    public Transform recoilPivot;
    public Transform aimPivot;
    public Transform pointAwayFromWallsPivot;
    public Transform weaponTransform;

    public VisualEffect muzzleFire;
    public Transform muzzle;
    public Transform wallDetector;
    public Transform closestWallDetector;

    public Transform gunIkTransform;
    public Transform leftHandIkTransform;
    public Transform rightHandIkTransform;

    public Animator weaponAnimator;
    public Animator weaponMovementAnimator;
}
