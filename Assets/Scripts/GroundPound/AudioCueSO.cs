using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioCue", menuName = "Game Data/Audio Cue")]
public class AudioCueSO : ScriptableObject
{
    [Tooltip("One of these is picked at random each time the cue plays.")]
    public AudioClip[] clips;

    [Tooltip("Random volume between X and Y each time it plays.")]
    public Vector2 volumeRange = new Vector2(0.8f, 1f);

    [Tooltip("Random pitch between X and Y each time it plays, so repeats don't sound identical.")]
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    public void Play(AudioSource source)
    {
        if (source == null || clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }
}
