using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clicker : MonoBehaviour
{

    public void OnClick()
    {
        Broadcaster.ParentalButton = this.GetComponentInChildren<Text>().text;
    }
    

}
