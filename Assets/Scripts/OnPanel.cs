using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPanel : MonoBehaviour {

    // Use this for initialization
    Controller controller;
    Animation thisAnimation;
    void OnEnable(){
		if(controller == null)controller = GameObject.FindObjectOfType<Controller>();//获取对象
        if(thisAnimation == null)thisAnimation = GetComponent<Animation>();
        controller.onBackButton += ClosePanel;
    }
    public void ClosePanel(){
        controller.onBackButton -= ClosePanel;
        thisAnimation.Play("PanelOut");
    }
	void SetDisable()//关闭此窗口
	{
        this.gameObject.SetActive(false);
    }

}
