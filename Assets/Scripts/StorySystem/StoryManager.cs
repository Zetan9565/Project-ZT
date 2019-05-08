using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    private static StoryManager instance;
    public static StoryManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<StoryManager>();
            return instance;
        }
    }

    private Queue<Plot > plots = new Queue<Plot>();

    public void BeginNewStory(Story story)
    {
        plots.Clear();
        foreach (Plot plot in story.Plots)
        {
            plots.Enqueue(plot);
        }
    }

    public void Play()
    {

    }
}
