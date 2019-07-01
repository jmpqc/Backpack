using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armors : BaseItem
{//防御力属性
    public int Defense
    {
        get;
        private set;
    }
    public Armors(int id, string name, string description, int buyPrice, int sellPrice, string icon, int defense) : base(id, name, description, buyPrice, sellPrice, icon)
    {
        Defense = defense;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + Defense;
    }
}
