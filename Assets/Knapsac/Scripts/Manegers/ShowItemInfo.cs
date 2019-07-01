using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowItemInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject showInfo; //显示物体信息的实例
    /// <summary>
    /// 鼠标悬浮与物体之上
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var item in KnapsackManager.ItemList) //遍历存储物品信息的类
        {
            if (GetComponent<Image>().sprite.name == item.Value.Icon) //如果找到与当前鼠标指向图片一致的类
            {
                showInfo.GetComponent<Text>().text = item.Value.GetInfo(); //显示所有信息
            }
        }
    }
    /// <summary>
    /// 鼠标离开
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        showInfo.GetComponent<Text>().text = "";
    }

    // Use this for initialization
    void Start()
    {
        showInfo = GameObject.Find("Info");
    }

    // Update is called once per frame
    void Update()
    {

    }

}
