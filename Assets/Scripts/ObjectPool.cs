using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject objectPrefab;
    public int initialPoolSize = 10;
    private List<GameObject> objectPool = new List<GameObject>();
    public List<GameObject> activeObjList = new();

    void Start()
    {
        LogManager.Instance.Log($"ObjectPool: Initializing pool for {objectPrefab.name}");
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(objectPrefab);
        obj.SetActive(false);
        objectPool.Add(obj);
        return obj;
    }

    public GameObject GetObject()
    {
        GameObject obj;
        if (objectPool.Count > 0)
        {
            int lastIndex = objectPool.Count - 1;
            obj = objectPool[lastIndex];
            objectPool.RemoveAt(lastIndex);
        }
        else
        {
            obj = CreateNewObject();
        }

        LogManager.Instance.Log($"ObjectPool: Getting object {obj.name} from pool");
        if (!activeObjList.Contains(obj))
        {
            activeObjList.Add(obj);
        }
        obj.SetActive(true);
        InitializeObject(obj);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        LogManager.Instance.Log($"ObjectPool: Returning object {obj.name} to pool");
        if (!activeObjList.Contains(obj))
        {
            Debug.LogWarning("Object not found in active list during return!");
            return;
        }
        activeObjList.Remove(obj);
        obj.SetActive(false);
        if (!objectPool.Contains(obj))
        {
            objectPool.Add(obj);
        }
    }

    public void ReturnAllObjects()
    {
        foreach (var obj in activeObjList.ToList())
        {
            ReturnObject(obj);
        }
    }

    private void InitializeObject(GameObject obj)
    {
        LogManager.Instance.Log($"ObjectPool: Initializing object {obj.name}");
        if (obj.TryGetComponent(out EvilBall evilBall))
        {
            evilBall.EvilBallPool = this;
        }

        if (obj.TryGetComponent(out NormalBall normalBall))
        {
            normalBall.NormalBallPool = this;
        }
    }
}