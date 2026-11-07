using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Scriptable Objects/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public struct SoundSlot
    {
        [Header("ID")]
        public string key;

        [Header("Data")]
        public AudioClip clip;
        public float volume;
        public bool loop;
    }

    //members
    public List<SoundSlot> musicList;
    public List<SoundSlot> sfxList;
}
