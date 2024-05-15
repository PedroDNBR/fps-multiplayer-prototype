using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeFirstLens : MonoBehaviour
{
    public Transform playerTarget;
    public Transform scope;

    public float scopeRange = 50f;

    public Renderer ocularRenderer;
    public Renderer objectiveRenderer;

    public Material baseMaterial;

    public GameObject test;

    public Camera ocularCamera;
    public Camera objectiveCamera;

    [SerializeField] RenderTexture ocularRenderTexture;
    [SerializeField] RenderTexture objectiveRenderTexture;
    [SerializeField] Material ocularMaterial;
    [SerializeField] Material objectiveMaterial;

    private void Start()
    {
        // playerTarget = transform.root.GetComponentInChildren<Camera>().transform;
        playerTarget = transform.root.GetComponentInChildren<AimingCamera>().transform;
        if (playerTarget == null) playerTarget = Camera.main.transform;

        if (ocularCamera == null || objectiveCamera == null) return;

        WeaponPrefab weaponPrefab = GetComponentInParent<WeaponPrefab>();
        if (weaponPrefab != null)
        {
            Transform muzzle = weaponPrefab.muzzle;
            if (muzzle != null)
            {
                
                GameObject model = Instantiate(test, muzzle);
                model.transform.parent = muzzle;
                model.transform.localPosition = new Vector3(0, 0, scopeRange);
                objectiveCamera.transform.LookAt(model.transform.position);

                Destroy(model);
            }
        }

        Color whiteColor = new Color(255, 255, 255);

        ocularRenderTexture = new RenderTexture(512, 512, 32);
        ocularRenderTexture.name = "OcularRenderTexture";
        ocularRenderTexture.enableRandomWrite = true;
        ocularRenderTexture.Create();

        ocularRenderTexture.Release();


        ocularCamera.targetTexture = ocularRenderTexture;

        ocularMaterial = new Material(Shader.Find("Shader Graphs/LensShaderGraph"));
        ocularMaterial.SetTexture("_MainTex", ocularRenderTexture);


        ocularRenderer.material = ocularMaterial;


        objectiveRenderTexture = new RenderTexture(512, 512, 32);
        objectiveRenderTexture.name = "ObjectiveRenderTexture";
        objectiveRenderTexture.enableRandomWrite = true;
        objectiveRenderTexture.Create();

        objectiveRenderTexture.Release();

        objectiveCamera.targetTexture = objectiveRenderTexture;

        objectiveMaterial = new Material(Shader.Find("Shader Graphs/LensShaderGraph"));
        objectiveMaterial.SetTexture("_MainTex", objectiveRenderTexture);

        //RenderTexture.active = objectiveRenderTexture;
        //GL.Clear(true, true, Color.black);
        //Graphics.Blit(mainTexture, objectiveRenderTexture, objectiveMaterial);

        objectiveRenderer.material = objectiveMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTarget == null) return;
        Vector3 localPlayer = scope.InverseTransformPoint(playerTarget.position);

        Vector3 lookAtScope = scope.TransformPoint(new Vector3(-localPlayer.x, -localPlayer.y, -localPlayer.z));

        ocularCamera.transform.rotation = Quaternion.LookRotation(lookAtScope - ocularCamera.transform.position, objectiveCamera.transform.up);
    }
}
