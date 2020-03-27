using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

//实现触摸旋转魔方的一面
public class OnCube : MonoBehaviour
{
    private bool auto = false;
    public bool Auto{get { return auto; }}//魔方是否正在自行运作
    [SerializeField] public float rotateDuration = .233f;//方块旋转延时，越小越快
    [SerializeField] private int steps = 0;//实时操作步数记录
    [SerializeField] public int maxRevokeSteps = 10;//最多可记录步骤数
    private List<char> historySteps = new List<char>();//记录历史步骤的栈，由于步骤数限制用List实现
    private Stack<char> redoSteps = new Stack<char>();//记录重做步骤的栈
    private Transform axis;//转动轴
    [SerializeField]private Text uiText;//显示步数的UI组件
    [SerializeField]private AudioClip rotateAudio;//旋转音效
     [SerializeField]private AudioClip revokeAudio;//撤销音效
    [SerializeField] private AudioSource audioSource;//播放器
    [SerializeField] private Button revoke, redo, reset;//按钮

    #region 触摸旋转
    public void RotateCubeByDrag(Transform cube, Vector3 direction)
    {
        if (auto) return;
        //得到三个坐标轴偏移最大的值，并转换方向向量
        Vector3 dir;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (Mathf.Abs(direction.z) > Mathf.Abs(direction.x)) dir = new Vector3(0, 0, direction.z > 0 ? 1 : -1);
            else dir = new Vector3(direction.x > 0 ? 1 : -1, 0, 0); ;
        }
        else
        {
            if (Mathf.Abs(direction.z) > Mathf.Abs(direction.y)) dir = new Vector3(0, 0, direction.z > 0 ? 1 : -1);
            else dir = new Vector3(0, direction.y > 0 ? 1 : -1, 0);
        }

        //获得旋转轴
        if (axis == null)
        {
            if (transform.GetChild(0).localPosition == new Vector3(0, 0, 0))
                axis = transform.GetChild(0);
            else
            {
                axis = new GameObject("axis").transform;//
                axis.SetParent(this.transform);
                axis.localPosition = Vector3.zero;
                axis.SetAsFirstSibling();
            }
        }

        //确定旋转方向
        Quaternion q;//四元数旋转
        if (Vector3.Dot(dir, Vector3.one) < 0){
            dir *= -1;
            q = Quaternion.AngleAxis(-90, dir);
        }
        else q = Quaternion.AngleAxis(90, dir);
        //找到相关联的方块并设置为中心点的子物体

        //print("旋转四元数: " + q);
        Vector3 temp = Vector3.Scale(dir, cube.localPosition);
        //中间
        if (temp == Vector3.zero){ 
            for (int i = 1; i < transform.childCount; i++)
            {
                float t = Vector3.Dot(dir, transform.GetChild(i).localPosition);
                if (t<1&&t>-1)
                {
                    transform.GetChild(i).SetParent(axis);
                    i--;
                }
            }
            //记录中间方块的步骤
            if(q.x!=0){
                if(q.x>0)PushStep('X');
                else PushStep('x');
            }
            else if(q.y!=0){
                if(q.y>0)PushStep('Y');
                else PushStep('y');
            }
            else if(q.z!=0){
                if(q.z>0)PushStep('z');
                else PushStep('Z');
            }
        }
        //两侧
        else
        {
            for (int i = 1; i < transform.childCount; i++)
            {
                if (Vector3.Dot(temp, transform.GetChild(i).localPosition) > 1)
                {
                    transform.GetChild(i).SetParent(axis);
                    i--;
                }
            }
            //记录两侧方块的步骤
            if(q.x!=0){
                if(q.x>0){
                    if(temp.x>0)PushStep('R');
                    else PushStep('l');
                }
                else {
                    if(temp.x>0)PushStep('r');
                    else PushStep('L');
                }
            }
            else if(q.y!=0){
                if(q.y>0){
                    if(temp.y>0)PushStep('U');
                    else PushStep('d');
                }
                else{
                    if(temp.y>0)PushStep('u');
                    else PushStep('D');
                }
            }
            else if(q.z!=0){ //z轴负方向为正
                if(q.z<0){
                    if(temp.z<0)PushStep('F');
                    else PushStep('b');
                }
                else{
                    if(temp.z<0)PushStep('f');
                    else PushStep('B');
                }
            }
        }
        StartCoroutine(RotateCubesByDragAnimation(q));
        PlayAudio(rotateAudio);//播放声音
        //if(!undo.interactable)undo.interactable = true;//激活撤销键
        //if(!reset.interactable)reset.interactable = true;//激活重置键
        if(redoSteps.Count>0)redoSteps.Clear();//清空重做栈
        if (redoSteps.Count < 1)redo.interactable = false;//如果重做栈为空，禁用重做键

    }
    private IEnumerator RotateCubesByDragAnimation(Quaternion rotation) //触摸方块旋转
    {
        float t = Time.time;
        Quaternion start = axis.localRotation;//初始旋转
        Quaternion end = rotation * start; //旋转末位置
        auto = true;
        while (Time.time - t < rotateDuration)
        {
            axis.localRotation = Quaternion.Slerp(start, end, (Time.time - t) / rotateDuration);
            yield return null;
        }
        axis.localRotation = end;//确保旋转到位
        //旋转完成后立刻解除父子关系
        foreach (Transform c in axis.GetComponentsInChildren<Transform>())
        {
            c.SetParent(this.transform);
        }
        auto = false;
    }

    #endregion

    #region 输入指令旋转
    public void RotateCubeByChar(char ch)//指令旋转，用字母代表某个面
    {
        if (auto) return;
        StartCoroutine(CubesRotateAnimation(ch));
    }
    public void RotateCubeByChar(char ch, bool record)//指令旋转并记录
    {
        RotateCubeByChar(ch);
        PushStep(ch);
        PlayAudio(rotateAudio);//播放声音=旋转
        //if(!undo.interactable)undo.interactable = true;//激活撤销键
        //if(!reset.interactable)reset.interactable = true;//激活重置键
        // if (redoSteps.Peek() != ch)//判断是否为重做
        // {
        //     redoSteps.Clear();//清空重做栈
        //     redo.interactable = false;//禁用重做按钮
        // }
    }
    private IEnumerator CubesRotateAnimation(char ch)  //方块旋转动画，小写字母代表逆时针，大写字母代表顺时针
    {
        //获取旋转轴和方向
        Vector3 axisVector;
        Quaternion q;//旋转四元数
        bool center = false;//中间轴判断
        switch (ch)
        {
            case 'L': //左顺时针
                axisVector = new Vector3(-1,0,0);q = Quaternion.AngleAxis(90,axisVector); break;
            case 'l': //左逆时针
                axisVector = new Vector3(-1,0,0);q = Quaternion.AngleAxis(-90,axisVector); break;
            case 'R': //右顺时针
                axisVector = new Vector3(1,0,0);q = Quaternion.AngleAxis(90,axisVector);break;
            case 'r': //右逆时针
                axisVector = new Vector3(1,0,0);q = Quaternion.AngleAxis(-90,axisVector);break;
            case 'U': //上顺时针
                axisVector = new Vector3(0,1,0);q = Quaternion.AngleAxis(90,axisVector);break;
            case 'u': //上逆时针
                axisVector = new Vector3(0,1,0);q = Quaternion.AngleAxis(-90,axisVector);break;
            case 'D': //下顺时针
                axisVector = new Vector3(0,-1,0);q = Quaternion.AngleAxis(90,axisVector);break;
            case 'd': //下逆时针
                axisVector = new Vector3(0,-1,0);q = Quaternion.AngleAxis(-90,axisVector);break;
            case 'F': //前顺时针
                axisVector = new Vector3(0,0,-1);q = Quaternion.AngleAxis(90,axisVector);break;
            case 'f': //前逆时针
                axisVector = new Vector3(0,0,-1);q = Quaternion.AngleAxis(-90,axisVector);break;
            case 'B': //后顺时针
                axisVector = new Vector3(0,0,1);q = Quaternion.AngleAxis(90,axisVector);break;
            case 'b': //后逆时针
                axisVector = new Vector3(0,0,1);q = Quaternion.AngleAxis(-90,axisVector);break;
            //中轴旋转，撤回/重做时使用
            case 'X'://绕x轴，逆时针
                axisVector = new Vector3(1,0,0);q = Quaternion.AngleAxis(90,axisVector);center=true;break;
            case 'x'://绕x轴，逆时针
                axisVector = new Vector3(1,0,0);q = Quaternion.AngleAxis(-90,axisVector);center=true;break;
            case 'Y'://绕x轴，逆时针
                axisVector = new Vector3(0,1,0);q = Quaternion.AngleAxis(90,axisVector);center=true;break;
            case 'y'://绕x轴，逆时针
                axisVector = new Vector3(0,1,0);q = Quaternion.AngleAxis(-90,axisVector);center=true;break;
            case 'Z'://绕x轴，逆时针
                axisVector = new Vector3(0,0,-1);q = Quaternion.AngleAxis(90,axisVector);center=true;break; 
            case 'z'://绕x轴，逆时针
                axisVector = new Vector3(0,0,-1);q = Quaternion.AngleAxis(-90,axisVector);center=true;break;
            default:
                axisVector = Vector3.zero;q = Quaternion.AngleAxis(0, axisVector); break;
        }
        //建立旋转轴对象
        if (axis == null)
        {
            if (transform.GetChild(0).localPosition == Vector3.zero)
                axis = transform.GetChild(0);
            else
            {
                axis = new GameObject("axis").transform;
                axis.SetParent(this.transform);
                axis.localPosition = Vector3.zero;
                axis.SetAsFirstSibling();
            }
        }
        //选中这个面的所有方块，设置为旋转轴对象的子物体
        if(center){
            for (int i = 1; i < transform.childCount; i++)
            {
                float tem = Vector3.Dot(transform.GetChild(i).localPosition, axisVector);
                if (tem<1 && tem>-1)
                {
                    transform.GetChild(i).SetParent(axis);
                    i--;
                }
            }
        }
        else
        {
            for (int i = 1; i < transform.childCount; i++)
            {
                if (Vector3.Dot(transform.GetChild(i).localPosition, axisVector) > 1)
                {
                    transform.GetChild(i).SetParent(axis);
                    i--;
                }
            }
        }
        //开始旋转
        float t = Time.time;
        Quaternion start = axis.localRotation;
        Quaternion end = q * axis.localRotation;
        auto = true;
        while (Time.time - t < rotateDuration)
        {
            axis.localRotation = Quaternion.Slerp(start, end, (Time.time - t) / rotateDuration);
            yield return null;
        }
        axis.localRotation = end;
        //旋转结束后解除父子关系
        foreach (Transform c in axis.GetComponentsInChildren<Transform>())
        {
            c.SetParent(this.transform);
        }
        //Destroy(axis.gameObject);
        auto = false;
    }

    public void RotateCubesByString(string str)//输入公式自动旋转魔方 
    {
        if (auto) return;
        if (str == null || str == "") return;
        StartCoroutine(RotateCubesByStringAnimation(str));
    }
    private IEnumerator RotateCubesByStringAnimation(string str)//公式旋转魔方动画
    {
        foreach (char c in str)
        {
            yield return StartCoroutine(CubesRotateAnimation(c));
        }
    }
    #endregion

    #region 自动复原
    public void AutoResetCubes()
    {
        if (auto) return;
        if (historySteps == null || historySteps.Count < 1) return;
        //reset.interactable = false;//禁用重置键
        //revoke.interactable = false;//禁用撤销键
        AddSteps(-steps);//step=0
        redoSteps.Clear();redo.interactable = false;//清除重做栈，禁用重做按钮
        StartCoroutine(AutoResetCubesAnimation());
        PlayAudio(revokeAudio);
    }
    private IEnumerator AutoResetCubesAnimation()
    {
        //print("String: " + rotateSteps);
        while (historySteps.Count > 0)
        {
            char temp = PopStep();
            yield return StartCoroutine(CubesRotateAnimation(ConvertChar(temp)));
        }
    }
    #endregion


    public void RenameCubes()//重命名方块以标记每个方块的位置
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform temp = transform.GetChild(i);
            Vector3 pos = temp.localPosition;
            string rename = pos.x.ToString() + " " + pos.y.ToString() + " " + pos.z.ToString();
            temp.name = rename;
        }
    }
    public void ResetCubes()//强制复原
    {
        AddSteps(-steps);//step=0
        redoSteps.Clear();redo.interactable = false;//清除重做栈，禁用重做按钮
        /*
        for (int i = 1; i < transform.childCount; i++)
        {
            string[] temp = transform.GetChild(i).name.Split(' ');
            Vector3 pos = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
            transform.GetChild(i).localPosition = pos;
            transform.GetChild(i).localEulerAngles = Vector3.zero;
        }*/
        StartCoroutine(ResetCubeAnimation());        
    }
    IEnumerator ResetCubeAnimation() //强制复原动画
    {
        if(!auto)auto = true;
        //记录每个方块的位置
        Vector3[] targets = new Vector3[transform.childCount];
        for (int i = 1; i < transform.childCount;i++){
            targets[i] = 2 * transform.GetChild(i).localPosition;
        }
        float t = Time.time;
        //方块分开
        while(Time.time - t <= rotateDuration){
            for (int i = 1; i < transform.childCount;i++){
                transform.GetChild(i).localPosition =
                    Vector3.Lerp(transform.GetChild(i).localPosition, targets[i], (Time.time - t) / rotateDuration);
            }
            yield return null;
        }
        t = Time.time;
        //归位
        for (int i = 1; i < transform.childCount; i++)
        {
            string[] temp = transform.GetChild(i).name.Split(' ');
            targets[i] = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
            transform.GetChild(i).localPosition = 2*targets[i];
            transform.GetChild(i).localEulerAngles = Vector3.zero;
            yield return null;
        }
        t = Time.time;
        //合拢
        while(Time.time-t <= rotateDuration){
            for (int i = 1; i < transform.childCount;i++){
                transform.GetChild(i).localPosition =
                    Vector3.Lerp(transform.GetChild(i).localPosition, targets[i], (Time.time - t) / rotateDuration);
            }
            yield return null;
        }
        System.GC.Collect();//调用垃圾回收
        auto = false;
    }
    public void Revoke()//撤销
    { 
        if(Auto)return;
        char c = PopStep();//字符出栈
        if(c=='0')return;
        AddSteps(-1);
        //判断栈中余下元素，若为空则禁用撤销按钮
        if (historySteps.Count < 1)revoke.interactable = false;
        RotateCubeByChar(ConvertChar(c));
        PlayAudio(revokeAudio);//播放音效==撤销
        redoSteps.Push(c);
        if(!redo.interactable)redo.interactable = true;//激活重做键
    }
    public void Redo()//重做
    {
        if(Auto)return;
        if(redoSteps.Count<1)return;
        char c = redoSteps.Pop();
        RotateCubeByChar(c);
        PlayAudio(rotateAudio);
        PushStep(c);
        //判断此时的重做栈是否为空
        if(redoSteps.Count<1){
            redo.interactable = false;//禁用重做键
        }
    }

    //字符入栈，如栈满，挤出靠前的元素
    private void PushStep(char ch){
        if(historySteps.Count<maxRevokeSteps)
            historySteps.Add(ch);
            //如果栈溢出，暂时存放到另一个栈中
        else {
            historySteps.RemoveAt(0);
            historySteps.Add(ch);
        }
        AddSteps(1);
    }
    //字符出栈
    private char PopStep(){
        if(historySteps.Count<1)return '0';
        char c = historySteps[historySteps.Count - 1];
        historySteps.RemoveAt(historySteps.Count - 1);
        //AddSteps(-1);
        return c;
    }
    //改变步数
    void AddSteps(int n){
        steps += n;
        if(steps<0)steps = 0;
        uiText.text = steps.ToString();
        if (steps == 0){
            reset.interactable = false;
            revoke.interactable = false;
            uiText.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, .36f);//UI数字变成灰色
            uiText.transform.parent.GetComponent<Text>().color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, .36f);
        }
        else if (steps > 0){
            reset.interactable = true;
            revoke.interactable = true;
            uiText.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 1);//UI数字恢复
            uiText.transform.parent.GetComponent<Text>().color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 1);
        }
    }
    private char ConvertChar(char c)//转换大小写字母
    {
        if (c >= 'a' && c <= 'z') c = char.ToUpper(c);//逆向
        else if (c >= 'A' && c <= 'Z') c = char.ToLower(c);//print("Shifted temp: " + temp);
        return c;
    }
    public void ViewRotateString()
    {
        StringBuilder sb = new StringBuilder("步骤栈: ");
        foreach(char c in historySteps){
            sb.Append(c);
        }
        print(sb);
    }
    public void PlayAudio(AudioClip a){
        audioSource.clip = a;
        audioSource.Play();
    }
    public void SetAudioVolume(float value){ //设置音量
        audioSource.volume = ((float)value)/10f;
    }
    public void SetMaxSteps(float value){ //设置记录的组大步数
        int temp = (int)value;
        if(temp<maxRevokeSteps){//如果最大步数减小，则依次删除栈尾的指令
            while(historySteps.Count > temp)
            historySteps.RemoveAt(0);
        }
        maxRevokeSteps = temp;
    }
    public void SetRotateSpeed(float value)//设置旋转速度 value= [1,5]
    {
        rotateDuration = .5f / value;
    }
}
