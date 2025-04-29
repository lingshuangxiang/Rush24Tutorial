using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleMatchTimer : MonoBehaviour
{
    public Text timerText; // 引用UI上的Text组件，用于显示时间
    private float timer = 0f; // 计时器变量，初始化为0
    public static float CurrentTime; // 当前秒数

    void Update()
    {
        if (timer <= 300) // 确保计时器时间大于等于0
        {
            timer += Time.deltaTime; // 每帧减少Time.deltaTime的时间
            CurrentTime = timer;
            int minutes = Mathf.FloorToInt(timer / 60); // 计算分钟数
            int seconds = Mathf.FloorToInt(timer % 60); // 计算剩余秒数
            timerText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2"); // 格式化时间并显示
        }
    }

    public void ResetTimer()
    {
        timer = 0;
        CurrentTime = 0;
        timerText.text ="00:00";
    }
}
