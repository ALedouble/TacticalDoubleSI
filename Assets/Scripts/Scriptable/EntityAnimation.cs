using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityAnimation", menuName = "ScriptableObjects/EntityAnimation")]
public class EntityAnimation : ScriptableObject
{
    public List<AnimationFrame> frames = new List<AnimationFrame>();
    public LoopMode loopMode;

    public float Length
    {
        get
        {
            float value = 0;

            for (int i = 0; i < frames.Count; i++)
            {
                value += frames[i].length;
            }

            return value;
        }
    }

    public Texture GetTexture(float time)
    {
        float value = 0;
        for (int i = 0; i < frames.Count; i++)
        {
            float nextValue = value + frames[i].length;

            if (time >= value && time <= nextValue)
            {
                return frames[i].texture;
            }

            value = nextValue;
        }

        if (time > Length) return frames[frames.Count - 1].texture;

        Debug.LogError("Something is fucky with the animation system");
        Debug.Break();

        return null;
    }
}

[System.Serializable]
public struct AnimationFrame
{
    public Texture texture;
    public float length;

    public AnimationFrame(Texture texture, float length)
    {
        this.texture = texture;
        this.length = length;
    }
}