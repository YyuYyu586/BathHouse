using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("要跟随的目标")]
    public Transform target;

    [Header("平滑度")]
    public float smoothSpeed = 0.125f;

    [Header("摄像机偏移量")]
    private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("地图边界设置 (在这填你量出来的数值)")]
    public float minX; // 摄像机能往左走的极限
    public float maxX; // 摄像机能往右走的极限
    public float minY; // 摄像机能往下走的极限
    public float maxY; // 摄像机能往上走的极限

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. 计算摄像机理想的位置
            Vector3 desiredPosition = target.position + offset;

            // 2. 限制 X 轴坐标 (Mathf.Clamp 会让数值卡在最小值和最大值之间)
            float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);

            // 3. 限制 Y 轴坐标
            float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

            // 4. 组合成最终的坐标
            Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

            // 5. 平滑移动过去
            transform.position = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);
        }
    }
}