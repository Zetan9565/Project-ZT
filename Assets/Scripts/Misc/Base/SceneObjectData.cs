using UnityEngine;

public abstract class SceneObjectData<TEntity> where TEntity : MonoBehaviour
{
    public string ID;
    public TEntity entity;
    public string scene;
    public Vector3 position;

    public abstract string Name { get; }

    public static implicit operator bool(SceneObjectData<TEntity> self)
    {
        return self != null;
    }
}