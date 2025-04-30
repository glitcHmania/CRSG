using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    private Transform parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            T instance = GameObject.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }
    }

    public T Get()
    {
        if (pool.Count == 0)
        {
            T instance = GameObject.Instantiate(prefab, parent);
            return instance;
        }

        var obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            if(obj != null)
                GameObject.Destroy(obj.gameObject);
        }
    }
}
