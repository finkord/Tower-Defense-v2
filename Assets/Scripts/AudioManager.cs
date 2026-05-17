using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Pool Settings")]
    public int initialPoolSize = 15;
    public int maxPoolSize = 30;
    private List<AudioSource> audioPool = new List<AudioSource>();
    
    [Header("Anti-Glitch Settings")]
    public float clipCooldown = 0.05f;
    private Dictionary<AudioClip, float> lastPlayTime = new Dictionary<AudioClip, float>();

    private void Awake()
    {
        Instance = this;
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            audioPool.Add(source);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // Prevent audio stacking/glitching (multiple identical sounds on exact same frame)
        if (lastPlayTime.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < clipCooldown)
            {
                return; // Drop the sound to save voices and prevent glitching
            }
        }

        lastPlayTime[clip] = Time.time;

        AudioSource source = GetAvailableSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume;
            source.Play();
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in audioPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        if (audioPool.Count < maxPoolSize)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            audioPool.Add(newSource);
            return newSource;
        }

        return null; // Drop the sound if we hit the hard limit
    }
}