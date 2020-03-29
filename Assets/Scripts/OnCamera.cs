using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCamera : MonoBehaviour
{
    Transform mainCamera;
    Transform target;
    [SerializeField] float maxDistance = 15, minDistance = 55; //相机距离，视野范围限制
    private float defaultDistance; //相机默认距离
    private Quaternion defaultRotation;//相机默认旋转
    private float Distance;//实时相机距离
    private bool auto = false;
    //Coroutine animation;//复位动画协程
    //float deltaT; //帧率测试
    void Awake()//赋值
    {
        mainCamera = Camera.main.transform;
        target = this.transform;
        //print("初始化成功！");
        defaultRotation = target.rotation;
        defaultDistance = -mainCamera.localPosition.z;
    }
    public void SetCurrentRotation()
    {
        if(!auto)
        StopAllCoroutines();
    }
    public void rotateView(Vector2 offset)//旋转视野
    {
        Vector3 rotate = new Vector3(-offset.y, offset.x, 0);//插值平滑
        target.Rotate(Vector3.Lerp(Vector3.zero, rotate,4*Time.deltaTime), Space.Self);
    }
    public void ScaleView(float scale)//缩放视野
    {
        float tempDis = -scale - mainCamera.localPosition.z;
        //print("相机距离变量: " + tempDis);
        if (tempDis <= maxDistance && tempDis >= minDistance)
            mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, new Vector3(0, 0, -tempDis), .3f);//平滑效果
        else if (tempDis > maxDistance)
            mainCamera.localPosition = new Vector3(0, 0, -maxDistance);
        else if (tempDis < minDistance)
            mainCamera.localPosition = new Vector3(0, 0, -minDistance);
    }
    public bool CameraDisMin()//相机距离最小值
    {
        return -mainCamera.localPosition.z <= minDistance;
    }
    public bool CameraDisMax()//相机距离最大值
    {
        return -mainCamera.localPosition.z >= maxDistance;
    }
    public void SetDefault()//恢复默认视角
    {
        StartCoroutine(SetDefaultAnimation(.2f));
    }
    private IEnumerator SetDefaultAnimation(float duration)//恢复默认视角时的动效
    {
        float t = Time.time;
        Quaternion q = target.rotation;
        Vector3 v = mainCamera.localPosition;
        auto = true;
        while ((Time.time - t) <= duration)
        {
            target.rotation = Quaternion.Slerp(target.rotation, defaultRotation, (Time.time - t) /duration);
            mainCamera.localPosition = new Vector3(v.x, v.y, Mathf.Lerp(mainCamera.localPosition.z, -defaultDistance,(Time.time - t)/duration));
            yield return null;
        }
        //print("相机复位的协程结束了！");
        auto = false;
    }
}
