using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem{ //背包物品的基础属性

    public int Id
    {
        get;
        private set;
    }
    public string Name
    {
        get;
        private set;
    }
    public string Description
    {
        get;
        private set;
    }
    public int BuyPrice
    {
        get;
        private set;
    }
    public int SellPrice
    {
        get;
        private set;
    }
    public string Icon
    {
        get;
        private set;
    }


    public BaseItem(int id, string name, string description, int buyPrice, int sellPrice, string icon)
    {
        Id = id;
        Name = name;
        Description = description;
        BuyPrice = buyPrice;
        SellPrice = sellPrice;
        Icon = icon;
    }

    public virtual string GetInfo()
    {
        return Name + "\n" + Description + "\n" + BuyPrice + "\n" + SellPrice + "\n" + Icon + "\n"; 
    }
}
