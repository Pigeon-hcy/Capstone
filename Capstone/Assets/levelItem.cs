using UnityEngine;
using UnityEngine.UI;
using SkateGame;

public class levelItem : MonoBehaviour
{
    public string SceneName;  // 场景名
    public string Name;       // 关卡名（显示用）
    public Button button;     // 对应 UI 按钮
    public Sprite image;      // 对应图片（如关卡缩略图）


    public void setUp(Level level){
        SceneName = level.SceneName;
        Name = level.Name;
        image = level.image;
    }
    

}
