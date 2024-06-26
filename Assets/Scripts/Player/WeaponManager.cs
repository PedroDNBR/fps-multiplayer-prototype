using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class WeaponManager : NetworkBehaviour
{
    [Header("Debug")]
    [SerializeField] bool keepAiming = false;

    InputManager inputManager;
    public WeaponItem currentWeapon;
    AnimatorManager animatorManager;
    PlayerManager playerManager;
    float nextShoot;
    public Transform playerCameraTransform;

    public float wallDetectorRadius = .5f;
    public LayerMask wallMask;

    public Transform recoilPivot;

    public Transform cameraPivot;

    public Transform aimPivot;
    public Transform cameraAimPivot;

    public Transform pointAwayFromWallsPivot;
    public Transform sightTransform;
    public float sightOffset;

    public Transform muzzle;
    public Transform wallDetector;
    public Transform closestWallDetector;
    public VisualEffect muzzleFire;

    public GameObject shootingSoundObject;
    public AudioClip audioShoot;

    public MultiParentConstraint gunIk;
    public MultiParentConstraint magazineIk;
    public MultiParentConstraint chargingHandleIk;
    public TwoBoneIKConstraint leftHandIk;
    public TwoBoneIKConstraint rightHandIk;

    Transform movementProbe;
    Vector3 lastPosition = Vector3.zero;
    Vector3 lastRotation = Vector3.zero;

    Spring positionSpring;
    Spring rotationSpring;

    Vector3 localOrigin = Vector3.zero;

    public NetworkVariable<bool> isAiming = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Animator weaponAnimator;

    public GameObject currentWeaponPrefab;

    [SerializeField] List<GameObject> spanwedBullets = new List<GameObject>();

    public bool isReloading;

    Coroutine reloadCorountine;

    public Dictionary<WeaponItem, int> weaponsAlredySetup = new Dictionary<WeaponItem, int>();

    public NetworkVariable<bool> mustCock = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        cameraPivot.gameObject.SetActive(IsOwner);
        cameraPivot.GetComponent<AudioListener>().enabled = IsOwner;
        mustCock.OnValueChanged += (oldValue, newValue) =>
        {
            teseeet = newValue;
        };
        base.OnNetworkSpawn(); // Not sure if this is needed though, but good to have it.
    }

    // Start is called before the first frame update
    public void Init(InputManager inputManager, AnimatorManager animatorManager, PlayerManager playerManager)
    {
        this.playerManager = playerManager;
        this.inputManager = inputManager;
        this.animatorManager = animatorManager;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetupWeaponServerRpc()
    {
        SetupWeaponClientRpc();
    }

    [ClientRpc]
    void SetupWeaponClientRpc()
    {
        if (currentWeapon == null)
        {
            var clearData = gunIk.data.sourceObjects;
            clearData.Clear();
            clearData.Add(new WeightedTransform(null, 1));
            gunIk.data.sourceObjects = clearData;
            gunIk.data.constrainedObject = null;

            var clearMagazineData = chargingHandleIk.data.sourceObjects;
            clearMagazineData.Clear();
            clearMagazineData.Add(new WeightedTransform(null, 1));
            magazineIk.data.sourceObjects = clearMagazineData;
            magazineIk.data.constrainedObject = null;

            var clearChargingHandleData = chargingHandleIk.data.sourceObjects;
            clearChargingHandleData.Clear();
            clearChargingHandleData.Add(new WeightedTransform(null, 1));
            chargingHandleIk.data.sourceObjects = clearChargingHandleData;
            chargingHandleIk.data.constrainedObject = null;


            leftHandIk.data.target = null;
            rightHandIk.data.target = null;

            muzzle = null;
            muzzleFire = null;
            wallDetector = null;
            closestWallDetector = null;
            pointAwayFromWallsPivot = null;
            recoilPivot = null;
            aimPivot = null;

            gunRestPosition = Vector3.zero;
            gunRestRotation = Quaternion.identity;
            cameraRestRotation = Vector3.zero;

            weaponAnimator = null;
            animatorManager.weaponAnimator = null;
            animatorManager.weaponMovementAnimator = null;


            animatorManager.GetComponent<RigBuilder>().Build();
            animatorManager.PlayTargetAnimation("NoWeaponAnimations", 3);

            sightTransform = null;
            sightOffset = 0;

            if (currentWeaponPrefab != null) Destroy(currentWeaponPrefab.gameObject);
        }

        if (currentWeapon == null) return;

        if (!weaponsAlredySetup.ContainsKey(currentWeapon))
        {
            weaponsAlredySetup.Add(currentWeapon, currentWeapon.maxMagazineAmmo);
        }
        if (reloadCorountine != null) StopCoroutine(reloadCorountine);
        isReloading = false;

        WeaponSpanwerParent spawner = GetComponentInChildren<WeaponSpanwerParent>();

        foreach (Transform child in spawner.transform)
        {
            Destroy(child.gameObject);
        }

        var prefab = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[currentWeapon.prefabId], spawner.transform);
        WeaponPrefab weaponPrefab = prefab.GetComponent<WeaponPrefab>();
        weaponPrefab.SetWeaponParts(currentWeapon);
        prefab.transform.parent = spawner.transform;


        weaponPrefab.playerCamera = playerCameraTransform;
        var data = gunIk.data.sourceObjects;
        data.Clear();
        data.Add(new WeightedTransform(weaponPrefab.gunIkTransform, 1));
        gunIk.data.sourceObjects = data;
        gunIk.data.constrainedObject = weaponPrefab.weaponTransform;

        var magazineData = magazineIk.data.sourceObjects;
        magazineData.Clear();
        magazineData.Add(new WeightedTransform(weaponPrefab.magazineIkTransform, 1));
        magazineIk.data.sourceObjects = magazineData;
        magazineIk.data.constrainedObject = weaponPrefab.magazineTransform;

        var chargingHandleData = chargingHandleIk.data.sourceObjects;
        chargingHandleData.Clear();
        chargingHandleData.Add(new WeightedTransform(weaponPrefab.chargingHandleIkTransform, 1));
        chargingHandleIk.data.sourceObjects = chargingHandleData;
        chargingHandleIk.data.constrainedObject = weaponPrefab.chargingHandleTransform;
        

        leftHandIk.data.target = weaponPrefab.leftHandIkTransform;
        rightHandIk.data.target = weaponPrefab.rightHandIkTransform;

        muzzle = weaponPrefab.muzzle;
        muzzleFire = weaponPrefab.muzzleFire;
        wallDetector = weaponPrefab.wallDetector;
        closestWallDetector = weaponPrefab.closestWallDetector;
        pointAwayFromWallsPivot = weaponPrefab.pointAwayFromWallsPivot;
        recoilPivot = weaponPrefab.recoilPivot;
        aimPivot = weaponPrefab.aimPivot;

        gunRestPosition = aimPivot.localPosition;
        gunRestRotation = aimPivot.localRotation;
        cameraRestRotation = cameraAimPivot.localPosition;

        weaponAnimator = weaponPrefab.weaponAnimator;
        if (animatorManager != null)
        {
            animatorManager.weaponAnimator = weaponPrefab.weaponAnimator;
            animatorManager.weaponMovementAnimator = weaponPrefab.weaponMovementAnimator;


            animatorManager.GetComponent<RigBuilder>().Build();
            animatorManager.PlayTargetAnimation("IdleHoldingGun", 3);

            animatorManager.isReloading = false;
        }

        if (weaponPrefab.sightTransforms.Count > 0)
        {
            sightTransform = weaponPrefab.sightTransforms[0];
            sightOffset = weaponPrefab.sightOffsets[0];
        } else
        {
            sightTransform = weaponPrefab.defaultSightTransform;
            sightOffset = weaponPrefab.defaultSightOffset;
        }

        currentWeaponPrefab = prefab;

        SetupSprings();
    }

    void SetupSprings()
    {
        movementProbe = recoilPivot;

        positionSpring = new Spring(currentWeapon.POSITION_SPRING,
            currentWeapon.POSITION_DRAG,
            -Vector3.one * currentWeapon.MAX_POSITION_OFFSET,
            Vector3.one * currentWeapon.MAX_POSITION_OFFSET,
            currentWeapon.POSITION_SPRING_ITERAIONS
        );

        rotationSpring = new Spring(
            currentWeapon.ROTATION_SPRING,
            currentWeapon.ROTATION_DRAG,
            -Vector3.one * currentWeapon.MAX_ROTATION_OFFSET,
            Vector3.one * currentWeapon.MAX_ROTATION_OFFSET,
            currentWeapon.ROTATION_SPRING_ITERAIONS
        );

        lastPosition = movementProbe.position;
        lastRotation = movementProbe.eulerAngles;
    }

    public void WeaponExecutionLocal()
    {
        if (currentWeapon == null) return;
        isAiming.Value = inputManager.rightMouse;
        HandleWeaponCloserToWallServerRpc();
        HandleShooting();
        HandleReload();

        if(Input.GetKeyDown(KeyCode.X))
        {
            animatorManager.GetComponent<RigBuilder>().Build();
        }
    }

    public void WeaponExecutionServer()
    {
        if (currentWeapon == null || currentWeaponPrefab == null) return;

        ApplyMotion();
        HandleAim();
        ReloadAnimationServerRpc();
    }

    bool shootButton = false;
    public void HandleShooting()
    {
        if (currentWeapon.fireMode == FireMode.Auto)
            shootButton = inputManager.holdLeftMouse;
        if (currentWeapon.fireMode == FireMode.SemiAuto)
            shootButton = inputManager.pressLeftMouse;

        if (shootButton && weaponsAlredySetup[currentWeapon] > 0 && !isReloading)
        {
            if (Time.time < currentWeapon.fireRate + nextShoot)
                return;


            if (weaponsAlredySetup[currentWeapon] <= 0) return;
            if (!IsServer)
            {
                weaponsAlredySetup[currentWeapon]--;
            }

            nextShoot = Time.time;
            FireServerRpc();
            muzzleFire.Play();
            Instantiate(shootingSoundObject, muzzle);
            var bullet = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[currentWeapon.bulletPrefabId], muzzle.position, muzzle.rotation);
            bullet.GetComponent<Bullet>().SetPlayerId(playerManager.clientId);
            ShootAnimation();
            ApplyRecoil();
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

        if (isAiming.Value)
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

        cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, cameraTargetRotation, currentWeapon.lerpSpeed * Time.deltaTime);

        cameraTargetRotation = Quaternion.identity;
    }

    Vector3 gunRestPosition;
    Quaternion gunRestRotation;
    Vector3 cameraRestRotation;

    public bool debugDisableAim = false;

    public void HandleAim()
    {
        Vector3 targetPosition = animatorManager.weaponMovementAnimator.transform.position;
        Vector3 cameraTargetRotation = cameraRestRotation;

        if (isAiming.Value || keepAiming)
        {
            targetPosition = cameraAimPivot.position + (aimPivot.position - sightTransform.position) + (cameraAimPivot.forward * sightOffset);
            cameraTargetRotation = currentWeapon.cameraAimPosition;
        }
        cameraAimPivot.localPosition = Vector3.Lerp(cameraAimPivot.localPosition, cameraTargetRotation, currentWeapon.aimSpeed * Time.deltaTime);
        aimPivot.position = Vector3.Lerp(aimPivot.position, targetPosition, .1f);
    }

    public void HandleReload()
    {
        if (inputManager.r)
        {
            ReloadServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ReloadAnimationServerRpc()
    {
        ReloadAnimationClientRpc();
    }

    [ClientRpc]
    void ReloadAnimationClientRpc()
    {

    }

    public void HandleWeaponCloserToWall()
    {
        HandleWeaponCloserToWallServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void HandleWeaponCloserToWallServerRpc()
    {
        HandleWeaponCloserToWallClientRpc();
    }


    [ClientRpc]
    void HandleWeaponCloserToWallClientRpc()
    {
        if (closestWallDetector == null) return;
        Quaternion newRotation = Quaternion.identity;
        if (Physics.CheckSphere(closestWallDetector.position, wallDetectorRadius, wallMask))
        {
            newRotation = currentWeapon.rotationAwayFromCollision;
        }

        pointAwayFromWallsPivot.localRotation = Quaternion.Lerp(pointAwayFromWallsPivot.localRotation, newRotation, 10f * Time.deltaTime);
    }

    void ShootAnimation()
    {
        animatorManager.weaponMovementAnimator.CrossFade("Shoot", .01f);
    }

    [ServerRpc(RequireOwnership = false)]
    void ReloadServerRpc()
    {
        animatorManager.weaponMovementAnimator.SetBool("MustCock", teseeet);
        animatorManager.weaponMovementAnimator.CrossFade("Reload", .5f);
        ReloadClientRpc();
    }

    [ClientRpc]
    void ReloadClientRpc()
    {
        isReloading = true;
        animatorManager.isReloading = false;

        animatorManager.weaponMovementAnimator.SetBool("MustCock", teseeet);
        animatorManager.weaponMovementAnimator.CrossFade("Reload", .5f);
    }
    bool teseeet = false;

    [ServerRpc]
    void FireServerRpc()
    {
        if (weaponsAlredySetup[currentWeapon] <= 0) return;
        weaponsAlredySetup[currentWeapon]--;
        mustCock.Value = false;
        if (weaponsAlredySetup[currentWeapon] <= 0)
            mustCock.Value = true;
        FireClientRpc();
    }

    [ClientRpc]
    void FireClientRpc()
    {
        if(!IsOwner)
        {
            Instantiate(shootingSoundObject, muzzle);
            muzzleFire.Play();
            var bullet = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[currentWeapon.bulletPrefabId], muzzle.position, muzzle.rotation);
            bullet.GetComponent<Bullet>().configuration.damage = 0;
            bullet.GetComponent<Bullet>().SetPlayerId(playerManager.clientId);
            ShootAnimation();
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

    [ServerRpc(RequireOwnership = false)]
    public void FinishReloadServerRpc()
    {
        FinishReloadClientRpc();
    }

    [ClientRpc]
    void FinishReloadClientRpc()
    {
        weaponsAlredySetup[currentWeapon] = currentWeapon.maxMagazineAmmo;
        isReloading = false;
        animatorManager.isReloading = false;
    }

    public void TestSetupGun()
    {
        if (!weaponsAlredySetup.ContainsKey(currentWeapon))
        {
            weaponsAlredySetup.Add(currentWeapon, currentWeapon.maxMagazineAmmo);
        }
        if (reloadCorountine != null) StopCoroutine(reloadCorountine);
        isReloading = false;
        animatorManager.isReloading = false;

        WeaponSpanwerParent spawner = GetComponentInChildren<WeaponSpanwerParent>();

        foreach (Transform child in spawner.transform)
        {
            Destroy(child.gameObject);
        }

        var prefab = Instantiate(DatabaseSingleton.instance.prefabList.prefabs[currentWeapon.prefabId], spawner.transform);
        prefab.transform.parent = spawner.transform;

        WeaponPrefab weaponPrefab = prefab.GetComponent<WeaponPrefab>();

        weaponPrefab.playerCamera = playerCameraTransform;
        var data = gunIk.data.sourceObjects;
        data.Clear();
        data.Add(new WeightedTransform(weaponPrefab.gunIkTransform, 1));
        gunIk.data.sourceObjects = data;
        gunIk.data.constrainedObject = weaponPrefab.weaponTransform;

        var magazineData = magazineIk.data.sourceObjects;
        magazineData.Clear();
        magazineData.Add(new WeightedTransform(weaponPrefab.magazineIkTransform, 1));
        magazineIk.data.sourceObjects = magazineData;
        magazineIk.data.constrainedObject = weaponPrefab.magazineTransform;

        var chargingHandleData = chargingHandleIk.data.sourceObjects;
        chargingHandleData.Clear();
        chargingHandleData.Add(new WeightedTransform(weaponPrefab.chargingHandleIkTransform, 1));
        chargingHandleIk.data.sourceObjects = chargingHandleData;
        chargingHandleIk.data.constrainedObject = weaponPrefab.chargingHandleTransform;


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


        animatorManager.GetComponent<RigBuilder>().Build();
        animatorManager.PlayTargetAnimation("IdleHoldingGun", 3);

        currentWeaponPrefab = prefab;

        SetupSprings();
    }
}
