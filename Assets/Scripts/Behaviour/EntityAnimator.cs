using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EntityAnimator : MonoBehaviour
{
    const int pixelUnit = 32;

    public Material mat;
    public EntityAnimation animation;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock matProp;

    GameObject child;


    // Start is called before the first frame update
    void Start()
    {
        matProp = new MaterialPropertyBlock();

        child = GameObject.CreatePrimitive(PrimitiveType.Quad);
        child.transform.parent = transform;
        child.transform.eulerAngles = Vector3.up * -45;

        meshRenderer = child.GetComponent<MeshRenderer>();

        meshRenderer.sharedMaterial = mat;

        PlayAnimation(animation, LoopMode.Loop);
        Update();
    }

    float animationStartTime;
    LoopMode loopMode;
    public void PlayAnimation(EntityAnimation animation, LoopMode mode)
    {
        Texture tex = animation.GetTexture(Time.time);

        loopMode = mode;

        this.animation = animation;
        animationStartTime = Time.time;
    }

    Texture lastTexture;
    private void Update()
    {
        float time = loopMode == LoopMode.Loop ? Mathf.Repeat(Time.time - animationStartTime, animation.Length) : Time.time - animationStartTime;

        Texture tex = animation.GetTexture(time);

        if (tex != lastTexture)
        {
            UpdateChildHeight(tex.width, tex.height);
            lastTexture = tex;
        }

        matProp.SetTexture("_MainTex", tex);
        matProp.SetTexture("_EmissionMap", tex);
        meshRenderer.SetPropertyBlock(matProp);
    }

    void UpdateChildHeight(float width, float height)
    {
        child.transform.localScale = new Vector3(width / pixelUnit, (height / pixelUnit) / Mathf.Cos(Camera.main.transform.eulerAngles.x * Mathf.Deg2Rad), 1);
        child.transform.localPosition = Vector3.up * (child.transform.localScale.y * .5f);
    }
}

public enum LoopMode
{
    Once,
    Loop
}