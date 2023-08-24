using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class WeaponManager : NetworkBehaviour
{
    InputManager inputManager;
    WeaponItem currentWeapon;
    AnimatorManager animatorManager;
    PlayerManager playerManager;
    float nextShoot;
    int magazine = 0;

    public LayerMask wallMask;

    public Transform recoilPivot;

    public Transform cameraPivot;

    public Transform aimPivot;
    public Transform cameraAimPivot;

    public Transform pointAwayFromWallsPivot;

    public Transform muzzle;
    public Transform wallDetector;
    public Transform closestWallDetector;
    public VisualEffect muzzleFire;

    public MultiParentConstraint gunIk;
    public TwoBoneIKConstraint leftHandIk;
    public TwoBoneIKConstraint rightHandIk;

    Transform movementProbe;
    Vector3 lastPosition = Vector3.zero;
    Vector3 lastRotation = Vector3.zero;

    Spring positionSpring;
    Spring rotationSpring;

    Vector3 localOrigin = Vector3.zero;

    public bool keepAiming = false;

    public Animator weaponAnimator;

    GameObject currentWeaponPrefab;

    [SerializeField] List<GameObject> spanwedBullets = new List<GameObject>();

    public bool isReloading;

    public override void OnNetworkSpawn()
    {
        cameraPivot.gameObject.SetActive(IsOwner);
        cameraPivot.GetComponent<AudioListener>().enabled = IsOwner;
        base.OnNetworkSpawn(); // Not sure if this is needed though, but good to have it.
    }

    // Start is called before the first frame update
    public void Init(InputManager inputManager, AnimatorManager animatorManager, PlayerManager playerManager)
    {
        // muzzleFire = muzzle.GetComponentInChildren<VisualEffect>();
        this.playerManager = playerManager;
        this.inputManager = inputManager;
        this.animatorManager = animatorManager;
    }

    public void SetupWeapon(WeaponItem weaponItem)
    {
        currentWeapon = weaponItem;

        if (!currentWeapon) return;

        if (currentWeaponPrefab != null) Destroy(currentWeaponPrefab);

        WeaponSpanwerParent spawner = GetComponentInChildren<WeaponSpanwerParent>();

        GameObject prefab = Instantiate(currentWeapon.weaponPrefab, spawner.transform);

        WeaponPrefab weaponPrefab = prefab.GetComponent<WeaponPrefab>();

        var data = gunIk.data.sourceObjects;
        data.Clear();
        data.Add(new WeightedTransform(weaponPrefab.gunIkTransform, 1));
        gunIk.data.sourceObjects = data;
        gunIk.data.constrainedObject = weaponPrefab.weaponTransform;
        Debug.Log(gunIk.data.sourceObjects[0].transform);
        leftHandIk.data.target = weaponPrefab.leftHandIkTransform;
        rightHandIk.data.target = weaponPrefab.rightHandIkTransform;

        muzzle = weaponPrefab.muzzle;
        muzzleFire = weaponPrefab.muzzleFire;
        wallDetector = weaponPrefab.wallDetector;
        closestWallDetector = weaponPrefab.closestWallDetector;
        pointAwayFromWallsPivot = weaponPrefab.pointAwayFromWallsPivot;
        recoilPivot = weaponPrefab.recoilPivot;
        aimPivot = weaponPrefab.aimPivot;

        weaponAnimator = weaponPrefab.weaponAnimator;
        animatorManager.weaponAnimator = weaponPrefab.weaponAnimator;
        animatorManager.weaponMovementAnimator = weaponPrefab.weaponMovementAnimator;

        currentWeaponPrefab = prefab;

        animatorManager.GetComponent<RigBuilder>().Build();
        animatorManager.PlayTargetAnimation("IdleHoldingGun", 3);

        magazine = currentWeapon.maxMagazineAmmo;

        SetupSprings();
    }

    void SetupSprings()
    {
        this.movementProbe = recoilPivot;

        this.positionSpring = new Spring(currentWeapon.POSITION_SPRING,
            currentWeapon.POSITION_DRAG,
            -Vector3.one * currentWeapon.MAX_POSITION_OFFSET,
            Vector3.one * currentWeapon.MAX_POSITION_OFFSET,
            currentWeapon.POSITION_SPRING_ITERAIONS
        );

        this.rotationSpring = new Spring(
            currentWeapon.ROTATION_SPRING,
            currentWeapon.ROTATION_DRAG,
            -Vector3.one * currentWeapon.MAX_ROTATION_OFFSET,
            Vector3.one * currentWeapon.MAX_ROTATION_OFFSET,
            currentWeapon.ROTATION_SPRING_ITERAIONS
        );

        this.lastPosition = this.movementProbe.position;
        this.lastRotation = this.movementProbe.eulerAngles;
    }

    public void WeaponExecution()
    {
        HandleShooting();
        ApplyMotion();
        HandleAim();
        HandleReload();
        HandleWeaponCloserToWall();
    }

    public void HandleShooting()
    {
        if (inputManager.leftMouse && magazine > 0 && animatorManager.rig.weight == 1)
        {
            if (Time.time < currentWeapon.fireRate + nextShoot)
                return;

            nextShoot = Time.time;
            FireServerRpc();
            var bullet = Instantiate(currentWeapon.bullet, muzzle.position, muzzle.rotation);
            bullet.GetComponent<Bullet>().SetPlayerId(playerManager.clientId);
            muzzleFire.Play();
            ShootAnimation();
            ApplyRecoil();
            magazine--;
        }
    }

    void ApplyRecoil()
    {
        Vector3 impulse = currentWeapon.kickback * Vector3.back + Random.insideUnitSphere * currentWeapon.randomKick;
        float rotationMultiplier = 0.1f + this.positionSpring.position.magnitude / currentWeapon.MAX_POSITION_OFFSET;

        Vector3 kickEuler = new Vector3(impulse.z, impulse.x * currentWeapon.horizontalRecoilMultiplier, -impulse.x);

        float cameraRandomKick = currentWeapon.cameraRandomKick;
        float cameraKickback = currentWeapon.cameraKickback;
        float cameraSnap = currentWeapon.cameraSnap;

        if (inputManager.rightMouse)
        {
            impulse = impulse / 1.25f;
            kickEuler = kickEuler / 1.25f;
            rotationMultiplier = rotationMultiplier / 1.25f;

            cameraRandomKick = cameraRandomKick / 1.25f;
            cameraKickback = cameraKickback / 1.25f;
            cameraSnap = cameraSnap / 1.25f;
        }

        this.positionSpring.AddVelocity(impulse);
        this.rotationSpring.AddVelocity(kickEuler * rotationMultiplier * currentWeapon.ROTATION_IMPULSE_GAIN);

        cameraTargetRotation.x += -cameraRandomKick * currentWeapon.cameraHorizontalRecoilMultiplier;
        cameraTargetRotation.y += Random.Range(-cameraSnap, cameraSnap) * currentWeapon.cameraHorizontalRecoilMultiplier;
        cameraTargetRotation.z -= cameraKickback * currentWeapon.cameraHorizontalRecoilMultiplier;
    }


    Quaternion cameraTargetRotation = Quaternion.identity;

    public void ApplyMotion()
    {
        Vector3 localDeltaMovement = recoilPivot.worldToLocalMatrix.MultiplyVector(this.movementProbe.position - this.lastPosition);

        this.positionSpring.Update();
        this.rotationSpring.Update();

        Vector2 deltaRotation = new Vector2(Mathf.DeltaAngle(lastRotation.x, this.movementProbe.eulerAngles.x), Mathf.DeltaAngle(lastRotation.y, this.movementProbe.eulerAngles.y));

        this.lastPosition = this.movementProbe.position;
        this.lastRotation = this.movementProbe.eulerAngles;

        recoilPivot.localPosition = (this.localOrigin + this.positionSpring.position + Vector3.down * currentWeapon.weaponSnap * 0.1f) * .01f;
        recoilPivot.localEulerAngles = this.rotationSpring.position + Vector3.left;

        this.rotationSpring.position += new Vector3(-0.1f * deltaRotation.x + localDeltaMovement.y * 5f, -0.15f * deltaRotation.y, 0f);
        this.positionSpring.position += new Vector3(-0.0001f * deltaRotation.y, 0.0001f * deltaRotation.x, 0f);

        cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, cameraTargetRotation, currentWeapon.lerpSpeed * inputManager.deltaTime);

        cameraTargetRotation = Quaternion.identity;
    }

    public void HandleAim()
    {
        if (inputManager.rightMouse || keepAiming)
        {
            aimPivot.localPosition = Vector3.Lerp(aimPivot.localPosition, currentWeapon.gunAimPosition, currentWeapon.aimSpeed * inputManager.deltaTime);
            aimPivot.localRotation = Quaternion.Lerp(aimPivot.localRotation, currentWeapon.gunAimRotation, currentWeapon.aimSpeed * inputManager.deltaTime);
            cameraAimPivot.localPosition = Vector3.Lerp(cameraAimPivot.localPosition, currentWeapon.cameraAimPosition, currentWeapon.aimSpeed * inputManager.deltaTime);
        }
        else
        {
            aimPivot.localPosition = Vector3.Lerp(aimPivot.localPosition, Vector3.zero, currentWeapon.aimSpeed * inputManager.deltaTime);
            aimPivot.localRotation = Quaternion.Lerp(aimPivot.localRotation, Quaternion.identity, currentWeapon.aimSpeed * inputManager.deltaTime);
            cameraAimPivot.localPosition = Vector3.Lerp(cameraAimPivot.localPosition, Vector3.zero, currentWeapon.aimSpeed * inputManager.deltaTime);
        }
    }

    public void HandleReload()
    {
        if (inputManager.r)
        {
            ReloadServerRpc();
        }
        float weight = animatorManager.rig.weight;

        if (isReloading)
        {
            if (weight > 0)
                weight -= .1f;
            else
                weight = 0;
            animatorManager.SetRigWeight(weight);
        } 
        else
        {
            if (weight < 1)
                weight += .1f;
            else
                weight = 1;
            animatorManager.SetRigWeight(weight);
        }
    }

    public void HandleWeaponCloserToWall()
    {
        if (isReloading) return;
        Quaternion newRotation = Quaternion.identity;
        if (Physics.CheckSphere(closestWallDetector.position, .05f, wallMask))
        {
            newRotation = currentWeapon.rotationAwayFromCollision;
        }

        pointAwayFromWallsPivot.localRotation = Quaternion.Lerp(pointAwayFromWallsPivot.localRotation, newRotation, 10f * inputManager.deltaTime);
    }


    void Reload()
    {
        isReloading = true;

        animatorManager.PlayTargetAnimation("Reload");
        weaponAnimator.CrossFade("Reload", .1f);

        StartCoroutine(finishReload());
    }

    void ShootAnimation()
    {
        weaponAnimator.CrossFade("Shoot", .1f);
    }

    [ServerRpc]
    void ReloadServerRpc()
    {
        ReloadClientRpc();
    }

    [ClientRpc]
    void ReloadClientRpc()
    {
        Reload();
    }

    [ServerRpc]
    void FireServerRpc()
    {
        FireClientRpc();
    }

    [ClientRpc]
    void FireClientRpc()
    {
        if(!IsOwner)
        {
            var bullet = Instantiate(currentWeapon.bullet, muzzle.position, muzzle.rotation);
            bullet.GetComponent<Bullet>().SetPlayerId(playerManager.clientId);
            ShootAnimation();
            muzzleFire.Play();
            ApplyRecoil();
        }
    }

    [ServerRpc]
    public void DestroyServerRpc()
    {
        GameObject toDestroy = spanwedBullets[0];
        toDestroy.GetComponent<NetworkObject>().Despawn();
        spanwedBullets.Remove(toDestroy);
        Destroy(toDestroy);
    }

    IEnumerator finishReload()
    {
        yield return new WaitForSeconds(5.3f);
        magazine = currentWeapon.maxMagazineAmmo;
        isReloading = false;
    }
}
