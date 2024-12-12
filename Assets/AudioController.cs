using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public enum AudioCategory
{
    Music,
    Actions,
    NPCs,
    UI
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public AudioCategory category;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
    public bool loop = false;
}

public class AudioController : MonoBehaviour
{
    [Header("Audio Mixer Groups")]
    public AudioMixer masterMixer;
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup actionMixerGroup;
    public AudioMixerGroup npcMixerGroup;
    public AudioMixerGroup uiMixerGroup;

    [Header("Sound Configuration")]
    public List<Sound> sounds;

    // Dictionary for quick sound lookup by name
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

    // Dictionaries to track category-to-mixer relationships
    private Dictionary<AudioCategory, AudioMixerGroup> categoryMixerMap = new Dictionary<AudioCategory, AudioMixerGroup>();

    // Dictionaries to track tagged audio sources by category
    // These are references to scene objects tagged for each category of sound.
    // For example, a "Music" tag object might hold the AudioSource for the music.
    private Dictionary<AudioCategory, List<AudioSource>> categorySources = new Dictionary<AudioCategory, List<AudioSource>>();

    private void Awake()
    {
        // Populate category to mixer map
        categoryMixerMap[AudioCategory.Music] = musicMixerGroup;
        categoryMixerMap[AudioCategory.Actions] = actionMixerGroup;
        categoryMixerMap[AudioCategory.NPCs] = npcMixerGroup;
        categoryMixerMap[AudioCategory.UI] = uiMixerGroup;

        // Populate sound dictionary
        foreach (var s in sounds)
        {
            if (!soundDictionary.ContainsKey(s.name))
                soundDictionary.Add(s.name, s);
            else
                Debug.LogWarning("Duplicate sound name found: " + s.name);
        }

        // Initialize category sources
        foreach (AudioCategory cat in Enum.GetValues(typeof(AudioCategory)))
        {
            categorySources[cat] = new List<AudioSource>();
        }

        // You might have tags named "MusicSource", "ActionSource", "NPCSource", "UISource" etc.
        // Find and assign these at runtime:
        AssignSourcesToCategory(AudioCategory.Music, "MusicSource");
        AssignSourcesToCategory(AudioCategory.Actions, "ActionSource");
        AssignSourcesToCategory(AudioCategory.NPCs, "NPCSource");
        AssignSourcesToCategory(AudioCategory.UI, "UISource");
    }

    /// <summary>
    /// Finds all objects with the given tag and assigns their AudioSources to the specified category,
    /// updating their output AudioMixerGroup accordingly.
    /// </summary>
    /// <param name="category">The audio category.</param>
    /// <param name="tagName">The tag name to search for in the scene.</param>
    private void AssignSourcesToCategory(AudioCategory category, string tagName)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagName);
        foreach (var obj in taggedObjects)
        {
            AudioSource src = obj.GetComponent<AudioSource>();
            if (src == null)
            {
                src = obj.AddComponent<AudioSource>();
            }

            // Assign proper mixer group
            if (categoryMixerMap.ContainsKey(category))
            {
                src.outputAudioMixerGroup = categoryMixerMap[category];
            }

            // Assign default settings if needed
            src.playOnAwake = false;
            src.loop = false;

            categorySources[category].Add(src);
        }
    }

    /// <summary>
    /// Plays a sound by name. Will search the sound's category for available AudioSources.
    /// </summary>
    /// <param name="soundName">Name of the sound as defined in the sounds list.</param>
    public void PlaySound(string soundName)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning("Sound " + soundName + " not found in the AudioController.");
            return;
        }

        Sound s = soundDictionary[soundName];

        // Find a free AudioSource from the appropriate category
        var sources = categorySources[s.category];
        if (sources.Count == 0)
        {
            Debug.LogWarning("No AudioSources available for category: " + s.category);
            return;
        }

        // For simplicity, just use the first AudioSource assigned to that category
        // In a more advanced system, you could find a free one or pool them.
        AudioSource source = sources[0];

        source.clip = s.clip;
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.loop = s.loop;
        source.Play();
    }

    /// <summary>
    /// Stops a sound by name if it is currently playing.
    /// </summary>
    /// <param name="soundName">Name of the sound to stop.</param>
    public void StopSound(string soundName)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning("Sound " + soundName + " not found in the AudioController.");
            return;
        }

        Sound s = soundDictionary[soundName];
        var sources = categorySources[s.category];
        foreach (var source in sources)
        {
            if (source.isPlaying && source.clip == s.clip)
            {
                source.Stop();
            }
        }
    }

    /// <summary>
    /// Example function to set the volume of a mixer group exposed parameter.
    /// </summary>
    /// <param name="exposedParam">The exposed parameter name in the AudioMixer.</param>
    /// <param name="volume">Volume in decibels (e.g., -80 to 0 dB).</param>
    public void SetVolume(string exposedParam, float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat(exposedParam, volume);
        }
    }
}
