using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragMove : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, ICanvasRaycastFilter
{
    GameObject outermostWindow; //背包最底层的图片，也是最外侧的边框
    GameObject tempUI; //为了让小格子的前景UI在被拖动时始终在最前面，要把它暂时移动到这个临时UI下（作为临时UI的子物体）。因为临时UI物体在Canvas的最下面，所以在前面。
    bool isCanReceiveRaycast = true; //当前脚本挂载的物体就是小格子的前景物体，当它被拖动的时候（在松开前）不能再接收射线
    bool isMouseWithinUIScope = true;//鼠标位置是否在UI的范围内，刚开始拖动时，肯定是在的

    GameObject oldParent; //小格子前景物体被拖动之前所在的位置
    /// <summary>
    /// 当前物体是否接收射线检测
    /// </summary>
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        //return false 这个物体将不会被射线击中
        //return true 这个物体将会被射线击中
        if (isCanReceiveRaycast == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 当前物体被拖动的开始
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isMouseWithinUIScope = true; //这时鼠标位置一定是在UI范围内的

            isCanReceiveRaycast = false; //不再接收射线检测
            oldParent = transform.parent.gameObject; //记录拖动前的位置
            transform.SetParent(tempUI.transform); //移动临时位置，以便可以显示在最前

            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); //被拖动时缩小
        }

    }
    /// <summary>
    /// 拖动中
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isMouseWithinUIScope == true) //如果上一次检测时鼠标位置还在UI范围内
            {
                if (eventData.pointerCurrentRaycast.gameObject == null) //返回null表明鼠标位置超出了UI的范围
                {
                    isMouseWithinUIScope = false; //设置鼠标位置不在UI范围内的标志
                    transform.SetParent(oldParent.transform); //让拖动出来的UI回到拖动之前的位置                    
                }
                if (EdgeDetection()) //如果UI没有超出边界
                {
                    GetComponent<RectTransform>().pivot.Set(0, 0);
                    transform.position = Input.mousePosition; //跟随鼠标移动
                }
            }
            else //如果鼠标拖动UI物体移到时，鼠标位置离开了UI的范围，这时鼠标将不再对UI有控制权，UI要回到之前的位置
            {
                transform.localPosition = new Vector3(0, 0, 0); //相对于父物体居中
                transform.localScale = new Vector3(1f, 1f, 1f); //恢复大小
            }
        }

    }
    /// <summary>
    /// 结束拖动（松开鼠标）
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isMouseWithinUIScope == true) //如果鼠标位置始终没有超出UI的范围，就执行松开鼠标的行为
            {

                //空格子，直接放入
                if (eventData.pointerCurrentRaycast.gameObject.tag == "UCell" && eventData.pointerCurrentRaycast.gameObject.transform.childCount == 0)
                {
                    if (eventData.pointerCurrentRaycast.gameObject.transform.childCount == 0)
                    {
                        transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform);
                        KnapsackManager.Instance.Drag2Empty(oldParent, eventData.pointerCurrentRaycast.gameObject);
                    }
                }
                //有东西的格子，交换
                else if (eventData.pointerCurrentRaycast.gameObject.tag == "UCell" && eventData.pointerCurrentRaycast.gameObject.transform.childCount != 0)
                {
                    GameObject targetObject = eventData.pointerCurrentRaycast.gameObject.transform.GetChild(0).gameObject; //记录目标物体，因为鼠标的位置是小格子背景与前景之间的缝隙，所以目标物体是当前鼠标指向的（小格子）背景所包含的前景
                    targetObject.transform.SetParent(oldParent.transform); //让目标物体到当前拖动物体来的位置
                    targetObject.transform.localPosition = new Vector3(0, 0, 0);//居中

                    transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform);//当前被拖动的物体去到目标位置
                    transform.localPosition = new Vector3(0, 0, 0);//居中
                }
                //鼠标悬浮于UItem之上，也是有东西的格子，交换
                else if (eventData.pointerCurrentRaycast.gameObject.tag == "UItem")
                {
                    Transform TargetPositon = eventData.pointerCurrentRaycast.gameObject.transform.parent; //获得目标位置，因为当前鼠标指向的是（小格子）前景的Uitem，所以目标位置是目标物体的父物体的transform
                    GameObject targetObject = eventData.pointerCurrentRaycast.gameObject; //获得目标物体
                    targetObject.transform.SetParent(oldParent.transform);// //让目标物体到当前拖动物体来的位置
                    targetObject.transform.localPosition = new Vector3(0, 0, 0);//居中

                    transform.SetParent(TargetPositon);//当前被拖动的物体去到目标位置
                    transform.localPosition = new Vector3(0, 0, 0);//居中
                }
                else
                {
                    transform.SetParent(oldParent.transform);
                }
            }
            transform.localPosition = new Vector3(0, 0, 0);//相对于父物体居中
            transform.localScale = new Vector3(1f, 1f, 1f); //恢复大小
            isCanReceiveRaycast = true; //松开鼠标后，小格子前景物体又可以接收射线了，以便可以接收下一次拖动
        }
    }

    /// <summary>
    /// 检测被移动物体的边框是否超出了背包背景的边框
    /// </summary>
    /// <returns></returns>
    bool EdgeDetection()
    {
        //移动的Item的四个边框的位置
        float currentItemLeftEdge = GetComponent<RectTransform>().position.x - GetComponent<RectTransform>().sizeDelta.x / 2;
        float currentItemRightEdge = GetComponent<RectTransform>().position.x + GetComponent<RectTransform>().sizeDelta.x / 2;
        float currentItemTopEdge = GetComponent<RectTransform>().position.y + GetComponent<RectTransform>().sizeDelta.y / 2;
        float currentItemBottomEdge = GetComponent<RectTransform>().position.y - GetComponent<RectTransform>().sizeDelta.y / 2;

        //背包最外层UI的四个边框的位置
        float outermostWindowLeftEdge = outermostWindow.GetComponent<RectTransform>().position.x - outermostWindow.GetComponent<RectTransform>().sizeDelta.x / 2;
        float outermostWindowRightEdge = outermostWindow.GetComponent<RectTransform>().position.x + outermostWindow.GetComponent<RectTransform>().sizeDelta.x / 2;
        float outermostWindowTopEdge = outermostWindow.GetComponent<RectTransform>().position.y + outermostWindow.GetComponent<RectTransform>().sizeDelta.y / 2;
        float outermostWindowBotteomEdge = outermostWindow.GetComponent<RectTransform>().position.y - outermostWindow.GetComponent<RectTransform>().sizeDelta.y / 2;

        if (currentItemLeftEdge - outermostWindowLeftEdge > 5f && currentItemBottomEdge - outermostWindowBotteomEdge > 5f && outermostWindowRightEdge - currentItemRightEdge > 5f && outermostWindowTopEdge - currentItemTopEdge > 5f)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    // Use this for initialization
    void Start()
    {
        tempUI = GameObject.Find("TempUI");
        outermostWindow = GameObject.Find("Knapsack");
    }

    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// 获取鼠标停留处UI
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    //public GameObject GetOverUI(GameObject canvas)
    //{
    //    PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
    //    pointerEventData.position = Input.mousePosition;
    //    GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
    //    List<RaycastResult> results = new List<RaycastResult>();
    //    gr.Raycast(pointerEventData, results);
    //    if (results.Count != 0)
    //    {
    //        return results[0].gameObject;
    //    }

    //    return null;
    //}

}
