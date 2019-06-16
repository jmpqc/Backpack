using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : BaseItem {//武器属性
    public int Attack
    {
        get;
        private set;
    }
    public Weapons(int id, string name, string description, int buyPrice, int sellPrice, string icon, int attack) : base(id, name, description, buyPrice, sellPrice, icon)
    {
        Attack = attack;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + Attack;
    }
}
