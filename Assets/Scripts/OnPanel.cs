using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPanel : MonoBehaviour {

    // Use this for initialization
    Controller controller;
    void OnEnable(){
		if(controller == null)controller = GameObject.FindObjectOfType<Controller>();//获取对象
        controller.onBackButton += CloseThisPanel;
    }
	void OnDisable(){
        controller.onBackButton -= CloseThisPanel;
    }
	void CloseThisPanel()//关闭此窗口
	{
        this.gameObject.SetActive(false);
    }

}
