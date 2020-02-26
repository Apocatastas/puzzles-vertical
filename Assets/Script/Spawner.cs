using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public const int howMuchBaloons = 15;
    public const float secondsBetweenSpawn = 0.4f;
    public GameObject[] baloonPrefabs;
    public GameObject[] baloons;
    public GameControl game;
    public Camera cam;
    public float m_force = 3.5f;
    public float YOffset;
    private const int YDestination = 400;
    public float t;

    Vector3 CalcRandomPos()
    {
        Vector3 pos;
        float localWidth;
        cam = FindObjectOfType<Camera>();
        YOffset = ((cam.orthographicSize * 20) / 16) * 1.2f;
        localWidth = cam.orthographicSize * 1.8f;
        pos = new Vector3(Random.Range(-localWidth, localWidth), -YOffset, 3f);
        return pos;
    }
    GameObject PrefabChooser()
    {
        switch (Mathf.CeilToInt(Random.Range(0, 6)))
        {
            case 0:
                return baloonPrefabs[0];
            case 1:
                return baloonPrefabs[1];
            case 2:
                return baloonPrefabs[2];
            case 3:
                return baloonPrefabs[3];
            case 4:
                return baloonPrefabs[4];
            case 5:
                return baloonPrefabs[5];
        }
        return baloonPrefabs[0];
    }
    public void Win()
    {
        baloons = new GameObject[howMuchBaloons];
        StartCoroutine(Spawn());
    }
    IEnumerator Spawn()
    {
       for (int i = 0; i < howMuchBaloons; i++)
        {
            baloons[i] = Instantiate(PrefabChooser(), CalcRandomPos(), Quaternion.identity) as GameObject;

            yield return new WaitForSeconds(secondsBetweenSpawn);
        }
        yield return new WaitForSeconds(5.5f);

        for (int i = 0; i < howMuchBaloons; i++)
        {
            Destroy(baloons[i]);
        }

        Broadcaster.AdHolder.ShowInterstitial();
        Broadcaster.AdHolder.RequestInterstitial();
    }
}