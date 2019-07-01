using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsumptionManager : MonoBehaviour
{
    GameObject parentInScene;
    //GameObject grid;
    // Use this for initialization
    void Start()
    {
        parentInScene = GameObject.Find("CanvasPool");//【前端】在前景物体不被使用时，在放在它对应的（场景中）池管理的父对象下
        //grid = GameObject.Find("Grid");
    }

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 【前端】鼠标点击背包物品后，减少对应物品显示数量或从背包中删掉该物品
    /// </summary>
    public void OnClick()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Text t = transform.GetChild(0).GetComponent<Text>();
            if (int.Parse(t.text) > 1)
            {
                t.text = (int.Parse(t.text) - 1).ToString();
            }
            else
            {
                //transform.parent.parent场景中的Grid(Grid0或Grid1)物体，它的组件KnapsackManager代码中有一个方法FullCellsToEmptyCells。在点击格子里的图片UI后，需要调用这个方法，将这个格子从UFullCells转到UEmptyCells。
                transform.parent.parent.SendMessage("FullCellsToEmptyCells", gameObject);
                Invoke("ReturnToPool", 0.02f);
            }
        }
    }

    void ReturnToPool()
    {
        ObjectsPool.ReturnToPool(gameObject, parentInScene);//将物体放回对象池

    }



}
