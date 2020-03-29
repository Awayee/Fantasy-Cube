using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Controller : MonoBehaviour
{
    //相机
    Camera mainCamera;//主相机
    Transform target,mainCameraTrans;//相机目标点
    [SerializeField] float maxDistance = 55, minDistance = 15; //相机距离，视野范围限制
    private float defaultDistance; //相机默认距离
    private Quaternion defaultRotation;//相机默认旋转
    private bool auto = false;

    //触摸
    Vector2 touchStart1, touchStart2;//触摸点
    OnCube cubes;
    RaycastHit hit;//射线检测到的物体
    private Vector3 startPoint, normal;//射线检测到的点，点的法向量
    private Transform hittedCube;//射线检测到的物体
    float currentTime;//当前时间
    float touchTime;//检测触摸时长
    private bool rotateInertia = false;//惯性转动
    private Vector3 rotateV;//转动速度
    enum TouchType {none, rotateCubes, controlView}//判断触摸控制类型
    TouchType touchType;
    
    //窗口
    [SerializeField] private GameObject sureExitPanel;//确认退出窗口
    public delegate void OnBackButton();//返回按钮事件
    public event OnBackButton onBackButton;
    void Start()//赋值
    {
        mainCamera = Camera.main;
        mainCameraTrans = mainCamera.transform;
        target = this.transform;
        //print("初始化成功！");
        defaultRotation = target.rotation;
        defaultDistance = -mainCameraTrans.localPosition.z;
        cubes = GameObject.FindObjectOfType<OnCube>();
        mainCamera = Camera.main;
        cubes.RenameCubes();
    }
    public void Exit()//退出游戏
    {
        //print("退出……");
        Application.Quit();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))//退出
        {
            if(onBackButton !=null)
            onBackButton();
            else sureExitPanel.SetActive(true);
        }
    }
    void LateUpdate()
    {
         //惯性滚动
         if(rotateInertia){
            //print("Delta Time: " + Time.deltaTime);
            Vector2 temp = rotateV - 8f * rotateV * Time.deltaTime;//加速度逐渐增大
            //print("Dot: "+Vector2.Dot(temp, rotateV));
            if (Vector2.Dot(temp, rotateV) <= 8f * Time.deltaTime)//停止
            {
                rotateInertia = false;
                rotateV = Vector3.zero;
            }
            RotateView(rotateV);
            rotateV = temp;
        }
        #region Windows平台
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        //视野控制
        if (Input.GetMouseButtonDown(1))//鼠标右键
        {
            //deltaT = Time.time;
            rotateInertia = false;
            if(auto)return;
            startPoint = Input.mousePosition;//记录鼠标按下时的坐标
            SetCurrentRotation();
        }
        if (Input.GetMouseButton(1))//鼠标右键
        {
            if(auto)return;
            Vector2 delta = Input.mousePosition - startPoint;

            RotateView(delta);
            startPoint = Input.mousePosition;
        }
        if(Input.GetMouseButtonUp(1)){
            rotateV = Input.mousePosition - startPoint;
            rotateInertia = true;
            //currentTime = Time.time;
        }
        if (Input.GetMouseButtonUp(2))//鼠标中键按下
        {
            Vector3 v = Input.mousePosition - startPoint;
            SetDefault();
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)//鼠标中键滚动
        {
            if(auto)return;
            StopAllCoroutines();
            ScaleView(50 * Input.GetAxis("Mouse ScrollWheel"));
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (cubes.Auto) return;//如果正在旋转则禁止输入
            if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())return;//点击了UI
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);//从鼠标点击位置发射一道射线
            if (Physics.Raycast(ray, out hit))
            {
                hittedCube = hit.transform;
                startPoint = hit.point;
                //print("射线1检测到物体: " + hittedCube.name+", 坐标: "+startPoint);
                normal = hit.normal;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (cubes.Auto) return;//如果正在旋转则禁止输入
            if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())return;//点击了UI
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);//从鼠标点击位置发射一道射线
            if (Physics.Raycast(ray, out hit))
            {
                //print("射线2检测到物体: " + hit.transform.name+", 坐标: "+hit.point);
                if (Vector3.Distance(hit.point, startPoint) > 1f)
                {
                    Vector3 crossed = Vector3.Cross(normal, hit.point - startPoint);//得到旋转轴向量
                    cubes.RotateCubeByTouch(hittedCube, crossed);
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))//按住Shift逆向旋转
        {
            if (Input.GetKeyDown(KeyCode.R)) cubes.RotateCubeByChar('R',true);
            else if (Input.GetKeyDown(KeyCode.L)) cubes.RotateCubeByChar('L',true);
            else if (Input.GetKeyDown(KeyCode.U)) cubes.RotateCubeByChar('U',true);
            else if (Input.GetKeyDown(KeyCode.D)) cubes.RotateCubeByChar('D',true);
            else if (Input.GetKeyDown(KeyCode.F)) cubes.RotateCubeByChar('F',true);
            else if (Input.GetKeyDown(KeyCode.B)) cubes.RotateCubeByChar('B',true);
            else if (Input.GetKeyDown(KeyCode.X)) cubes.RotateCubeByChar('X',true);
            else if (Input.GetKeyDown(KeyCode.Y)) cubes.RotateCubeByChar('Y',true);
            else if (Input.GetKeyDown(KeyCode.Z)) cubes.RotateCubeByChar('Z',true);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R)) cubes.RotateCubeByChar('r',true);
            else if (Input.GetKeyDown(KeyCode.L)) cubes.RotateCubeByChar('l',true);
            else if (Input.GetKeyDown(KeyCode.U)) cubes.RotateCubeByChar('u',true);
            else if (Input.GetKeyDown(KeyCode.D)) cubes.RotateCubeByChar('d',true);
            else if (Input.GetKeyDown(KeyCode.F)) cubes.RotateCubeByChar('f',true);
            else if (Input.GetKeyDown(KeyCode.B)) cubes.RotateCubeByChar('b',true);
            else if (Input.GetKeyDown(KeyCode.X)) cubes.RotateCubeByChar('x',true);
            else if (Input.GetKeyDown(KeyCode.Y)) cubes.RotateCubeByChar('y',true);
            else if (Input.GetKeyDown(KeyCode.Z)) cubes.RotateCubeByChar('z',true);
        }


#endif

        #endregion

        #region 安卓平台
#if UNITY_ANDROID

        if (Input.touchCount == 0) return;
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            rotateInertia = false;
            //触摸开始
            if (touch.phase == TouchPhase.Began)
            {
                //如果起点触摸到UI组件，返回
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    touchType = TouchType.none;
                    return;
                }
                touchTime = Time.time;//记录此时
                if (cubes.Auto) //如果正在自动旋转，则触摸任何区域旋转视角
                {
                    touchType = TouchType.controlView;//标记该触摸为控制视角
                    SetCurrentRotation();
                    touchStart1 = touch.position;
                }
                else //如果没有自动旋转，触摸到魔方转动魔方，否则旋转视角
                {
                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray.origin, ray.direction, out hit))
                    {
                        touchType = TouchType.rotateCubes;//标记该触摸为旋转方块
                        hittedCube = hit.transform;
                        startPoint = hit.point;
                        normal = hit.normal;
                    }
                    else
                    {
                        touchType = TouchType.controlView;//标记该触摸为控制视角
                        SetCurrentRotation();
                        touchStart1 = touch.position;
                    }
                }
            }
            //持续移动
            if (touch.phase == TouchPhase.Moved)
            {
                if (touchType == TouchType.rotateCubes)//旋转方块
                {
                    if (cubes.Auto) return;//如果正在旋转则禁止输入
                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    RaycastHit h;
                    if (Physics.Raycast(ray, out h)) hit = h;//持续移动，如果射线仍检测到物体，则不作操作
                    //如果没有检测到物体，则执行转动
                    else
                    {
                        if (hittedCube != null)
                        {
                            if (Vector3.Distance(hit.point, startPoint) > 1f)
                            {
                                Vector3 crossed = Vector3.Cross(normal, hit.point - startPoint);//得到旋转轴向量
                                cubes.RotateCubeByTouch(hittedCube, crossed);
                                hittedCube = null;//释放该方块
                            }
                        }
                    }
                }
                else if (touchType == TouchType.controlView)//控制视角
                {
                    if (auto) return;
                    RotateView(touch.position - touchStart1);
                    touchStart1 = touch.position;
                }
            }

            //触摸结束
            if (touch.phase == TouchPhase.Ended)
            {
                //检测双击，触摸时长小于一定值时触发
                if (Time.time - touchTime < .2f && touch.deltaPosition.magnitude < .2f)
                {
                    if (Time.time - currentTime < .2f) SetDefault();
                    currentTime = Time.time;
                }
                else
                {
                    if (touchType == TouchType.rotateCubes)
                    {
                        if (cubes.Auto) return;//如果正在旋转则禁止输入
                        Ray ray = mainCamera.ScreenPointToRay(touch.position);
                        if (Physics.Raycast(ray, out hit))
                        {
                            if (Vector3.Distance(hit.point, startPoint) > 1f)
                            {
                                Vector3 crossed = Vector3.Cross(normal, hit.point - startPoint);//得到旋转轴向量
                                cubes.RotateCubeByTouch(hittedCube, crossed);
                            }
                        }
                    }
                    else if(touchType == TouchType.controlView) {//启用惯性滑动
                        //currentTime = Time.time;
                        rotateV = touch.position - touchStart1;
                        //print("Velocity: " + velocity);
                        rotateInertia = true;
                    }
                }
            }
        }
        //多指控制视野
        else if (Input.touchCount > 1)
        {
            if (auto) return;               
            //之取前两个点
            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                    || UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(1).fingerId))
                {
                    touchType = TouchType.none;
                    return;
                }
                touchType = TouchType.controlView;
                touchStart1 = Input.GetTouch(0).position;
                touchStart2 = Input.GetTouch(1).position;
                rotateInertia = false;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                if(touchType == TouchType.controlView){
                    Vector2 currentPos1 = Input.GetTouch(0).position;
                    Vector2 currentPos2 = Input.GetTouch(1).position;
                    float delta = Vector2.Distance(currentPos2, currentPos1) - Vector2.Distance(touchStart1, touchStart2);
                    //print("Touch Scale delta: " + delta);
                    //满足缩放条件，缩放
                    if (Mathf.Abs(delta) >= 1f)
                        ScaleView(delta * .1f);
                    if((currentPos2 + currentPos1 - touchStart1 - touchStart2).sqrMagnitude >1f)
                        RotateView((currentPos2 + currentPos1 - touchStart1 - touchStart2)*.5f);
                    //print("Point Delta: " + (currentPos2 + currentPos1 - touchStart1 - touchStart2));
                    //else onCamera.rotateView(currentPos1 + currentPos2-touchVectors[0]-touchVectors[1]);
                    touchStart1 = currentPos1;
                    touchStart2 = currentPos2;
                }

            }
            // 两只手指同时离开
            if(Input.GetTouch(0).phase == TouchPhase.Ended && Input.GetTouch(1).phase == TouchPhase.Ended){
                if(touchType == TouchType.controlView){
                    Vector2 currentPos1 = Input.GetTouch(0).position;
                    Vector2 currentPos2 = Input.GetTouch(1).position;
                    rotateV = (currentPos1 + currentPos2 - touchStart1 - touchStart2)*.5f;
                    rotateInertia = true;
                    //currentTime = Time.time;                   
                }
                return;

            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (touchType == TouchType.controlView) {
                    if (Input.touchCount > 2)
                        touchStart1 = Input.GetTouch(2).position;
                    else if (Input.touchCount == 2)
                    {
                        touchStart1 = Input.GetTouch(1).position;
                    }
                }
            }
            if (Input.GetTouch(1).phase == TouchPhase.Ended)
            {
                if (touchType == TouchType.controlView){
                    if (Input.touchCount > 2)
                        touchStart2 = Input.GetTouch(2).position;
                    else if (Input.touchCount == 2)
                    {
                        touchStart1 = Input.GetTouch(0).position;
                    }
                }
            }

        }
#endif
        #endregion
    }

    public void SetCurrentRotation()
    {
        if(!auto)
        StopAllCoroutines();
    }
    public void RotateView(Vector2 offset)//旋转视野
    {
        Vector3 rotate = new Vector3(-offset.y, offset.x, 0);//插值平滑
        target.Rotate(Vector3.Lerp(Vector3.zero, rotate,12*Time.deltaTime), Space.Self);
    }
    public void ScaleView(float scale)//缩放视野
    {
        float tempDis = -scale - mainCameraTrans.localPosition.z;
        if (tempDis <= maxDistance && tempDis >= minDistance)
            mainCameraTrans.localPosition = Vector3.LerpUnclamped(mainCameraTrans.localPosition, new Vector3(0, 0, -tempDis), .3f);//平滑效果
        else if (tempDis > maxDistance)
            mainCameraTrans.localPosition = new Vector3(0, 0, -maxDistance);
        else if (tempDis < minDistance){
            mainCameraTrans.localPosition = new Vector3(0, 0, -minDistance);
        }
            
    }
    public bool CameraDisMin()//相机距离最小值
    {
        return -mainCameraTrans.localPosition.z <= minDistance;
    }
    public bool CameraDisMax()//相机距离最大值
    {
        return -mainCameraTrans.localPosition.z >= maxDistance;
    }
    public void SetDefault()//恢复默认视角
    {
        StartCoroutine(SetDefaultAnimation(.2f));
    }
    private IEnumerator SetDefaultAnimation(float duration)//恢复默认视角时的动效
    {
        float t = Time.time;
        Quaternion q = target.rotation;
        Vector3 v = mainCameraTrans.localPosition;
        auto = true;
        while ((Time.time - t) <= duration)
        {
            target.rotation = Quaternion.Slerp(target.rotation, defaultRotation, (Time.time - t) /duration);
            mainCameraTrans.localPosition = new Vector3(v.x, v.y, 
                Mathf.Lerp(mainCameraTrans.localPosition.z, -defaultDistance,(Time.time - t)/duration));
            yield return null;
        }
        //print("相机复位的协程结束了！");
        auto = false;
    }
}
