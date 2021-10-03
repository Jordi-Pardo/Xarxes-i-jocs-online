using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class SpawnManager : MonoBehaviour
{
    //Prefab to spawn
    public GameObject cubePrefab;

    //List for checking current spawned cubes and move them
    List<GameObject> spawnedCubes = new List<GameObject>();

    //List for queuedActions that will be triggered when thred finished, works as a callback
    List<Action> queuedActions = new List<Action>();

    //Cube move speed
    private float speed = 3f;

    //Limited max cubes
    int maxCubes = 5;

    //keep track to reset threads
    List<Thread> currentThreads = new List<Thread>();

    //time to sleep in miliseconds
    int timeToSleep = 5000;

    void Update()
    {
        /*----  First Exercise-------*/

        FirstExercise();     //Uncomment/Comment

        /*----- Second Exercise------*/

        //SecondExercise();     //Uncomment/Comment

        /*----- Third Exercise------*/

        //ThirdExercise();      //Uncomment/Comment

        /*----- Fourth Exercise------*/

        //FourthExercise();     //Uncomment/Comment

    }

    private void FirstExercise()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject newCube = Instantiate(cubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            spawnedCubes.Add(newCube);
            Thread thread = new Thread(TimerToDestroy);
            thread.Start();

        }

        foreach (GameObject item in spawnedCubes)
        {
            item.transform.position += new Vector3(0, 0, 1 * Time.deltaTime * speed);
            if (queuedActions.Count > 0)
            {
                queuedActions[0].Invoke();
                queuedActions.RemoveAt(0);
                return;
            }
        }
    }
    private void SecondExercise()
    {
        if (Input.GetMouseButtonDown(0) && spawnedCubes.Count < 1)
        {
            GameObject newCube = Instantiate(cubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            spawnedCubes.Add(newCube);
            Thread thread = new Thread(TimerToDestroy);
            thread.Start();

        }

        foreach (GameObject item in spawnedCubes)
        {
            item.transform.position += new Vector3(0, 0, 1 * Time.deltaTime * speed);
            if (queuedActions.Count > 0)
            {
                queuedActions[0].Invoke();
                queuedActions.RemoveAt(0);
                return;
            }
        }
    }
    private void ThirdExercise()
    {
        if (Input.GetMouseButtonDown(0) && spawnedCubes.Count < 5)
        {
            GameObject newCube = Instantiate(cubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            spawnedCubes.Add(newCube);
            Thread thread = new Thread(TimerToDestroy);
            thread.Start();

        }

        foreach (GameObject item in spawnedCubes)
        {
            item.transform.position += new Vector3(0, 0, 1 * Time.deltaTime * speed);
            if (queuedActions.Count > 0)
            {
                queuedActions[0].Invoke();
                queuedActions.RemoveAt(0);
                return;
            }
        }
    }
    private void FourthExercise()
    {
        if (Input.GetMouseButtonDown(0) && spawnedCubes.Count < maxCubes)
        {
            GameObject newCube = Instantiate(cubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            spawnedCubes.Add(newCube);
            Thread thread = new Thread(TimerToDestroyAll);
            foreach (Thread currentThread in currentThreads)
            {
                Debug.Log("Aborted");
                currentThread.Abort();
            }
            currentThreads.Clear();
            currentThreads.Add(thread);
            thread.Start();

        }

        foreach (GameObject item in spawnedCubes)
        {
            item.transform.position += new Vector3(0, 0, 1 * Time.deltaTime * speed);

        }
        if (queuedActions.Count > 0)
        {
            Action action = queuedActions[0];
            action();
            queuedActions.Clear();
        }
    }

    void TimerToDestroy()
    {
        Debug.Log("Start");
        Thread.Sleep(timeToSleep);
        Action action = () => {
            GameObject gameObject = spawnedCubes[0];
            spawnedCubes.RemoveAt(0);
            Destroy(gameObject);
        };
        Debug.Log("end");
        queuedActions.Add(action);
    }

    void TimerToDestroyAll()
    {
        Debug.Log("Start");
        Thread.Sleep(timeToSleep);
        Action action = () =>
        {
            for (int i = 0; i < spawnedCubes.Count; i++)
            {
                Destroy(spawnedCubes[i]);
            }
            spawnedCubes.Clear();

        };
        Debug.Log("end");
        queuedActions.Add(action);
    }
}
