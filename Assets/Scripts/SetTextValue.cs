using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTextValue : MonoBehaviour {
	public void SetValue(float val){
        GetComponent<Text>().text = val.ToString();
    }
}
