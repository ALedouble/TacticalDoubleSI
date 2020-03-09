using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EntityAnimator : MonoBehaviour
{
    const int pixelUnit = 32;

    public Material mat;
    public Texture texture;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock matProp;

    GameObject child;

    // Start is called before the first frame update
    void Start()
    {
        matProp = new MaterialPropertyBlock();
        matProp.SetTexture("_MainTex", texture);

        child = GameObject.CreatePrimitive(PrimitiveType.Quad);
        child.transform.parent = transform;
        UpdateChildHeight();
        child.transform.eulerAngles = Vector3.up * -45;

        meshRenderer = child.GetComponent<MeshRenderer>();

        meshRenderer.sharedMaterial = mat;
        meshRenderer.SetPropertyBlock(matProp);
    }

    void UpdateChildHeight()
    {
        child.transform.localScale = new Vector3(texture.width / pixelUnit, (texture.height / pixelUnit) / Mathf.Cos(Camera.main.transform.eulerAngles.x * Mathf.Deg2Rad), 1);
        child.transform.localPosition = Vector3.up * (child.transform.localScale.y * .5f);
    }
}
