using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using XLua;

public class KnapsackManager : MonoBehaviour //背包分为前端和后端，后端包含背包的各种属性，包括（用于在前端显示的）图片资源的路径及名称。前端用于在视图中显示，控制实际用来显示的游戏物体
{
    private static KnapsackManager instance; //单例字段
    public static KnapsackManager Instance //单例属性
    {
        get { return instance; }
        set { }
    }



    LuaEnv luaenv;//创建Lua环境
    //背包的前端也分为前景和背景。背景是不会有变化的，包括背景图，Panel窗口，和每个小格子的背景。前景是一个或多个Image的UI物体，在需要的时候它的Sprite会引用一个图片资源，并成为某个小格子背景的子物体，成为小格子的前景
    GameObject item; //【前端】来用接收实例化背包(前景)物体的引用（是一有Image物体，会有一个<Imgae>组件，sprite指向一个图片，然后成为某个小格子的前景），
    Dictionary<string, Sprite> spriteItems = new Dictionary<string, Sprite>();//【前端】存放背包物体大图里所有被分割的小图，因为小图并没有单独形成文件
    public static Dictionary<int, BaseItem> ItemList = new Dictionary<int, BaseItem>();//【前端】背包里的物体，显示在游戏窗口里的只是这个物体对应的图片，是物体的一个属性

    Dictionary<string, object> dictLuaTable = new Dictionary<string, object>(); //【后端】映射LuaTable，获取Lua表里存放的背包物品及属性

    //在拾取一个物体的时候，（背景）空格子（Empty）有可能会少一个，（背景）非空（Full）格子有可能会多一个，所以使用两个列表分别记录空格子和非空格子，以便遍历
    SortedDictionary<int, GameObject> UEmptyCells;//自动排序
    SortedDictionary<int, GameObject> UFullCells;//自动排序

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        luaenv = new LuaEnv();
    }

    // Use this for initialization
    void Start()
    {
        GameObject parentInScene = GameObject.Find("CanvasPool");//【前端】在前景物体不被使用时，放在它对应的（场景中）池管理的父对象下
        ObjectsPool.InitPool("Prefabs/", "UItem", 3, parentInScene);//在对象池中，初始化这个小格子前景对象

        UEmptyCells = new SortedDictionary<int, GameObject>();//空格子列表
        UFullCells = new SortedDictionary<int, GameObject>();//非空格子列表


        int i = 0;//临时计数变量
        foreach (Transform child in transform)//【前端】当前物体是容纳所有（背景）小格子的panel，这个遍历可以获得所有（背景）小格子。
        {//这里的child就是第一个Grid下每一个UCell(0)
            UEmptyCells.Add(i++, child.gameObject);//【前端】将所有的背景小格子装入空格子列表    
        }

        Load();//加载背包中可能用到的物品
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            int index = Random.Range(0, 4);
            Pickup(ItemList[index]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Load()
    {
        //【前端】加载Sprite资源
        Sprite[] allSmallSprites = Resources.LoadAll<Sprite>("Picture/UIElement");//通过这种方式可以获取到所有的小图资源
        foreach (Sprite s in allSmallSprites)
        {
            spriteItems.Add(s.name, s);//将小图资源全部存入字典里，以便可以通过名称调用对应的资源
        }



        //【后端】加载背包物品
        luaenv.DoString("require 'LuaCode/ItemAttribute'");//载入Lua文件

        //读取一种武器的信息
        dictLuaTable = luaenv.Global.Get<Dictionary<string, object>>("sword_dragonslayer");
        Weapons sword_dragonslayer = new Weapons(int.Parse((dictLuaTable["id"]).ToString()), (string)dictLuaTable["name"], (string)dictLuaTable["description"], int.Parse(dictLuaTable["buyPrice"].ToString()), int.Parse(dictLuaTable["sellPrice"].ToString()), (string)dictLuaTable["icon"], int.Parse(dictLuaTable["attack"].ToString()));

        //读取一种武器的信息
        dictLuaTable = luaenv.Global.Get<Dictionary<string, object>>("sword_machete");
        Weapons sword_machete = new Weapons(int.Parse((dictLuaTable["id"]).ToString()), (string)dictLuaTable["name"], (string)dictLuaTable["description"], int.Parse(dictLuaTable["buyPrice"].ToString()), int.Parse(dictLuaTable["sellPrice"].ToString()), (string)dictLuaTable["icon"], int.Parse(dictLuaTable["attack"].ToString()));

        //读取一种消耗器的信息
        dictLuaTable = luaenv.Global.Get<Dictionary<string, object>>("HP_back");
        Consumable HP_back = new Consumable(int.Parse((dictLuaTable["id"]).ToString()), (string)dictLuaTable["name"], (string)dictLuaTable["description"], int.Parse(dictLuaTable["buyPrice"].ToString()), int.Parse(dictLuaTable["sellPrice"].ToString()), (string)dictLuaTable["icon"], int.Parse(dictLuaTable["backHP"].ToString()), int.Parse(dictLuaTable["backMP"].ToString()));

        //读取一种消耗器的信息
        dictLuaTable = luaenv.Global.Get<Dictionary<string, object>>("MP_back");
        Consumable MP_back = new Consumable(int.Parse((dictLuaTable["id"]).ToString()), (string)dictLuaTable["name"], (string)dictLuaTable["description"], int.Parse(dictLuaTable["buyPrice"].ToString()), int.Parse(dictLuaTable["sellPrice"].ToString()), (string)dictLuaTable["icon"], int.Parse(dictLuaTable["backHP"].ToString()), int.Parse(dictLuaTable["backMP"].ToString()));

        //将刚刚读取的（带各种属性）背包物品存入到一个字典里，以可以便通过名称调用对应的物品对像
        if (!ItemList.ContainsKey(sword_dragonslayer.Id)) ItemList.Add(sword_dragonslayer.Id, sword_dragonslayer);
        if (!ItemList.ContainsKey(sword_machete.Id)) ItemList.Add(sword_machete.Id, sword_machete);
        if (!ItemList.ContainsKey(HP_back.Id)) ItemList.Add(HP_back.Id, HP_back);
        if (!ItemList.ContainsKey(MP_back.Id)) ItemList.Add(MP_back.Id, MP_back);
    }

    //拾取物品，通过【后端】的数据显示【前端】的图像
    void Pickup(BaseItem baseItem)
    {
        //先遍历非空格子列表，判断已经有的这个东西与正要拾取的这个东西是不是同一种，如果不是一种东西，遍历下一个非空格子。
        //如果是同一种，非空格子里的数量+1，return
        //如果在非空格子列表中没有找到一样的东西，取空格子列表里的第一个空格子，放入拾取的物品，然后return

        foreach (var cell in UFullCells)//遍历非空格子集合
        {
            if (cell.Value.transform.GetChild(0).GetComponent<Image>().sprite.name == baseItem.Icon)
            {
                Text t = cell.Value.transform.GetChild(0).GetChild(0).GetComponent<Text>();
                t.text = (int.Parse(t.text) + 1).ToString();
                return;
            }
        }
        //如果上面没有找到一样的，就要放到一个空格子里
        item = ObjectsPool.GetFromPool("UItem") as GameObject;//从对象池取一个（前景）物体
        if (item == null) return;//当对象池不允许新增物体时，就可能取不到物体
        item.GetComponent<Image>().sprite = spriteItems[baseItem.Icon];//为物体设置一张图片

        item.transform.SetParent(UEmptyCells.ElementAt(0).Value.transform);//为物体设置新的位置，放到一个空格子里
        item.transform.localPosition = Vector3.zero;

        EmptyCellsToFullCells();//空格子列表里的第一个格子转给非空格子列表

        item = null;//item变量不再指向该变量
        return;

    }

    /// <summary>
    ///【前端】 通过（路径）名称调用对应的sprite
    /// </summary>
    /// <param name="iconName"></param>
    /// <returns></returns>
    private Sprite GetIcon(string iconName)
    {
        Sprite sprite = new Sprite();
        if (spriteItems.TryGetValue(iconName, out sprite))
        {
            return sprite;
        }
        return null;
    }


    /// <summary>
    /// 当拾取到一个新的物品时，要放到空的格子里，这时空格子少了一个，需非空格子多了一个。就是把空格子列表里的第一个空格子转给非空格子列表
    /// </summary>
    void EmptyCellsToFullCells()
    {
        int key = UEmptyCells.ElementAt(0).Key;//空格子列表中的第一个空格子的key
        GameObject value = UEmptyCells.ElementAt(0).Value;//空格子列表中的第一个空格子的value
        UFullCells.Add(key, value);//将这个格子转给UFullCells列表
        UEmptyCells.Remove(key);//在空格子列表中删除这个格子
    }


    /// <summary>
    /// 当通过鼠标点击格子里的图片，当数量为1时再点击图片就会消失，格子就会从非空变为空。这时，要将这个格子从非空格子列表转向空格子列表
    /// 这个方法，由ConsumptionManaer类调用
    /// </summary>
    /// <param name="item">需要回到对象池的Uitem</param>
    void FullCellsToEmptyCells(object item)
    {
        GameObject cell = (item as GameObject).transform.parent.gameObject;//找到存入这个Uitem的格子，这个格子正在由非空变为空
        foreach (var v in UFullCells)//遍历列表
        {
            if (v.Value == cell)//找到这个格子
            {
                UEmptyCells.Add(v.Key, v.Value);//转给空格子列表
                UFullCells.Remove(v.Key);//从非空格子列表中删除
                return;
            }
        }
    }

    private void OnDisable()
    {
        luaenv.Dispose();//消毁Lua环境变量
    }

    /// <summary>
    /// 将一个UItem从一个UCell中拖到一个空的UCell时调用该方法
    /// 空格子列表与非空格子列表进行交换
    /// 有东西的格子变成没东西的
    /// 没有东西的格子变成有东西的
    /// </summary>
    /// <param name="oldParent">Utem被拖出的UCell</param>
    /// <param name="newParent">Utem被拖入的UCell</param>
    public void Drag2Empty(GameObject oldParent, GameObject newParent)
    {
        foreach (var cell in UFullCells) //遍历非空格子列表
        {
            if (cell.Value.name == oldParent.name) //找到物品被拖动之前的格子
            {
                UFullCells.Remove(cell.Key); //从非空格子列表中删除该格子
                UEmptyCells.Add(cell.Key, cell.Value); //加入到空格子列表中
                break;
            }
        }

        foreach (var cell in UEmptyCells) //遍历空格子列表
        {
            if (cell.Value.name == newParent.name) //找到物品被拖动之后的新位置，的格子
            {
                UEmptyCells.Remove(cell.Key); //从空格子列表中删除这个格子
                UFullCells.Add(cell.Key, cell.Value); //加入非空格子列表中
                break;
            }
        }

    }
}
