using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池
/// </summary>
public static class ObjectsPool
{
    public static bool canGrow = true;//如果池中的物体不够取时，是否可以再生成新的对象
    private static Dictionary<string, ArrayList> pool = new Dictionary<string, ArrayList>();//池体

    /// <summary>
    /// 初始化对象池，根据给定物体(预置体必须在Resources目录下)的名称、数量，在对象池中实例化对应的物体并放置到指定的父物体下
    /// </summary>
    /// <param name="path">预置体路径</param>    
    public static void InitPool(string path, string prefabName, int num, GameObject parent)
    {
        if (pool.ContainsKey(prefabName + "(Clone)")) return;//如果池中已经这种物体，就不能再进行初始化
        pool.Add(prefabName + "(Clone)", new ArrayList());//在字典中，依据某类对象的名字创建一个列表
        for (int i = 0; i < num; i++)//按给定的初始化数量循环，创建指定数量的物体
        {
            //根据路径和名称实例化物体
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load(path + prefabName), parent.transform.position, parent.transform.rotation) as GameObject;
            obj.SetActive(false);//初始时，物体应该是不可用
            obj.transform.SetParent(parent.transform); //放置在父物体下         
            pool[obj.name].Add(obj);//在字典中为该类对象增加成员
        }
    }
    /// <summary>
    /// 从对象池中取物体，物体实际上并没有离开对象池，只是被激活
    /// </summary>
    /// <param name="path">预置体路径</param>
    /// <param name="prefabName">预置体名称</param>
    /// <returns></returns>
    public static object GetFromPool(string path, string prefabName)
    {
        foreach(object o in pool[prefabName + "(Clone)"])//遍历某种物体的对象池
        {
            if((o as GameObject).activeSelf == false)//如果有，没被使用的
            {
                (o as GameObject).SetActive(true);//激活后
                return o;//交给调用处
            }
        }
        if(canGrow)//如果允许增加物体
        {
            //增加一个新物体
            GameObject obj = MonoBehaviour.Instantiate(Resources.Load(path + prefabName)) as GameObject;
            pool[obj.name].Add(obj);//添加到字典中
            return obj;
        }
        return null;
    }

    /// <summary>
    /// 将被调用的物体收回到池里，实际上池里始终有物体的引用，这里只是将物体禁用
    /// </summary>
    /// <param name="obj">返回的物体</param>
    /// <param name="parent">物体被禁用后，应置于指定父物体下</param>
    public static void ReturnToPool(GameObject obj, GameObject parent)
    {
        if(pool.ContainsKey(obj.name))//如果对象里包含这种物体
        {
            obj.SetActive(false);//禁用
            obj.transform.SetParent(parent.transform);//改变在场景中的位置
        }
    }
}
