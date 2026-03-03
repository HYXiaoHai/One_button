using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;//基础线速度
    public float radius = 3f;//圆周半径
    public float accelerationMultiplier = 2f; //加速倍率

    [Header("中心点")]
    public Transform centerPoint;//原点
    private Vector3 center;//实际使用的中心位置

    private PlayerControls actions;//输入系统
    private int direction = 1;//旋转方向：1 逆时针，-1 顺时针
    private bool isAccelerating = false;//是否处于加速状态
    private float currentAngle;//当前角度（弧度）

    private void Awake()
    {
        actions = new PlayerControls();

        //按键事件
        actions.Gameplay.ChangeDirection.performed += OnChangeDirection;
        actions.Gameplay.Accelerate.performed += OnAcceleratePerformed;   //长按触发
        actions.Gameplay.Accelerate.canceled += OnAccelerateCanceled;//松开触发
    }

    private void OnEnable()
    {
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }

    private void OnDestroy()
    {
        // 取消订阅
        actions.Gameplay.ChangeDirection.performed -= OnChangeDirection;
        actions.Gameplay.Accelerate.performed -= OnAcceleratePerformed;
        actions.Gameplay.Accelerate.canceled -= OnAccelerateCanceled;
    }

    private void Start()
    {
        //确定中心点位置
        center = centerPoint != null ? centerPoint.position : Vector3.zero;

        //初始化角度：以玩家当前位置相对于中心点的方向作为起始角度
        Vector3 offset = transform.position - center;
        currentAngle = Mathf.Atan2(offset.y, offset.x);
    }

    private void Update()
    {
        Move();
    }

    // 处理单击空格：改变方向
    private void OnChangeDirection(InputAction.CallbackContext context)
    {
        direction *= -1;
        Debug.Log("方向切换为：" + (direction == 1 ? "逆时针" : "顺时针"));
    }

    // 长按触发加速（开始加速）
    private void OnAcceleratePerformed(InputAction.CallbackContext context)
    {
        isAccelerating = true;
        Debug.Log("加速开始");
    }

    // 松开时结束加速
    private void OnAccelerateCanceled(InputAction.CallbackContext context)
    {
        isAccelerating = false;
        Debug.Log("加速结束");
    }
    // 圆周运动
    private void Move()
    {
        // 计算当前实际速度
        float currentSpeed = moveSpeed * (isAccelerating ? accelerationMultiplier : 1f);

        // 角速度 = 线速度 / 半径 (弧度/秒)
        float angularSpeed = currentSpeed / radius;

        // 根据方向更新角度
        currentAngle += direction * angularSpeed * Time.deltaTime;

        // 计算新位置
        Vector3 offset = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0) * radius;
        transform.position = center + offset;
    }

    // 可选：在场景中绘制中心点和半径，方便调试
    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoCenter = centerPoint != null ? centerPoint.position : Vector3.zero;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gizmoCenter, radius);
    }
}