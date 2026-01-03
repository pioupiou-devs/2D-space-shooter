using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic object pool for efficient object reuse
/// </summary>
public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Transform parent;
    private Queue<T> availableObjects;
    private List<T> allObjects;
    private int initialSize;
    private int maxSize;
    
    public int AvailableCount => availableObjects.Count;
    public int ActiveCount => allObjects.Count - availableObjects.Count;
    public int TotalCount => allObjects.Count;
    
    public ObjectPool(T prefab, int initialSize = 10, int maxSize = 100, Transform parent = null)
    {
        this.prefab = prefab;
        this.initialSize = initialSize;
        this.maxSize = maxSize;
        this.parent = parent;
        
        availableObjects = new Queue<T>();
        allObjects = new List<T>();
        
        // Pre-instantiate initial objects
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    
    private T CreateNewObject()
    {
        T newObj = Object.Instantiate(prefab, parent);
        newObj.gameObject.SetActive(false);
        allObjects.Add(newObj);
        availableObjects.Enqueue(newObj);
        return newObj;
    }
    
    public T Get()
    {
        T obj;
        
        // If no available objects and we haven't reached max size, create a new one
        if (availableObjects.Count == 0)
        {
            if (allObjects.Count < maxSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                // Pool is full, reuse the oldest active object
                Debug.LogWarning($"Object pool reached maximum size ({maxSize}). Reusing oldest object.");
                return null;
            }
        }
        else
        {
            obj = availableObjects.Dequeue();
        }
        
        obj.gameObject.SetActive(true);
        return obj;
    }
    
    public void Return(T obj)
    {
        if (obj == null || !allObjects.Contains(obj))
        {
            return;
        }
        
        obj.gameObject.SetActive(false);
        
        // Reset transform
        if (parent != null)
        {
            obj.transform.SetParent(parent);
        }
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        availableObjects.Enqueue(obj);
    }
    
    public void ReturnAll()
    {
        foreach (T obj in allObjects)
        {
            if (obj.gameObject.activeSelf)
            {
                Return(obj);
            }
        }
    }
    
    public void Clear()
    {
        foreach (T obj in allObjects)
        {
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }
        
        availableObjects.Clear();
        allObjects.Clear();
    }
}
