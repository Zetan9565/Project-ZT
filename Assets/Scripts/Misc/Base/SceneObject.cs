using System.Collections;
using UnityEngine;

public interface ISceneObject<T> : ISceneObject where T : MonoBehaviour { }
public interface ISceneObject
{
    string EntityID { get; }
}