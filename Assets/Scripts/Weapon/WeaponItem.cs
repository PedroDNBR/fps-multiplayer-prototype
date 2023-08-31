using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon Item")]
public class WeaponItem : ScriptableObject
{
    [Header("Weapon Info")]
    public string name;
    public int maxMagazineAmmo;
    public float fireRate;
    public WeaponTypes weaponType = new WeaponTypes();
    public float baseDamage = 15;
    public GameObject weaponPrefab;
    public GameObject bullet;

    [Header("Weapon Properies")]
    public Vector3 gunAimPosition;
    public Quaternion gunAimRotation;
    public Vector3 cameraAimPosition;
    public float aimSpeed = 2;
    public Vector3 positionAwayFromCollision;
    public Quaternion rotationAwayFromCollision;

    [Header("Weapon Recoil Properies")]
    public float weaponSnap = .1f;
    public float kickback = 1f;
    public float randomKick = .1f;
    public float horizontalRecoilMultiplier = 3f;

    [Header("Camera Recoil Properies")]
    public float cameraSnap = 1f;
    public float cameraKickback = 1f;
    public float cameraRandomKick = 9f;
    public float cameraHorizontalRecoilMultiplier = 0.1f;
    public float lerpSpeed = 2;

    [Header("Weapon Spring Effect")]
    public float POSITION_SPRING = 150f;
    public float POSITION_DRAG = 20f;
    public float MAX_POSITION_OFFSET = 0.2f;
    public int POSITION_SPRING_ITERAIONS = 8;
    public float ROTATION_SPRING = 70f;
    public float ROTATION_DRAG = 6f;
    public float MAX_ROTATION_OFFSET = 20f;
    public int ROTATION_SPRING_ITERAIONS = 8;
    public float ROTATION_IMPULSE_GAIN = 300f;
}

public enum WeaponTypes
{
    Rifle,
    Pistol,
};
