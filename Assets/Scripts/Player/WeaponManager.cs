using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponManager : NetworkBehaviour
{
    InputManager inputManager;
    WeaponItem currentWeapon;
    AnimatorManager animatorManager;
    float nextShoot;
    int magazine = 0;

    public Transform recoilPivot;

    public Transform cameraPivot;

    public Transform aimPivot;
    public Transform cameraAimPivot;

    public Transform muzzle;

    Transform movementProbe;
    Vector3 lastPosition = Vector3.zero;
    Vector3 lastRotation = Vector3.zero;

    Spring positionSpring;
    Spring rotationSpring;

    Vector3 localOrigin = Vector3.zero;

    public bool keepAiming = false;

    public Animator weaponAnimator;

    [SerializeField] List<GameObject> spanwedBullets = new List<GameObject>();

    bool isReloading;

    public override void OnNetworkSpawn()
    {
        cameraPivot.gameObject.SetActive(IsOwner);
        cameraPivot.GetComponent<AudioListener>().enabled = IsOwner;
        base.OnNetworkSpawn(); // Not sure if this is needed though, but good to have it.
    }

    // Start is called before the first frame update
    public void Init(InputManager inputManager, WeaponItem weaponItem, AnimatorManager animatorManager)
    {
        this.inputManager = inputManager;
        this.animatorManager = animatorManager;
        currentWeapon = weaponItem;

        magazine = currentWeapon.maxMagazineAmmo;

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

    public void HandleShooting()
    {
        if (inputManager.leftMouse && magazine > 0 && !isReloading)
        {
            if (Time.time < currentWeapon.fireRate + nextShoot)
                return;

            nextShoot = Time.time;
            FireServerRpc();
            Instantiate(currentWeapon.bullet, muzzle.position, muzzle.rotation);
            ApplyRecoil();
            magazine--;
        }
    }

    void ApplyRecoil()
    {
        Vector3 impulse = currentWeapon.kickback * Vector3.back + Random.insideUnitSphere * currentWeapon.randomKick;
        float rotationMultiplier = 0.1f + this.positionSpring.position.magnitude / currentWeapon.MAX_POSITION_OFFSET;

        Vector3 kickEuler = new Vector3(impulse.z, impulse.x * currentWeapon.horizontalRecoilMultiplier, -impulse.x);

        if (inputManager.rightMouse)
        {
            impulse = impulse / 1.25f;
            kickEuler = kickEuler / 1.25f;
            rotationMultiplier = rotationMultiplier / 1.25f;
        }

        this.positionSpring.AddVelocity(impulse);
        this.rotationSpring.AddVelocity(kickEuler * rotationMultiplier * currentWeapon.ROTATION_IMPULSE_GAIN);
        cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, new Quaternion(.1f, 0, 0, 0), .1f);

    }

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

        cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, Quaternion.identity, .1f);

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

            //cameraAimPivot.localPosition = Vector3.Lerp(cameraAimPivot.localPosition, currentWeapon.cameraAimPosition, currentWeapon.aimSpeed * inputManager.deltaTime);
        }
    }

    public void HandleReload()
    {
        if (inputManager.r)
        {
            ReloadServerRpc();
        }
    }


    void Reload()
    {
        isReloading = true;
        animatorManager.SetRigWeight(0);

        animatorManager.PlayTargetAnimation("Reload");
        weaponAnimator.CrossFade("Reload", .1f);

        StartCoroutine(finishReload());
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
            ApplyRecoil();
            Instantiate(currentWeapon.bullet, muzzle.position, muzzle.rotation);
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
        yield return new WaitForSeconds(5.5f);
        magazine = currentWeapon.maxMagazineAmmo;
        animatorManager.SetRigWeight(1);
        isReloading = false;
    }
}
