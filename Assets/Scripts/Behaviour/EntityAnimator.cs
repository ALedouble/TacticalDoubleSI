using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EntityAnimator : MonoBehaviour
{
    const int pixelUnit = 26;

    public Material mat;
    public EntityAnimation animation;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock matProp;

    GameObject child;


    public void Init()
    {
        matProp = new MaterialPropertyBlock();

        child = GameObject.CreatePrimitive(PrimitiveType.Quad);
        child.transform.parent = transform;
        child.transform.eulerAngles = Vector3.up * -45;

        Destroy(child.GetComponent<MeshCollider>());

        meshRenderer = child.GetComponent<MeshRenderer>();

        meshRenderer.sharedMaterial = mat;
    }

    float animationStartTime;
    LoopMode loopMode;
    public void PlayAnimation(EntityAnimation animation)
    {
        if (animation == null || (animation.frames.Count == 0))
        {

            return;
        }

        Texture tex = animation.GetTexture(Time.time);

        loopMode = animation.loopMode;

        this.animation = animation;
        animationStartTime = Time.time;
    }

    Texture lastTexture;
    public void Update()
    {
        if (animation == null) return;

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
        child.transform.localPosition = new Vector3(child.transform.localPosition.x, child.transform.localScale.y * .5f, child.transform.localPosition.z);
    }
}

public enum LoopMode
{
    Once,
    Loop
}