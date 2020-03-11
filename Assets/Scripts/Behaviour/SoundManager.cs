using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public Queue<GameObject> receptaclePool;

    public float sfxVolume = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        templateReceptacle = new GameObject();
        templateReceptacle.transform.parent = transform;
        templateReceptacle.AddComponent<AudioSource>();



        receptaclePool = new Queue<GameObject>();

        FillReceptaclePool();

    }



    GameObject templateReceptacle;
    GameObject newReceptacle;
    public void FillReceptaclePool()
    {
        for (int i = 0; i < 200; i++)
        {
            GameObject newReceptacle = Instantiate(templateReceptacle, transform);
            receptaclePool.Enqueue(newReceptacle);
        }
    }

    public void PlaySound(Sound _sound, bool loops = false, bool bypassAudioListener = false)
    {
        if(receptaclePool.Count < 1)
        {
            FillReceptaclePool();
        }
        GameObject receptacle = receptaclePool.Dequeue();
        AudioSource source = receptacle.GetComponent<AudioSource>();
        
        StartCoroutine(PlaySoundCoroutine(receptacle, source, _sound, loops, false, Vector3.zero, 0f, 500f, 0f, bypassAudioListener));
    }

    public AudioSource PlaySoundReturnSource(Sound _sound, bool loops = false, bool bypassAudioListener = false)
    {
        if (receptaclePool.Count < 1)
        {
            FillReceptaclePool();
        }
        GameObject receptacle = receptaclePool.Dequeue();
        AudioSource source = receptacle.GetComponent<AudioSource>();
        StartCoroutine(PlaySoundCoroutine(receptacle, source, _sound, loops, false, Vector3.zero, 0f, 500f, 0f, bypassAudioListener)) ;
        return source;
    }


    public AudioSource PlaySoundAtPosition(Sound _sound, Vector3 pos, float spatialBlend = 0.5f, bool loops = false, float maximumDistance = 30f, float minimumDistance = 5f, bool bypassAudioListener = false)
    {
        if (receptaclePool.Count < 1)
        {
            FillReceptaclePool();
        }
        GameObject receptacle = receptaclePool.Dequeue();
        AudioSource source = receptacle.GetComponent<AudioSource>();
        StartCoroutine(PlaySoundCoroutine(receptacle, source, _sound, loops, true, pos, spatialBlend, maximumDistance, minimumDistance, bypassAudioListener));
        return source;
    }

    public void RepoolSource(AudioSource toRepool)
    {
        toRepool.enabled = false;
        toRepool.Stop();
        receptaclePool.Enqueue(toRepool.gameObject);
    }


    public IEnumerator PlaySoundCoroutine(GameObject _receptacle, AudioSource source, Sound _sound, bool loops, bool hasPos, Vector3 pos, float spatialBlend, float maximumDistance, float minimumDistance = 5f, bool bypassAudioListener = false)
    {
        source.enabled = true;
        source.clip = _sound.clips[Random.Range(0, _sound.clips.Count)];
        source.volume = Random.Range(_sound.minVolume, _sound.maxVolume) * sfxVolume;
        source.pitch = Random.Range(_sound.minPitch, _sound.maxPitch);
        source.maxDistance = maximumDistance;
        source.minDistance = minimumDistance;
        source.loop = loops;
        source.bypassListenerEffects = bypassAudioListener;
        source.gameObject.transform.position = pos;
        source.spatialBlend = spatialBlend;

        source.Play();

        if(!loops)
        {
            yield return new WaitForSeconds(source.clip.length);
            RepoolSource(source);
        }

    }
}
