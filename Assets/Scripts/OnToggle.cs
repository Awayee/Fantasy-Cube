using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnToggle : MonoBehaviour {

	public void ToggleSet(bool toggleOn){
        transform.GetChild(0).gameObject.SetActive(!toggleOn);
    }
}
