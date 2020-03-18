using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Controller : MonoBehaviour
{
    Camera mainCamera;//主相机
    OnCamera onCamera;//主相机脚本
    private Vector3 startPoint, normal;//检测到的点，点的法向量
    Vector2 touchStart1, touchStart2;//多点触摸时只取前两个点
    OnMagicCube magicCube;
    RaycastHit hit;//射线检测到的物体
    private Transform hittedCube;//射线检测到的物体
    float currentTime;//当前时间
    float touchTime;//检测触摸时长
    enum TouchType {none, rotateCubes, controlView}//判断触摸控制类型
    TouchType touchType;
    //List<Vector2> points = new List<Vector2>();//记录触摸点
    Dictionary<int, Vector2> points = new Dictionary<int, Vector2>();//记录触摸点
    [SerializeField] private GameObject sureExitPanel;//确认退出窗口
    public delegate void OnBackButton();//返回按钮事件
    public event OnBackButton onBackButton;
    private bool exitting =false;//是否已弹出窗口
    // Use this for initialization
    void Start()
    {
        magicCube = GameObject.FindObjectOfType<OnMagicCube>();
        mainCamera = Camera.main;
        onCamera = mainCamera.transform.parent.GetComponent<OnCamera>();

        magicCube.RenameCubes();
        //onBackButton += SetPaneActive;
    }
    public void S() //打开或关闭某个窗口
    {
        sureExitPanel.SetActive(true);
    }
    public void Exit()//退出游戏
    {
        print("退出……");
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
    // Update is called once per frame
    void LateUpdate()
    {
        #region Windows平台
        /*
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0))
        {
            if (magicCube.Auto) return;//如果正在旋转则禁止输入
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
            if (magicCube.Auto) return;//如果正在旋转则禁止输入
            if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())return;//点击了UI
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);//从鼠标点击位置发射一道射线
            if (Physics.Raycast(ray, out hit))
            {
                //print("射线2检测到物体: " + hit.transform.name+", 坐标: "+hit.point);
                if (Vector3.Distance(hit.point, startPoint) > 1f)
                {
                    Vector3 crossed = Vector3.Cross(normal, hit.point - startPoint);//得到旋转轴向量
                    magicCube.RotateCubeByDrag(hittedCube, crossed);
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))//按住Shift逆向旋转
        {
            if (Input.GetKeyDown(KeyCode.R)) magicCube.RotateCubeByChar('R',true);
            else if (Input.GetKeyDown(KeyCode.L)) magicCube.RotateCubeByChar('L',true);
            else if (Input.GetKeyDown(KeyCode.U)) magicCube.RotateCubeByChar('U',true);
            else if (Input.GetKeyDown(KeyCode.D)) magicCube.RotateCubeByChar('D',true);
            else if (Input.GetKeyDown(KeyCode.F)) magicCube.RotateCubeByChar('F',true);
            else if (Input.GetKeyDown(KeyCode.B)) magicCube.RotateCubeByChar('B',true);
            else if (Input.GetKeyDown(KeyCode.X)) magicCube.RotateCubeByChar('X',true);
            else if (Input.GetKeyDown(KeyCode.Y)) magicCube.RotateCubeByChar('Y',true);
            else if (Input.GetKeyDown(KeyCode.Z)) magicCube.RotateCubeByChar('Z',true);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R)) magicCube.RotateCubeByChar('r',true);
            else if (Input.GetKeyDown(KeyCode.L)) magicCube.RotateCubeByChar('l',true);
            else if (Input.GetKeyDown(KeyCode.U)) magicCube.RotateCubeByChar('u',true);
            else if (Input.GetKeyDown(KeyCode.D)) magicCube.RotateCubeByChar('d',true);
            else if (Input.GetKeyDown(KeyCode.F)) magicCube.RotateCubeByChar('f',true);
            else if (Input.GetKeyDown(KeyCode.B)) magicCube.RotateCubeByChar('b',true);
            else if (Input.GetKeyDown(KeyCode.X)) magicCube.RotateCubeByChar('x',true);
            else if (Input.GetKeyDown(KeyCode.Y)) magicCube.RotateCubeByChar('y',true);
            else if (Input.GetKeyDown(KeyCode.Z)) magicCube.RotateCubeByChar('z',true);
        }


        //视野控制
        if (Input.GetMouseButtonDown(1))
        {
            //deltaT = Time.time;
            if(onCamera.Auto)return;
            startPoint = Input.mousePosition;//记录鼠标按下时的坐标
            onCamera.SetCurrentRotation();
        }
        //print("按下了鼠标左键。");
        if (Input.GetMouseButton(1))
        {
            if(onCamera.Auto)return;
            Vector2 delta = Input.mousePosition - startPoint;

            onCamera.rotateView(delta);
            startPoint = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
        {
            onCamera.SetDefault();
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if(onCamera.Auto)return;
            onCamera.StopAllCoroutines();
            float deltaDis = Input.GetAxis("Mouse ScrollWheel") * 40f;
            onCamera.ScaleView(deltaDis);
            //print("滚轮在滑动哦！" + deltaDis);
        }
#endif*/
        #endregion

        #region 安卓平台
#if UNITY_ANDROID

        if (Input.touchCount == 0) return;
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
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
                if (magicCube.Auto) //如果正在自动旋转，则触摸任何区域旋转视角
                {
                    touchType = TouchType.controlView;//标记该触摸为控制视角
                    onCamera.SetCurrentRotation();
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
                        onCamera.SetCurrentRotation();
                        touchStart1 = touch.position;
                    }
                }
            }
            //持续移动
            if (touch.phase == TouchPhase.Moved)
            {
                if (touchType == TouchType.rotateCubes)//旋转方块
                {
                    if (magicCube.Auto) return;//如果正在旋转则禁止输入
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
                                magicCube.RotateCubeByDrag(hittedCube, crossed);
                                hittedCube = null;//释放该方块
                            }
                        }
                    }
                }
                else if (touchType == TouchType.controlView)//控制视角
                {
                    if (onCamera.Auto) return;
                    Vector2 delta = touch.position - touchStart1;
                    onCamera.rotateView(delta * 2);
                    touchStart1 = touch.position;
                }
            }

            //触摸结束
            if (touch.phase == TouchPhase.Ended)
            {
                //检测双击，触摸时长小于一定值时触发
                if (Time.time - touchTime < .2f && touch.deltaPosition.magnitude < .2f)
                {
                    if (Time.time - currentTime < .2f) onCamera.SetDefault();
                    currentTime = Time.time;
                }
                else
                {
                    if (touchType == TouchType.rotateCubes)
                    {
                        if (magicCube.Auto) return;//如果正在旋转则禁止输入
                        Ray ray = mainCamera.ScreenPointToRay(touch.position);
                        if (Physics.Raycast(ray, out hit))
                        {
                            if (Vector3.Distance(hit.point, startPoint) > 1f)
                            {
                                Vector3 crossed = Vector3.Cross(normal, hit.point - startPoint);//得到旋转轴向量
                                magicCube.RotateCubeByDrag(hittedCube, crossed);
                            }
                        }
                    }
                }
            }
        }
        //多指控制视野
        else if (Input.touchCount > 1)
        {
            touchType = TouchType.controlView;
            if (onCamera.Auto) return;
            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
            {
                touchStart1 = Input.GetTouch(0).position;
                touchStart2 = Input.GetTouch(1).position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                Vector2 currentPos1 = Input.GetTouch(0).position;
                Vector2 currentPos2 = Input.GetTouch(1).position;
                float delta = Vector2.Distance(currentPos2, currentPos1) - Vector2.Distance(touchStart1, touchStart2);
                //print("Touch Scale delta: " + delta);
                //缩放的同时根据两触摸点的中点的位置变化旋转
                if (Mathf.Abs(delta) >= 1f)
                    onCamera.ScaleView(delta * .1f);
                onCamera.rotateView(currentPos2 + currentPos1 - touchStart1 - touchStart2);
                //print("Point Delta: " + (currentPos2 + currentPos1 - touchStart1 - touchStart2));

                //else onCamera.rotateView(currentPos1 + currentPos2-touchVectors[0]-touchVectors[1]);
                touchStart1 = currentPos1;
                touchStart2 = currentPos2;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if(Input.touchCount>2)
                    touchStart1 = Input.GetTouch(2).position;
            }
            if (Input.GetTouch(1).phase == TouchPhase.Ended)
            {
                if(Input.touchCount>2)
                    touchStart2 = Input.GetTouch(2).position;
            }

        }
        /*
        //多指控制视野
        else if (Input.touchCount > 0)
        {
            Touch[] touches = Input.touches;
            int count = Input.touchCount;
            for (int i = 0; i < count; i++)
            {
                //开始触控
                if (touches[i].phase == TouchPhase.Began)
                {
                    print("触控开始，触控点个数: " + points.Count);
                    print("Finger ID: " + touches[i].fingerId);
                    points.Add(touches[i].fingerId, touches[i].position);//记录点的索引和位置
                    //如果起点触摸到UI组件，返回
                    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touches[i].fingerId))
                    {
                        touchType = TouchType.none;
                        continue;
                    }
                    //记录此时
                    touchTime = Time.time;
                    //单点触摸
                    if (count == 1)
                    {
                        print("魔方自动旋转：" + magicCube.Auto);
                        if (magicCube.Auto) //如果正在自动旋转，则触摸任何区域旋转视角
                        {
                            touchType = TouchType.controlView;//标记该触摸为控制视角
                            onCamera.SetCurrentRotation();
                            //touchStart1 = touch.position;
                        }
                        else //如果没有自动旋转，触摸到魔方转动魔方，否则旋转视角
                        {
                            Ray ray = mainCamera.ScreenPointToRay(touches[i].position);
                            if (Physics.Raycast(ray.origin, ray.direction, out hit))
                            {
                                touchType = TouchType.rotateCubes;//标记该触摸为旋转方块
                                hittedCube = hit.transform;
                                startPoint = hit.point;
                                normal = hit.normal;
                                //print("摸到方块了，触控类型: "+touchType);
                            }
                            else
                            {
                                //print("没有摸到方块.");
                                touchType = TouchType.controlView;//标记该触摸为控制视角
                                onCamera.SetCurrentRotation();
                            }
                        }
                    }
                    else touchType = TouchType.controlView;
                }
                //触控点移动
                else if (touches[i].phase == TouchPhase.Moved)
                {
                    int fid = touches[i].fingerId;
                    //单点移动
                    if (count == 1)
                    {
                        print("Touch type: " + touchType);
                        if (touchType == TouchType.rotateCubes)//旋转方块
                        {
                            if (magicCube.Auto) return;//如果正在旋转则禁止输入
                            Ray ray = mainCamera.ScreenPointToRay(touches[i].position);
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
                                        magicCube.RotateCubeByDrag(hittedCube, crossed);
                                        hittedCube = null;//释放该方块
                                    }
                                }
                            }
                        }
                        else if (touchType == TouchType.controlView)//控制视角
                        {
                            if (onCamera.Auto) return;
                            Vector2 delta = touches[i].position - points[fid];
                            onCamera.rotateView(delta * 2);
                            //points[fid] = touches[i].position;
                        }
                    }
                    //多点移动
                    else
                    {
                        if (i == 0)
                        {
                            if (touches.Length > 1)
                            {
                                //只取前两个点
                                float delta = Vector2.Distance(touches[i].position, touches[i+1].position) - Vector2.Distance(points[fid], points[touches[i+1].fingerId]);
                                if (Mathf.Abs(delta) > 1) onCamera.ScaleView(delta * .1f);
                                onCamera.rotateView(touches[i].position + touches[i+1].position - points[fid] - points[touches[i+1].fingerId]);
                            }
                        }
                    }
                    points[fid] = touches[i].position;

                }
                //触摸结束
                else if (touches[i].phase == TouchPhase.Ended)
                {
                    if (count == 1)
                    {
                        //检测双击，触摸时长小于一定值时触发
                        if (Time.time - touchTime < .2f && touches[i].deltaPosition.magnitude < .2f)
                        {
                            if (Time.time - currentTime < .2f) onCamera.SetDefault();
                            currentTime = Time.time;
                        }
                        else
                        {
                            if (touchType == TouchType.rotateCubes)
                            {
                                if (magicCube.Auto) return;//如果正在旋转则禁止输入
                                Ray ray = mainCamera.ScreenPointToRay(touches[i].position);
                                if (Physics.Raycast(ray, out hit))
                                {
                                    if (Vector3.Distance(hit.point, startPoint) > 1f)
                                    {
                                        Vector3 crossed = Vector3.Cross(normal, hit.point - startPoint);//得到旋转轴向量
                                        magicCube.RotateCubeByDrag(hittedCube, crossed);
                                    }
                                }
                            }
                        }
                    }
                    int fid = touches[i].fingerId;
                    points.Remove(fid);
                    print("触控结束，控制点个数: " + points.Count);
                }
            }

        }*/
#endif
        #endregion

    }
}
