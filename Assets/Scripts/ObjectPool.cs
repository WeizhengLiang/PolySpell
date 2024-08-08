using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject objectPrefab;
    public int initialPoolSize = 10;
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    public List<GameObject> activeObjList = new();

    void Start()
    {
        if (objectPool.Count < initialPoolSize)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(objectPrefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
        }
    }

    public GameObject GetObject()
    {
        GameObject obj;
        if (objectPool.Count > 0)
        {
            obj = objectPool.Dequeue();
        }
        else
        {
            obj = Instantiate(objectPrefab);
        }

        if (!activeObjList.Contains(obj))
        {
            // Debug.LogWarning("Object added into active list during getting!");
            activeObjList.Add(obj);
        }
        else
        {
            // Debug.LogWarning("Object added into active list during Getting!");
        }
        obj.SetActive(true);
        InitializeObject(obj);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if (!activeObjList.Contains(obj))
        {
            Debug.LogWarning("Object not found in active list during return!");
            return;
        }
        activeObjList.Remove(obj);
        obj.SetActive(false);
        if (objectPool.Contains(obj))
        {
            Debug.LogWarning("You cannot enqueue game object that already in the q");
            return;
        }
        objectPool.Enqueue(obj);
    }
    
    private void InitializeObject(GameObject obj)
    {
        EvilBall evilBall = obj.GetComponent<EvilBall>();
        if (evilBall != null)
        {
            evilBall.EvilBallPool = this;
            evilBall.SetRandomHealth();  // Assign random health
        }

        NormalBall normalBall = obj.GetComponent<NormalBall>();
        if (normalBall != null)
        {
            normalBall.NormalBallPool = this;
        }
    }
}