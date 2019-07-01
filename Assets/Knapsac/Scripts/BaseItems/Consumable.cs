using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : BaseItem //消耗品属性
{
    public int BackHP
    {
        get;
        private set;
    }
    public int BackMP
    {
        get;
        private set;
    }
    public Consumable(int id, string name, string description, int buyPrice, int sellPrice, string icon, int backHP, int backMP) : base(id, name, description, buyPrice, sellPrice, icon)
    {
        BackHP = backHP;
        BackMP = backMP;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + BackHP + "\n" + BackMP;
    }
}