using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageAnimator : MonoBehaviour
{
    public float interval = 0.1f;
    public bool loop;
    public bool auto;
    public bool nativeSize;
    public Sprite[] sprites = new Sprite[0];

    private Image image;
    private int currentIndex = 0;
    private bool isPlaying;
    private float time;

    private Action<ImageAnimator> loopCallback;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (auto) Play();
    }

    private void Update()
    {
        if (isPlaying)
        {
            time += Time.deltaTime;
            if (time >= interval)
            {
                time -= interval;
                currentIndex++;
                if (currentIndex >= sprites.Length)
                {
                    if (loop)
                    {
                        loopCallback?.Invoke(this);
                        currentIndex %= sprites.Length;
                    }
                    else
                    {
                        isPlaying = false;
                        return;
                    }
                }
                ApplyIndex();
            }
        }
    }

    private void ApplyIndex()
    {
        if (currentIndex >= 0 && currentIndex < sprites.Length)
        {
            image.sprite = sprites[currentIndex];
            if (nativeSize) image.SetNativeSize();
        }
    }

    public void Play()
    {
        currentIndex = 0;
        ApplyIndex();
        time = 0;
        isPlaying = true;
    }

    public void PlayLoop(Action<ImageAnimator> callback = null)
    {
        loop = true;
        loopCallback = callback;
        Play();
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        isPlaying = true;
    }
}
