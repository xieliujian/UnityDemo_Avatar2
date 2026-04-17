using UnityEngine;

/// <summary>
/// 摄像机轨道控制器：拖动屏幕水平旋转角色视角。
/// 兼容 WebGL（鼠标）和移动端（单指触摸）。
/// 挂载到 Camera GameObject，将 target 指向角色骨骼根节点。
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("目标")]
    public Transform target;

    [Header("轨道参数")]
    // 参考初始摄像机 Position(-0.10, 1.076, 1.688) 推算
    public float distance    = 1.7f;
    public float height      = 1.08f;
    public float lookAtRatio = 0.8f;   // LookAt 目标 Y = height * lookAtRatio，匹配俯仰角 ~7.4°

    [Header("旋转参数")]
    public float rotateSpeed   = 0.3f;
    public float dragThreshold = 3f;   // 触发拖动所需最小像素移动量，避免点击按钮时误旋转

    [Header("初始角度")]
    // 初始摄像机 Rotation.Y ≈ 179.45°，对应轨道角 180°（摄像机在角色正前方）
    public float initialAngleY = 180f;

    float   mAngleY;
    Vector2 mPointerDownPos;
    Vector2 mLastPointerPos;
    bool    mIsDragging;
    bool    mPointerDown;

    // UI 面板宽度（参考分辨率下），用于屏蔽左侧 UI 区域的拖动
    const float UI_PANEL_REF_WIDTH = 400f;
    const float REF_WIDTH          = 1920f;
    const float REF_HEIGHT         = 1080f;

    void Start()
    {
        mAngleY = initialAngleY;
        UpdateCameraTransform();
    }

    void LateUpdate()
    {
        HandleMouse();
        HandleTouch();
        UpdateCameraTransform();
    }

    // ── 鼠标输入（PC / WebGL 桌面端） ─────────────────────────────────
    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mPointerDownPos  = Input.mousePosition;
            mLastPointerPos  = Input.mousePosition;
            mPointerDown     = true;
            mIsDragging      = false;
        }

        if (mPointerDown && Input.GetMouseButton(0))
        {
            Vector2 cur   = Input.mousePosition;
            Vector2 delta = cur - mLastPointerPos;

            if (!mIsDragging)
            {
                float moved = Vector2.Distance(cur, mPointerDownPos);
                // 超过阈值且起点不在 UI 面板内才开始旋转
                if (moved >= dragThreshold && !IsPointerInUIPanel(mPointerDownPos))
                    mIsDragging = true;
            }

            if (mIsDragging)
                    mAngleY -= delta.x * rotateSpeed;

            mLastPointerPos = cur;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mPointerDown = false;
            mIsDragging  = false;
        }
    }

    // ── 触摸输入（移动端 / 移动端 WebGL） ────────────────────────────
    void HandleTouch()
    {
        if (Input.touchCount != 1)
            return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                mPointerDownPos = touch.position;
                mLastPointerPos = touch.position;
                mPointerDown    = true;
                mIsDragging     = false;
                break;

            case TouchPhase.Moved:
                if (!mIsDragging)
                {
                    float moved = Vector2.Distance(touch.position, mPointerDownPos);
                    if (moved >= dragThreshold && !IsPointerInUIPanel(mPointerDownPos))
                        mIsDragging = true;
                }
                if (mIsDragging)
                    mAngleY -= touch.deltaPosition.x * rotateSpeed;
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                mPointerDown = false;
                mIsDragging  = false;
                break;
        }
    }

    // ── 摄像机位置更新 ────────────────────────────────────────────────
    void UpdateCameraTransform()
    {
        if (target == null)
            return;

        float rad   = mAngleY * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, -Mathf.Cos(rad));

        // 摄像机位置：固定高度 + 水平轨道偏移
        transform.position = target.position + dir * distance + Vector3.up * height;

        // LookAt 目标固定在角色中心高度，保证任意旋转角度仰角不变
        Vector3 lookAtPoint = target.position + Vector3.up * (height * lookAtRatio);
        transform.LookAt(lookAtPoint);
    }

    // ── 工具：判断起点是否在左侧 UI 面板内 ───────────────────────────
    bool IsPointerInUIPanel(Vector2 screenPos)
    {
        float scale         = Mathf.Min(Screen.width / REF_WIDTH, Screen.height / REF_HEIGHT);
        float uiPanelWidth  = UI_PANEL_REF_WIDTH * scale;
        // screenPos.x 从屏幕左侧计算
        return screenPos.x < uiPanelWidth;
    }

    // ── 公开接口：由外部重置角度（如切换角色时回正） ─────────────────
    public void ResetAngle()
    {
        mAngleY = initialAngleY;
    }
}
