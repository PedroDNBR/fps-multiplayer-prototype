using UnityEngine;
using UnityEngine.UI;
using static CodeMonkey.Utils.UI_TextComplex;

public class IconMaker : MonoBehaviour
{
    public Image icon;
    public Camera cam;
    public Transform objectHolder;

    public Sprite GetIcon(GameObject objectToCreateIcon, Vector3 offsetPosition, float orthographicSize, Item item = null)
    {
        cam.targetTexture = null;
        RenderTexture.active = null;
        foreach (Transform child in objectHolder)
        {
            Destroy(child.gameObject);
        }

        cam.orthographicSize = orthographicSize;

        var instantiatedObjectToCreateIcon = Instantiate(objectToCreateIcon, objectHolder);

        if(item != null)
        {
            instantiatedObjectToCreateIcon.GetComponent<WeaponPrefab>().isIcon = true;
            instantiatedObjectToCreateIcon.GetComponent<WeaponPrefab>().SetWeaponParts(item as WeaponItem);
        }

        instantiatedObjectToCreateIcon.transform.localPosition = offsetPosition;

        Texture2D texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        cam.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;

        cam.Render();
        texture.ReadPixels(new Rect(0,0, 512, 512), 0, 0);
        texture.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        foreach (Transform child in objectHolder)
        {
            Destroy(child.gameObject);
        }

        return Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0,0));
    }

    private void Start()
    {
        //icon.sprite = GetIcon();
    }

}
