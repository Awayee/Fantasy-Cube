using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnSettingPanel : MonoBehaviour {
    [SerializeField]Slider volumeSlider, maxstepsSlider, rotateSpeedSlider;
    OnMagicCube magicCube;
	void OnEnable(){
		//如果组件为空 ，获取组件
		if(magicCube == null)magicCube = GameObject.FindObjectOfType<OnMagicCube>();
		//if(volumeSlider ==null)volumeSlider = transform.GetChild(0).GetComponent<Slider>();
		//if(maxstepsSlider == null)maxstepsSlider = transform.GetChild(1).GetComponent<Slider>();
        volumeSlider.value = 10 * magicCube.GetComponent<AudioSource>().volume;
        maxstepsSlider.value = magicCube.maxRevokeSteps;
        rotateSpeedSlider.value = .5f / magicCube.rotateDuration;
    }
}
