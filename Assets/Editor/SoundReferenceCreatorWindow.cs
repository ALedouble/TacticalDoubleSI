using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class SoundReferenceCreatorWindow : EditorWindow
{
    [MenuItem("Tools/Sound Reference Creator")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SoundReferenceCreatorWindow window = (SoundReferenceCreatorWindow)GetWindow(typeof(SoundReferenceCreatorWindow));
        window.name = "Sound Reference Creator";
        window.Show();

        window.sound = new Sound();
    }

    public Sound sound;
    string assetName = "yourNameHere";
    string path = "yourPathHere";

    private void OnGUI()
    {
        CustomEditorUtility.DrawTitle("Sound Reference Creator");

        DisplayClips();

        assetName = EditorGUILayout.TextField(assetName);
        path = EditorGUILayout.TextField(path);

        EditorInspector.Show(sound);

        if (ValidSelection() != null && Length() > 0)
        {
            Color def = GUI.color;
            GUI.color = CustomEditorUtility.AddButtonColor();
            if (GUILayout.Button("Make Soundref", GUILayout.MinHeight(80)))
            {
                MakeScriptableObject();
            }

            GUI.color = def;
        }
    }

    private void DisplayClips()
    {
        if (Length() == 0) return;

        GUILayout.Label("Currently Selected : ", EditorStyles.boldLabel);

        AudioClip[] clips = ValidSelection();

        for (int i = 0; i < clips.Length; i++)
        {
            GUILayout.Label(clips[i].name, EditorStyles.boldLabel);
        }
    }

    private void MakeScriptableObject()
    {
        SoundReference soundRef = ScriptableObject.CreateInstance<SoundReference>();
        soundRef.sound = new Sound();
        soundRef.sound.clips = ValidSelection().ToList<AudioClip>();
        soundRef.sound.minPitch = sound.minPitch;
        soundRef.sound.maxPitch = sound.maxPitch;
        soundRef.sound.minVolume = sound.minVolume;
        soundRef.sound.maxPitch = sound.maxPitch;
        soundRef.name = assetName;

        AssetDatabase.CreateAsset(soundRef, soundRefPath + "/" + path + "/" + assetName + ".asset");

    }

    private AudioClip[] ValidSelection()
    {
        return Selection.GetFiltered<AudioClip>(SelectionMode.Unfiltered);
    }

    private int Length()
    {
        if (ValidSelection() == null) return 0;

        return ValidSelection().Length;
    }

    private string soundRefPath
    {
        get
        {
            return "Assets/ScriptableObjects/Audio/";
        }
    }
}
