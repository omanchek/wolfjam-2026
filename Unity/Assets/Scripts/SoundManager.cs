using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum SoundTypes
    {
        MUSIC,
        SFX
    }

    //configuration
    [SerializeField] private SoundLibrary library;
    [SerializeField] private int initialSources = 8;

    //members
    List<AudioSource> sourcePool = new List<AudioSource>();

    private void Awake()
    {
        //setup sources
        for (int i = 0; i < initialSources; i++)
        {
            sourcePool.Add(gameObject.AddComponent<AudioSource>());
        }
    }

    private void Start()
    {
        PlaySoundByTypeAndID(SoundTypes.MUSIC, "danger_theme");
        PlaySoundByTypeAndID(SoundTypes.SFX, "loot_sound");
    }

    public bool PlaySoundByTypeAndID(SoundTypes type, string id)
    {
        int i, j;
        List<SoundLibrary.SoundSlot> list = new List<SoundLibrary.SoundSlot>();
        if (type == SoundTypes.MUSIC) list = library.musicList;
        else list = library.sfxList;

        //determine open slot or
        for (i = 0; i < list.Count; i++)
        {
            if (list[i].key == id)
            {
                for (j = 0; j < sourcePool.Count; j++)
                {
                    if (!sourcePool[j].isPlaying)
                    {
                        sourcePool[j].clip = list[i].clip;
                        sourcePool[j].volume = list[i].volume;
                        sourcePool[j].loop = list[i].loop;
                        sourcePool[j].Play();
                        return true;
                    }
                }

                sourcePool.Add(gameObject.AddComponent<AudioSource>());
                sourcePool[sourcePool.Count - 1].clip = list[i].clip;
                sourcePool[sourcePool.Count - 1].volume = list[i].volume;
                sourcePool[sourcePool.Count - 1].loop = list[i].loop;
                sourcePool[sourcePool.Count - 1].Play();
                return true;
            }
        }

        return false;
    }
}
