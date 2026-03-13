using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;//默认速度
    public float radius = 3f;//圆周半径
    public float maxSpeed = 10f;//最大速度（加速上限 + 双击目标）

    [Header("加速/减速参数")]
    public float accelerationRate = 5f;//加速度
    public float decelerationRate = 3f;//减速度

    //[Header("双击参数")]
    //public float doubleClickThreshold = 0.2f;//双击最大间隔
    [Header("特效")]
    public TrailRenderer playerTril;
    public ParticleSystem playerPartical;

    [Header("中心点")]
    public Transform centerPoint;
    private Vector3 center;

    private PlayerControls actions;//按键检测
    public int direction = 1;//移动方向 用来转向
    private float currentAngle;//当前角度

    //速度相关变量
    private float currentSpeed;//当前速度
    private float targetSpeed;//目标速度
    private bool isAccelerating = false;//是否正在长按加速
    private bool isMaxSpeedMode = false;//是否处于最大速度模式
    //private float storedSpeed;//双击前保持的速度（用于恢复）
    [Header("加速视觉效果")]
    public Camera mainCam;
    public float normalCamSize = 5f;
    public float boostCamSize = 6f;
    public float effectDuration = 0.2f;


    //双击检测
    //private float lastPressTime = 0f;
    private Coroutine turnCoroutine;

    private void Awake()
    {
        actions = new PlayerControls();

        actions.Gameplay.Accelerate.performed += OnAcceleratePerformed;
        actions.Gameplay.Accelerate.canceled += OnAccelerateCanceled;
        //actions.Gameplay.MaxSpeed.performed += OnMaxSpeed;
        actions.Gameplay.ChangeDirection.performed += ChangeDirection;
    }

    private void OnEnable() => actions.Enable();
    private void OnDisable() => actions.Disable();

    private void OnDestroy()
    {
        actions.Gameplay.ChangeDirection.performed -= ChangeDirection;

        actions.Gameplay.Accelerate.performed -= OnAcceleratePerformed;
        actions.Gameplay.Accelerate.canceled -= OnAccelerateCanceled;
        //actions.Gameplay.MaxSpeed.performed -= OnMaxSpeed;
    }

    private void Start()
    {
        center = centerPoint != null ? centerPoint.position : Vector3.zero;
        Vector3 offset = transform.position - center;
        currentAngle = Mathf.Atan2(offset.y, offset.x);

        //初始化速度
        currentSpeed = moveSpeed;
        targetSpeed = moveSpeed;
        //storedSpeed = moveSpeed;
    }

    private void Update()
    {
        // 根据游戏状态动态启用/禁用 Gameplay Action Map
        bool shouldBeActive = (GameManage.instance != null && GameManage.instance.gameState == GameState.Start);
        if (actions.Gameplay.enabled != shouldBeActive)
        {
            if (shouldBeActive)
                actions.Gameplay.Enable();
            else
                actions.Gameplay.Disable();
        }

        // 非 Start 状态不执行移动逻辑
        if (!shouldBeActive) return;

        UpdateSpeed();
        Move();
        UpdateRotation();
    }

    //核心速度更新逻辑
    private void UpdateSpeed()
    {
        if (isMaxSpeedMode)
        {
            targetSpeed = maxSpeed;
        }
        else
        {
            if (isAccelerating)
            {
                //长按加速
                targetSpeed += accelerationRate * Time.deltaTime;
                targetSpeed = Mathf.Min(targetSpeed, maxSpeed);
            }
            else
            {
                //松开后：目标速度向默认速度 moveSpeed 线性减少
                targetSpeed -= decelerationRate * Time.deltaTime;
                targetSpeed = Mathf.Max(targetSpeed, moveSpeed);
            }
        }

        //将当前速度过渡到目标速度
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.1f);
        //currentSpeed = targetSpeed;
    }

    //长按加速开始
    private void OnAcceleratePerformed(InputAction.CallbackContext context)
    {
        isAccelerating = true;
        playerPartical.Play();
        Debug.Log("加速开始");

        // 镜头拉远
        if (mainCam != null)
            mainCam.DOOrthoSize(boostCamSize, effectDuration);
    }

    //长按加速结束
    private void OnAccelerateCanceled(InputAction.CallbackContext context)
    {
        isAccelerating = false;
        playerPartical.Stop();
        Debug.Log("开始减速");

        if (mainCam != null)
            mainCam.DOOrthoSize(normalCamSize, effectDuration);
    }

    ////双击切换最大速度
    //private void OnMaxSpeed(InputAction.CallbackContext context)
    //{
    //    float currentTime = Time.time;
    //    if (currentTime - lastPressTime <= doubleClickThreshold)
    //    {
    //        //双击成功
    //        if (isMaxSpeedMode)
    //        {
    //            //退出最大速度模式
    //            isMaxSpeedMode = false;
    //            targetSpeed = storedSpeed;
    //            Debug.Log("退出最大速度模式，速度恢复为 " + storedSpeed);
    //        }
    //        else
    //        {
    //            //进入最大速度模式，保存当前速度
    //            isMaxSpeedMode = true;
    //            storedSpeed = currentSpeed;//保存当前速度
    //            targetSpeed = maxSpeed;
    //            Debug.Log("进入最大速度模式，速度设为 " + maxSpeed);
    //        }
    //        lastPressTime = 0f;

    //        //取消正在等待的转向
    //        if (turnCoroutine != null)
    //        {
    //            StopCoroutine(turnCoroutine);
    //            turnCoroutine = null;
    //        }
    //    }
    //    else
    //    {
    //        // 第一次按下
    //        lastPressTime = currentTime;
    //    }
    //}
    private void ChangeDirection(InputAction.CallbackContext context)
    {
        direction *= -1;
        Debug.Log("方向切换为：" + (direction == 1 ? "逆时针" : "顺时针"));
    }

    ////单击改变方向（延迟执行）
    //private void OnChangeDirection(InputAction.CallbackContext context)
    //{
    //    if (turnCoroutine != null)
    //        StopCoroutine(turnCoroutine);
    //    turnCoroutine = StartCoroutine(DelayedTurn());
    //}

    //private IEnumerator DelayedTurn()
    //{
    //    yield return new WaitForSeconds(doubleClickThreshold);
    //    if (lastPressTime != 0f)//等待期间没有发生双击
    //    {
    //        direction *= -1;
    //        Debug.Log("方向切换为：" + (direction == 1 ? "逆时针" : "顺时针"));
    //    }
    //    turnCoroutine = null;
    //}

    //圆周运动
    private void Move()
    {
        float angularSpeed = currentSpeed / radius;
        currentAngle += direction * angularSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0) * radius;
        transform.position = center + offset;
    }

    //更新玩家正方向指向圆心
    public void UpdateRotation()
    {
        Vector3 directionToCenter = center - transform.position;
        if (directionToCenter != Vector3.zero)
        {
            transform.up = directionToCenter.normalized;
        }
    }
    //重置状态
    public void ResetPlayer()
    {
        //停止可能正在进行的转向协程
        if (turnCoroutine != null)
        {
            StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        //重置方向
        direction = 1;

        //重置速度相关
        currentSpeed = moveSpeed;
        targetSpeed = moveSpeed;
        isAccelerating = false;
        isMaxSpeedMode = false;
        //storedSpeed = moveSpeed;

        //重置双击检测时间
        //lastPressTime = 0f;

        //根据当前位置重新计算角度（确保与位置一致）
        Vector3 offset = transform.position - center;
        currentAngle = Mathf.Atan2(offset.y, offset.x);
        //强制更新旋转，使玩家指向圆心
        UpdateRotation();
    }

    #region 调试
    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoCenter = centerPoint != null ? centerPoint.position : Vector3.zero;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gizmoCenter, radius);
    }
    #endregion
}