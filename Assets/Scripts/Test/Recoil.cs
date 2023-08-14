using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    public float weaponSnap = .5f;
    public float kickback = .5f; 
    public float randomKick = .5f;

    public float fireRate = .1f;
    float nextShoot;

    Transform movementProbe;
    Vector3 lastPosition = Vector3.zero;
    Vector3 lastRotation = Vector3.zero;

    Spring positionSpring;
    Spring rotationSpring;

    Vector3 localOrigin = Vector3.zero;

    private void Awake()
    {
        this.movementProbe = this.transform;

        this.positionSpring = new Spring(POSITION_SPRING, POSITION_DRAG, -Vector3.one*MAX_POSITION_OFFSET, Vector3.one*MAX_POSITION_OFFSET, POSITION_SPRING_ITERAIONS);
        this.rotationSpring = new Spring(ROTATION_SPRING, ROTATION_DRAG, -Vector3.one * MAX_ROTATION_OFFSET, Vector3.one * MAX_ROTATION_OFFSET, ROTATION_SPRING_ITERAIONS);

        this.lastPosition = this.movementProbe.position;
        this.lastRotation = this.movementProbe.eulerAngles;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetButton("Fire1"))
        {
            if (Time.time > fireRate + nextShoot)
            {
                nextShoot = Time.time;

                Vector3 impulse = kickback * Vector3.back + Random.insideUnitSphere * randomKick;
                float rotationMultiplier = 0.1f + this.positionSpring.position.magnitude / MAX_POSITION_OFFSET;

                Vector3 kickEuler = new Vector3(impulse.z, impulse.x * 5, -impulse.x);

                this.positionSpring.AddVelocity(impulse);
                this.rotationSpring.AddVelocity(kickEuler * rotationMultiplier * ROTATION_IMPULSE_GAIN);
            }
        }

        ApplyMotion();

    }

    void ApplyMotion()
    {
		Vector3 localDeltaMovement = this.transform.worldToLocalMatrix.MultiplyVector(this.movementProbe.position - this.lastPosition);

        this.positionSpring.Update();
        this.rotationSpring.Update();

        Vector2 deltaRotation = new Vector2(Mathf.DeltaAngle(lastRotation.x, this.movementProbe.eulerAngles.x), Mathf.DeltaAngle(lastRotation.y, this.movementProbe.eulerAngles.y));

        this.lastPosition = this.movementProbe.position;
        this.lastRotation = this.movementProbe.eulerAngles;

        this.transform.localPosition = (this.localOrigin + this.positionSpring.position + Vector3.down * weaponSnap * 0.1f) * .01f;
        this.transform.localEulerAngles = this.rotationSpring.position + Vector3.left;

        this.rotationSpring.position += new Vector3(-0.1f*deltaRotation.x+localDeltaMovement.y*5f, -0.15f*deltaRotation.y, 0f);
        this.positionSpring.position += new Vector3(-0.0001f * deltaRotation.y, 0.0001f * deltaRotation.x, 0f);
    }

    public float POSITION_SPRING = 150f;
    public float POSITION_DRAG = 10f;
    public float MAX_POSITION_OFFSET = 0.2f;
    public int POSITION_SPRING_ITERAIONS = 8;

    public float ROTATION_SPRING = 70f;
    public float ROTATION_DRAG = 6f;
    public float MAX_ROTATION_OFFSET = 15f;
    public int ROTATION_SPRING_ITERAIONS = 8;
    public float ROTATION_IMPULSE_GAIN = 100f;


}
