using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baloon : MonoBehaviour
{
    public float floatStrength = 0.8f;


    void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * floatStrength);
    }

    public void OnMouseDown()
    {
        if (PlayerPrefs.GetInt("SoundStatus") == 1)
        {
            this.GetComponentInParent<AudioSource>().Play();
        }
        this.GetComponentInChildren<SpriteRenderer>().enabled = false;
        this.GetComponentInChildren<ParticleSystem>().Play();
    }
}