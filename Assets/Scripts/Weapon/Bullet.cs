using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public Configuration configuration;

    public ulong playerId;

    public bool autoAssignArmorDamage = true;

    protected Vector3 velocity = Vector3.zero;
    protected float expireTime = 0f;

    protected float travelDistance = 0f;

    public bool debugBulletMark = false;
    public GameObject bulletMark;

    int hitMask;

    protected bool travelling = true;

    public LayerMask layermask;
    protected virtual void Start()
    {
        this.velocity = this.transform.forward * this.configuration.speed;
        this.expireTime = Time.time + this.configuration.lifetime;
    }

    public void SetPlayerId(ulong playerId)
    {
        this.playerId = playerId;
    }

    // Update is called once per frame
    protected virtual void Update()
    {

        if (Time.time > this.expireTime)
        {
            /* GetComponent<NetworkObject>().Despawn();
             parent.DestroyServerRpc();*/
            Destroy(gameObject);
            return;
        }

        UpdatePosition();
    }

    protected virtual void UpdatePosition()
    {
        this.travelDistance += this.configuration.speed * Time.deltaTime;

        this.velocity += Physics.gravity * this.configuration.gravityMultiplier * Time.deltaTime;
        Vector3 delta = this.velocity * Time.deltaTime;
        Travel(delta);
    }

    protected virtual void Travel(Vector3 delta)
    {

        Vector3 nextPosition = this.transform.position + delta;

        RaycastHit hitInfo;
        if (Physics.Linecast(this.transform.position, nextPosition, out hitInfo, layermask))
        {
            if(debugBulletMark)
                Instantiate(bulletMark, hitInfo.point, transform.rotation);

            if (configuration.damage < 1) return;

            // Hit(hitInfo.point, hitInfo.normal);
            BodyMember bodyMember = hitInfo.transform.gameObject.GetComponent<BodyMember>();

            if(bodyMember)
            {
                bodyMember.TakeDamage(configuration.damage, playerId);
            }

            Destroy(gameObject);
        }

        if (travelling)
        {
            this.transform.position += delta;
            this.transform.rotation = Quaternion.LookRotation(delta);
        }
    }

    protected virtual void Hit(Vector3 point, Vector3 normal)
    {
        /*GetComponent<NetworkObject>().Despawn();
        parent.DestroyServerRpc();*/
        Destroy(gameObject);
        // do logic
    }

    [System.Serializable]
    public class Configuration
    {
        public float speed = 300f;
        public float impactForce = 200f;
        public float lifetime = 2f;
        public int damage = 70;
        public float balanceDamage = 60f;
        public float impactDecalSize = 0.2f;
        public bool passThroughPenetrateLayer = true;
        public bool piercing = false;
        public bool makesFlybySound = false;
        public float flybyPitch = 1f;
        public float dropoffEnd = 300f;
        public float gravityMultiplier = 1f;
        public AnimationCurve damageDropOff;
        public bool inheritVelocity = false;
    }
}
