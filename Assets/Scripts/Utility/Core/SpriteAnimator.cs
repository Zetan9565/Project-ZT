using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public float interval = 0.1f;
    public bool loop;
    public bool auto;
    public Sprite[] sprites = new Sprite[0];

    private SpriteRenderer mRenderer;
    private int currentIndex = 0;
    private bool isPlaying;
    private float time;

    private Action<SpriteAnimator> loopCallback;

    private void Awake()
    {
        mRenderer = GetComponent<SpriteRenderer>();
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
                mRenderer.sprite = sprites[currentIndex];
            }
        }
    }

    public void Play()
    {
        currentIndex = 0;
        mRenderer.sprite = sprites[currentIndex];
        time = 0;
        isPlaying = true;
    }

    public void PlayLoop(Action<SpriteAnimator> callback = null)
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
