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
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(objectPrefab);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
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
        if(!activeObjList.Contains(obj))activeObjList.Add(obj);
        obj.SetActive(true);
        InitializeObject(obj);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        activeObjList.Remove(obj);
        obj.SetActive(false);
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