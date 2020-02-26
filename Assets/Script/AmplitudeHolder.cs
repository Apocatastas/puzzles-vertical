using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmplitudeHolder : MonoBehaviour
{

    void Awake()
    {
        Amplitude amplitude = Amplitude.Instance;
        amplitude.logging = true;
        amplitude.init("46a020e5756eb9d84018d64d6248f812");
    }

}
