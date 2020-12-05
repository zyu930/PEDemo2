﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    工作学习, 心体恢复, 谋划野心, 关系交往
}
//基类
public class Event
{
    public int FailureNum = 0;//用来检测干预事件发生前是大失败还是失败
    public int MinFaith = 101;
    public int MotivationRequire = 3; //1弱 2弱中 3弱中强 4中 5中强 6强
    public int MoralRequire = 0; // 0无 1功利主义 2中庸 3绝对律令
    public int ReligionRequire = 0; // 0无 1机械 2中庸 3人文
    public int RelationRequire = 0; //0无 1朋友 2挚友 3徒弟  4师傅 5倾慕 6追求 7情侣 8伴侣 -1陌路 -2仇人
    public int PerkRequire = 0;     //特质需求(Perk编号)
    public int PerkRemoveNumber;    //要移除的特质 （20.12.5新增）
    public int TimeLeft = 2;
    public bool HaveTarget = true;  //是否有目标
    public bool isSolving = false;
    public string EventName = "无";
    public string ResultText = "无";
    public int Weight = 1;          //事件权重

    public GameControl GC;
    public Employee Self, Target;
    public EmpEntity SelfEntity { get { return Self.InfoDetail.Entity; } }
    public EmpEntity TargetEntity { get { return Target.InfoDetail.Entity; } }
    public Building TargetBuilding;
    public BuildingType BuildingRequire = BuildingType.空; //暂时用空表示没有需求
    public List<EColor> SelfEmotionRequire = new List<EColor>(); //自身颜色需求
    public List<EColor> TargetEmotionRequire = new List<EColor>(); //对方颜色需求
    public List<Event> SubEvents = new List<Event>(); //可能的子事件
    public List<Employee> Targets = new List<Employee>();

    public Event()
    {
        if (GameControl.Instance != null)
            GC = GameControl.Instance;
    }

    //可行性检测
    public virtual bool ConditionCheck(int Motivation)
    {
        //防止没设置上
        if (GC == null)
            GC = GameControl.Instance;

        //建筑需求检测
        if (BuildingCheck() == false)
            return false;

        //最低信念检测
        if (Self.Character[4] >= MinFaith)
            return false;

        //情绪检测
        if (EmotionCheck() == false)
            return false;

        //关系检测
        if (HaveTarget == true)
        {
            if (RelationCheck() == false)
                return false;
        }

        //特质检测
        if (PerkCheck() == false)
            return false;

        //动机检测
        if (Motivation != -1)
        {
            if (MotivationCheck(Motivation) == false)
                return false;
        }
        //道德检测
        if (MoralCheck() == false)
            return false;

        //信仰检测
        if (ReligionCheck() == false)
            return false;

        //特殊检测
        if (SpecialCheck() == false)
            return false;

        return true;
    }
    //建筑要求
    public virtual bool BuildingCheck()
    {
        if (BuildingRequire == BuildingType.空)
            return true;
        else
        {
            for (int i = 0; i < GC.CurrentDeps.Count; i++)
            {
                if (GC.CurrentDeps[i].building.Type == BuildingRequire)
                {
                    TargetBuilding = GC.CurrentDeps[i].building;
                    return true;
                }
            }
            for (int i = 0; i < GC.CurrentOffices.Count; i++)
            {
                if (GC.CurrentOffices[i].building.Type == BuildingRequire)
                {
                    TargetBuilding = GC.CurrentOffices[i].building;
                    return true;
                }
            }
            return false;
        }
    }
    //关系要求
    public virtual bool RelationCheck()
    {
        if (Target == null)
        {
            if (Self.Relations.Count > 0)
            {
                Target = Self.Relations[Random.Range(0, Self.Relations.Count)].Target;
                return true;
            }
            return false;
        }
        else
        {
            if (RelationRequire == 0)
                return true;
            else if (RelationRequire < 3)
            {
                if (Self.FindRelation(Target).FriendValue == RelationRequire)
                    return true;
                else
                    return false;
            }
            else if (RelationRequire < 5)
            {
                if (Self.FindRelation(Target).MasterValue == RelationRequire - 2)
                    return true;
                else
                    return false;
            }
            else if (RelationRequire < 9)
            {
                if (Self.FindRelation(Target).LoveValue == RelationRequire - 4)
                    return true;
                else
                    return false;
            }
            return false;
        }
    }
    //动机要求
    public virtual bool MotivationCheck(int Mo)
    {
        if (MotivationRequire == 1)
        {
            if (Mo < 20)
                return true;
        }
        else if (MotivationRequire == 2)
        {
            if (Mo < 40)
                return true;
        }
        else if (MotivationRequire == 3)
            return true;
        else if (MotivationRequire == 4)
        {
            if (Mo >= 20 && Mo < 40)
                return true;
        }
        else if (MotivationRequire == 5)
        {
            if (Mo >= 20)
                return true;
        }
        else if (MotivationRequire == 6)
        {
            if (Mo >= 40)
                return true;
        }
        return false;
    }
    //情绪要求
    public virtual bool EmotionCheck()
    {
        if (SelfEmotionRequire.Count == 0 && TargetEmotionRequire.Count == 0)
            return true;
        else if (TargetEmotionRequire.Count == 0)
        {
            for(int i = 0; i < SelfEmotionRequire.Count; i++)
            {
                for(int j = 0; j < Self.CurrentEmotions.Count; j++)
                {
                    if (Self.CurrentEmotions[j].color == SelfEmotionRequire[i])
                        return true;
                }
            }
            return false;
        }
        else if (SelfEmotionRequire.Count == 0)
        {
            for (int i = 0; i < TargetEmotionRequire.Count; i++)
            {
                for (int j = 0; j < Target.CurrentEmotions.Count; j++)
                {
                    if (Target.CurrentEmotions[j].color == TargetEmotionRequire[i])
                        return true;
                }
            }
            return false;
        }
        else
        {
            for (int i = 0; i < TargetEmotionRequire.Count; i++)
            {
                for (int j = 0; j < Target.CurrentEmotions.Count; j++)
                {
                    if (Target.CurrentEmotions[j].color == TargetEmotionRequire[i])
                        break;
                }
                if (i == TargetEmotionRequire.Count - 1)
                    return false;
            }

            for (int i = 0; i < SelfEmotionRequire.Count; i++)
            {
                for (int j = 0; j < Self.CurrentEmotions.Count; j++)
                {
                    if (Self.CurrentEmotions[j].color == SelfEmotionRequire[i])
                        return true;
                }
            }
            return false;
        }
    }
    //特质需求
    public virtual bool PerkCheck()
    {
        if (PerkRequire == 0)
            return true;
        else
        {
            for(int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
            {
                if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == PerkRequire)
                    return true;
            }
            return false;
        }
    }

    public virtual void PerkRemoveCheck()
    {
        if (PerkRemoveNumber == 0)
            return;
        else
        {
            for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
            {
                if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == PerkRemoveNumber)
                {
                    Self.InfoDetail.PerksInfo[i].CurrentPerk.RemoveEffect();
                    return;
                }
            }
        }
    }

    //其他要求
    public virtual bool SpecialCheck()
    {
        return true;
    }
    public bool MoralCheck()
    {
        if (MoralRequire == 0)
            return true;
        else if (MoralRequire == 1 && Self.CharacterTendency[2] == -1)
            return true;
        else if (MoralRequire == 2 && Self.CharacterTendency[2] == 0)
            return true;
        else if (MoralRequire == 3 && Self.CharacterTendency[2] == 1)
            return true;

        return false;
    }
    public bool ReligionCheck()
    {
        if (ReligionRequire == 0)
            return true;
        else if (ReligionRequire == 1 && Self.CharacterTendency[1] == -1)
            return true;
        else if (ReligionRequire == 2 && Self.CharacterTendency[1] == 0)
            return true;
        else if (ReligionRequire == 3 && Self.CharacterTendency[1] == 1)
            return true;

        return false;
    }
    //子事件检测
    public Event SubEventCheck()
    {
        if (SubEvents.Count == 0)
            return null;
        Event E = null;
        List<Event> EL = new List<Event>();
        for (int i = 0; i < SubEvents.Count; i++)
        {
            SubEvents[i].Self = Self;
            SubEvents[i].Target = Target;
            if (SubEvents[i].ConditionCheck(-1) == true)
                EL.Add(SubEvents[i]);
        }
        if (EL.Count > 0)
            E = EL[Random.Range(0, EL.Count)];
        return E;
    }

    //执行时间效果
    public virtual void EventFinish()
    {
        isSolving = false;
        //再检测一下事件是否还有效
        if (HaveTarget == true && Target == null)
        {
            return;
        }
        int result = FindResult();
        float Posb = Random.Range(0.0f, 1.0f);
        if (result == 1)
            MajorFailure(Posb);
        else if (result == 2)
            Failure(Posb);
        else if (result == 3)
            Success(Posb);
        else if (result == 4)
            MajorSuccess(Posb);
        AddHistory();
        MonoBehaviour.print(Self.Name + "发生了事件" + EventName);
    }
    //事件结果判定
    public virtual int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 2)
        {
            ResultText = "大失败,";
            return 1;
        }
        else if (value <= 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功,";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    //四种结果
    public virtual void MajorFailure(float Posb)
    {

    }
    public virtual void Failure(float Posb)
    {

    }
    public virtual void Success(float Posb)
    {

    }
    public virtual void MajorSuccess(float Posb)
    {

    }
    //特殊点数判定
    public virtual int ExtraValue()
    {
        return 0;
    }
    //关系点数判定
    public int RelationBonus(bool Reverse = false)
    {
        int Value = 0;
        if (Reverse == false && Target != null)
        {
            Relation r = Self.FindRelation(Target);
            if (r.FriendValue == -2)
                Value -= 4;
            else if (r.FriendValue == -1)
                Value -= 2;
            else if (r.FriendValue == 1)
                Value += 1;
            else if (r.FriendValue == 2)
                Value += 3;

            if (r.MasterValue == 1)
                Value += 1;
            else if (r.MasterValue == 2)
                Value += 2;

            if (r.LoveValue == 1)
                Value += 1;
            else if (r.LoveValue == 2)
                Value -= 1;
            else if (r.LoveValue == 3)
                Value += 2;
            else if (r.LoveValue == 4)
                Value += 3;

            if (Self.CurrentClique == Target.CurrentClique)
                Value += 3;
        }
        else if (Reverse == true && Target != null)
        {
            Relation r = Self.FindRelation(Target);
            if (r.FriendValue == -2)
                Value += 4;
            else if (r.FriendValue == -1)
                Value += 2;
            else if (r.FriendValue == 1)
                Value -= 1;
            else if (r.FriendValue == 2)
                Value -= 3;

            if (r.MasterValue == 1)
                Value -= 1;
            else if (r.MasterValue == 2)
                Value -= 2;

            if (r.LoveValue == 1)
                Value -= 1;
            else if (r.LoveValue == 2)
                Value += 1;
            else if (r.LoveValue == 3)
                Value -= 2;
            else if (r.LoveValue == 4)
                Value += 1;
        }
        return Value;
    }
    public int CEORelationBonus(Employee Emp)
    {
        int Value = 0;
        Relation r = Emp.FindRelation(GC.CurrentEmployees[0]);
        if (r.FriendValue == -2)
            Value -= 4;
        else if (r.FriendValue == -1)
            Value -= 2;
        else if (r.FriendValue == 1)
            Value += 1;
        else if (r.FriendValue == 2)
            Value += 3;

        if (r.MasterValue == 1)
            Value += 1;
        else if (r.MasterValue == 2)
            Value += 2;

        if (r.LoveValue == 1)
            Value += 1;
        else if (r.LoveValue == 2)
            Value -= 1;
        else if (r.LoveValue == 3)
            Value += 2;
        else if (r.LoveValue == 4)
            Value += 3;

        if (Self.CurrentClique == Target.CurrentClique)
            Value += 3;

        return Value;
    }
    //文化信仰点数判定
    public int CRBonus(bool Reverse = false)
    {
        int Value = 0;
        if (Reverse == false && Target != null)
        {
            if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == 1)
                Value += 1;
            else if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == -1)
                Value -= 2;

            if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == 1)
                Value += 1;
            else if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == -1)
                Value -= 2;
        }
        else if (Reverse == true && Target != null)
        {
            if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == 1)
                Value -= 1;
            else if (Self.CharacterTendency[0] * Target.CharacterTendency[0] == -1)
                Value += 2;

            if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == 1)
                Value -= 1;
            else if (Self.CharacterTendency[1] * Target.CharacterTendency[1] == -1)
                Value += 2;
        }
        return Value;
    }
    //士气点数判定
    public int MoraleBonus(int BonusType = 1)
    {
        int Value = 0;
        if (BonusType == 1)
        {
            if (GC.Morale > 80)
                Value += 3;
            else if (GC.Morale > 60)
                Value += 1;
            else if (GC.Morale < 40)
                Value -= 2;
            else if (GC.Morale < 20)
                Value -= 4;
        }
        else if (BonusType == 2)
        {
            if (GC.Morale > 80)
                Value -= 3;
            else if (GC.Morale > 60)
                Value -= 1;
            else if (GC.Morale < 40)
                Value += 1;
            else if (GC.Morale < 20)
                Value += 3;
        }
        else if (BonusType == 3)
        {
            if (GC.Morale > 80)
                Value += 1;
            else if (GC.Morale < 40)
                Value -= 2;
        }
        return Value;
    }

    //干预事件相关
    public virtual void AddSolvableEvent()
    {
        TimeLeft = 6;
        GC.EC.CreateEventInfo(this);
    }
    public virtual string SetSolvableEventText(int type)
    {
        return "";
    }
    public virtual string ConfirmEventSelect(int type)
    {
        return "";
    }

    //添加时间历史（暂时）
    public void AddHistory()
    {
        if (Target != null)
        {
            Self.InfoDetail.AddHistory("自己和" + Target.Name + "发生了" + EventName + "事件" + ResultText);
            Target.InfoDetail.AddHistory("参与了" + Self.Name + "的" + EventName + "事件" + ResultText);
        }
        else if (Targets.Count > 0)
        {
            Self.InfoDetail.AddHistory("自己与一群人参与了" + EventName + "事件" + ResultText);
            for (int i = 0; i < Targets.Count; i++)
            {
                Targets[i].InfoDetail.AddHistory("自己与一群人参与了" + EventName + "事件" + ResultText);
            }
        }
        else
            Self.InfoDetail.AddHistory("自己发生了" + EventName + "事件" + ResultText);

    }

    //复制当前类(利用新建基类覆盖 如new Event = Storage[num].Clone())
    public Event Clone()
    {
        return (Event)this.MemberwiseClone();
    }
}
#region 事件版本2

//要求涨工资
public class Event1: Event
{
    public Event1() : base()
    {
        EventName = "要求涨工资";
        BuildingRequire = BuildingType.高管办公室;
        MinFaith = 80;
        MotivationRequire = 1;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentDep != null && Self.CurrentDep.CommandingOffice != null && Self.CurrentDep.CommandingOffice.CurrentManager != null)
        {
            Target = Self.CurrentDep.CommandingOffice.CurrentManager;
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Levels[0] > 20)
            Extra += 2;
        if (Self.Levels[2] > 20)
            Extra += 1;
        if (Self.Levels[3] > 20)
            Extra += 1;
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20，信念-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10，信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += 5;
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.2f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力+5,工资上涨20%";
        GC.CreateMessage(Self.Name + "的工资根据需求上涨了20%");
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += 15;
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.4f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力+15,工资上涨40%";
        GC.CreateMessage(Self.Name + "的工资根据需求上涨了40%");
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//谋求高位
public class Event2 : Event
{
    public Event2() : base()
    {
        EventName = "谋求高位";
        BuildingRequire = BuildingType.CEO办公室;
        MinFaith = 80;
        MotivationRequire = 4;
    }
    public override bool RelationCheck()
    {
        Target = GC.CurrentEmployees[0];
        return true;
    }
    public override bool SpecialCheck()
    {
        if (Self.CurrentOffice != null && (Self.CurrentOffice.building.Type == BuildingType.CEO办公室 || Self.CurrentOffice.building.Type == BuildingType.高管办公室))
            return false;
        else
            return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Levels[2] > 40)
            Extra += 2;
        else if (Self.Levels[2] > 30)
            Extra += 1;
        if (Self.Levels[3] > 20)
            Extra += 2;
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20，信念-10";
        GC.CreateMessage(Self.Name + "谋求高位大失败");
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10，信念-5";
        GC.CreateMessage(Self.Name + "谋求高位失败");
        Self.InfoDetail.Entity.ShowTips(7);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.2f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念+5";
        GC.CreateMessage(Self.Name + "谋求高位成功");
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.ChangeCharacter(4, 10);
        Self.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.4f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "获得状态 启发*1，信念+10";
        GC.CreateMessage(Self.Name + "谋求高位大成功");
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//加入派系
public class Event3 : Event
{
    public Event3() : base()
    {
        EventName = "加入派系";
        BuildingRequire = BuildingType.空;
        MinFaith = 60;
        MotivationRequire = 4;
    }
    public override bool RelationCheck()
    {
        List<Employee> T = new List<Employee>();
        if (Self.CurrentClique != null || Self.isCEO == true)
            return false;
        for(int i = 0; i < Self.Relations.Count; i++)
        {
            if(Self.Relations[i].Target.CurrentClique != null && Self.Relations[i].Target.isCEO == false)
            {
                if(Self.Relations[i].FriendValue > 0 || Self.Relations[i].MasterValue > 0 || Self.Relations[i].LoveValue > 2)
                {

                    T.Add(Self.Relations[i].Target);
                }
            }
        }
        if(T.Count > 0)
        {
            Target = T[Random.Range(0, T.Count)];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Gossip * 0.2);
        Extra += (int)(Self.Convince * 0.1);

        Extra += RelationBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.FindRelation(Target).RPoint -= 30;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "与对方的好感-30，心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, -10);
        Self.CurrentClique = Target.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-10, 加入派系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, -15);
        Self.CurrentClique = Target.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        Self.CurrentClique.Members[0].ChangeRelation(Self, 20);
        Self.ChangeRelation(Self.CurrentClique.Members[0], 20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "派系领袖与自己关系+20，信念-15";
    }
}

//搞破坏
public class Event4 : Event
{
    public Event4() : base()
    {
        EventName = "搞破坏";
        BuildingRequire = BuildingType.空;
        MinFaith = 40;
        MotivationRequire = 5;
        MoralRequire = 1;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique == null || Self.isCEO == true)
            return false;
        List<DepControl> D = new List<DepControl>();
        for(int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            for(int j = 0; j < GC.CurrentDeps[i].CurrentEmps.Count; j++)
            {
                if (GC.CurrentDeps[i].CurrentEmps[j].CurrentClique == Self.CurrentClique)
                    break;
                if (j == GC.CurrentDeps[i].CurrentEmps.Count - 1)
                    D.Add(GC.CurrentDeps[i]);
            }
        }
        //这儿可能会有几个问题 1没部门 2部门没人 3事件执行时目标离开部门
        if (D.Count > 0)
        {
            int DepNum = Random.Range(0, D.Count);
            int EmpNum = Random.Range(0, D[DepNum].CurrentEmps.Count);
            Target = D[DepNum].CurrentEmps[EmpNum];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Strategy * 0.4);
        Extra += (int)(Self.Strength * 0.2);

        Extra += RelationBonus(true) + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null)
            return;
        base.EventFinish();
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.FindRelation(Target).RPoint -= 30;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "与对方的好感-30，心力-20";
        GC.CreateMessage(Self.Name + "在试图搞破坏但是没成功");
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "破坏失败,心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Target.CurrentDep != null)
            Target.CurrentDep.FailCheck(true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "破坏成功，该部门增加1个失误";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Target.CurrentDep != null)
        {
            Target.CurrentDep.FailCheck(true);
            Target.CurrentDep.FailCheck(true);
        }
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "破坏成功，该部门增加2个失误";
    }
}

//罢工
public class Event5 : Event
{
    public Event5() : base()
    {
        EventName = "罢工";
        BuildingRequire = BuildingType.空;
        MinFaith = 40;
        MotivationRequire = 5;
        MoralRequire = 3;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique != null && Self.CurrentClique.Members.Count > 5 && Self.CurrentClique.Members[0] == Self)
            return true;
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null)
            return;
        base.EventFinish();
        for(int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            if (i > 0)
                Self.CurrentClique.Members[i].InfoDetail.AddHistory(ResultText);
        }
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        for(int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 20;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        ResultText += "所有派系成员心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 10;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, -10);
            else
                Self.ChangeCharacter(3, -10);
        }
        ResultText += "所有派系成员心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].InfoDetail.AddPerk(new Perk28(Self.CurrentClique.Members[i]), true);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        GC.CreateMessage(Self.Name + "领导所在派系罢工");
        ResultText += "开展了持续一个月的罢工";
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality += 20;
            Self.CurrentClique.Members[i].InfoDetail.AddPerk(new Perk28(Self.CurrentClique.Members[i]), true);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, 30);
        }
        GC.CreateMessage(Self.Name + "领导所在派系罢工");
        ResultText += "开展了持续一个月的罢工,所有派系成员心力+20";
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//建立派系
public class Event6 : Event
{
    public Event6() : base()
    {
        EventName = "建立派系";
        BuildingRequire = BuildingType.空;
        MinFaith = 60;
        MotivationRequire = 6;
    }
    public override bool RelationCheck()
    {
        List<Employee> T = new List<Employee>();
        if (Self.CurrentClique != null || Self.isCEO == true)
            return false;
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentClique == null && Self.Relations[i].Target.isCEO == false)
            {
                if (Self.Relations[i].FriendValue > 0 || Self.Relations[i].MasterValue > 0 || Self.Relations[i].LoveValue > 2)
                {
                    T.Add(Self.Relations[i].Target);
                }
            }
        }
        if (T.Count > 0)
        {
            Target = T[Random.Range(0, T.Count)];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique != null)
            return;
        base.EventFinish();
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.CurrentClique = new Clique();
        Target.CurrentClique = Self.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        Self.CurrentClique.Members.Add(Target);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        Self.ChangeCharacter(4, -10);
        ResultText += "成立了新派系,信念-10";
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.CurrentClique = new Clique();
        Target.CurrentClique = Self.CurrentClique;
        Self.CurrentClique.Members.Add(Self);
        Self.CurrentClique.Members.Add(Target);
        Target.ChangeRelation(Self, 20);
        Self.ChangeRelation(Target, 20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        Self.ChangeCharacter(4, -15);
        ResultText += "成立了新派系,双方关系+20,信念-15";
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//篡权
public class Event7 : Event
{
    public Event7() : base()
    {
        EventName = "篡权";
        BuildingRequire = BuildingType.空;
        MinFaith = 60;
        MotivationRequire = 6;
        MoralRequire = 1;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique != null && Self.CurrentClique.Members[0] == Self)
        {
            Target = GC.CurrentEmployees[0];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus(true) + CRBonus() + MoraleBonus(2);
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null)
            return;
        base.EventFinish();
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            if (i > 0)
                Self.CurrentClique.Members[i].InfoDetail.AddHistory(ResultText);
        }
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 20;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        ResultText += "所有派系成员心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            Posb = Random.Range(0.0f, 1.0f);
            Self.CurrentClique.Members[i].Mentality -= 10;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, -10);
            else
                Self.ChangeCharacter(3, -10);
        }
        ResultText += "所有派系成员心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            GC.CurrentEmployees[0].Mentality -= 10;
            Posb = Random.Range(0.0f, 1.0f);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, 10);
        }
        GC.CreateMessage(Self.Name + "领导所在派系篡权成功（目前没效果）,CEO心力-10");
        ResultText += "成功篡权,CEO心力-10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        for (int i = 0; i < Self.CurrentClique.Members.Count; i++)
        {
            GC.CurrentEmployees[0].Mentality -= 20;
            GC.CurrentEmployees[0].InfoDetail.AddPerk(new Perk7(GC.CurrentEmployees[0]), true);
            Posb = Random.Range(0.0f, 1.0f);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, 30);
        }
        GC.CreateMessage(Self.Name + "领导所在派系篡权大成功（目前没效果）,CEO心力-20,获得抑郁状态");
        ResultText += "成功篡权,CEO心力-20,获得抑郁状态";
    }
}

//健身房
public class Event8 : Event
{
    public Event8() : base()
    {
        EventName = "健身房";
        BuildingRequire = BuildingType.健身房;
        MotivationRequire = 2;
    }
    public override bool RelationCheck()
    {
        if(TargetBuilding != null && TargetBuilding.Office != null && TargetBuilding.Office.CurrentManager != null)
        {
            Target = TargetBuilding.Office.CurrentManager;
            return true;
        }
        return false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Stamina < 70)
            return true;
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.Strength > 15)
            Extra += 3;
        else if (Target.Strength > 10)
            Extra += 2;
        else if (Target.Strength > 5)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.InfoDetail.AddPerk(new Perk1(Self), true);
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，获得状态“受伤”，信念-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力下降5点，信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.05f);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "体力回复5%，信念+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.1f);
        Self.InfoDetail.AddPerk(new Perk2(Self), true);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "获得状态“斯巴达”，体力回复10%，信念+10";
    }
}

//9喝咖啡没做

//按摩
public class Event10 : Event
{
    public Event10() : base()
    {
        EventName = "健身房";
        BuildingRequire = BuildingType.按摩房;
        MotivationRequire = 2;
    }
    public override bool RelationCheck()
    {
        if (TargetBuilding != null && TargetBuilding.Office != null && TargetBuilding.Office.CurrentManager != null)
        {
            Target = TargetBuilding.Office.CurrentManager;
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.Strength > 24)
            Extra += 4;
        else if (Target.Strength > 20)
            Extra += 3;
        else if (Target.Strength > 15)
            Extra += 2;
        else if (Target.Strength > 10)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override bool SpecialCheck()
    {
        if (Self.Stamina < 40)
            return true;
        return false;
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，信念-10";
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.1f);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "体力回复10%，信念-5";
        Self.InfoDetail.Entity.ShowTips(7);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.3f);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "体力回复30%，信念+5";
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Stamina += (int)(Self.Stamina * 0.5f);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "体力回复50%，信念+10";
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//倾诉
public class Event11 : Event
{
    public Event11() : base()
    {
        EventName = "倾诉";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 6;
    }
    public override bool RelationCheck()
    {
        List<Employee> T = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue > 0 || Self.Relations[i].MasterValue > 0 || Self.Relations[i].LoveValue > 2)
            {
                T.Add(Self.Relations[i].Target);
            }
        }
        if (T.Count > 0)
        {
            Target = T[Random.Range(0, T.Count)];
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.HR > 15)
            Extra += 3;
        else if (Target.HR > 10)
            Extra += 2;
        else if (Target.HR > 5)
            Extra += 1;

        if (Self.Tenacity > 20)
            Extra += 2;
        else if (Self.Tenacity > 10)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，单方面好感-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Self.ChangeRelation(Target, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力下降5点，单方面好感-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.05f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力回复5%";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.1f);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力回复10%";
    }
}

//寻找HR沟通
public class Event12 : Event
{
    public Event12() : base()
    {
        EventName = "寻找HR沟通";
        //BuildingRequire = BuildingType.人力资源部B;
        MotivationRequire = 5;
    }
    public override bool BuildingCheck()
    {
        for(int i = 0; i < GC.CurrentDeps.Count; i++)
        {
            if(GC.CurrentDeps[i].type == EmpType.HR && GC.CurrentDeps[i].CurrentEmps.Count > 0)
            {
                if (GC.CurrentDeps[i].CurrentEmps.Count == 1)
                    Target = GC.CurrentDeps[i].CurrentEmps[0];
                else
                {
                    int num = Random.Range(0, 2);
                    Target = GC.CurrentDeps[i].CurrentEmps[num];
                }
            }
        }
        if (Target != null)
            return true;
        return false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality < 40)
            return true;
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Target.HR > 24)
            Extra += 4;
        else if (Target.HR > 20)
            Extra += 3;
        else if (Target.HR > 15)
            Extra += 2;
        else if (Target.HR > 10)
            Extra += 1;

        if (Self.Tenacity > 20)
            Extra += 2;
        else if (Self.Tenacity > 10)
            Extra += 1;

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力下降10点，信念-10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力下降5点，信念-5";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(7);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.1f);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力提升10%，信念+5";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.2f);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力提升20%，信念+10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//13寻找心理咨询师沟通没做

//心力爆炸归零事件1
public class Event14 : Event
{
    public Event14() : base()
    {
        EventName = "心力爆炸归零事件1";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0)
        {
            for(int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
            {
                if(Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 7)
                    return true;
            }
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        string PerkName;
        if (Posb < 0.5f)
        {
            Self.InfoDetail.AddPerk(new Perk8(Self), true);
            PerkName = "重度抑郁症";
        }
        else
        {
            Self.InfoDetail.AddPerk(new Perk6(Self), true);
            PerkName = "欧洲人";
        }
        ResultText += "获得特质" + PerkName + "，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk9(Self), true);

        ResultText += "获得特质“元气满满”，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//心力爆炸归零事件2
public class Event15 : Event
{
    public Event15() : base()
    {
        EventName = "心力爆炸归零事件2";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0 && Self.RTarget != null)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk19(Self), true);
        ResultText += "获得特质“反社会人格”，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk25(Self), true);
        ResultText += "获得特质“爱的艺术”，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//心力爆炸归零事件3
public class Event16 : Event
{
    public Event16() : base()
    {
        EventName = "心力爆炸归零事件3";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        HaveTarget = false;
        ReligionRequire = 2;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk10(Self), true);
        ResultText += "获得特质“狂热”，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk26(Self), true);
        ResultText += "获得特质“悟者”，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//心力爆炸归零事件4
public class Event17 : Event
{
    public Event17() : base()
    {
        EventName = "心力爆炸归零事件4";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        HaveTarget = false;
    }
    public override bool SpecialCheck()
    {
        if (Self.Mentality <= 0 && Self.CheckMotivation(3) >= 20)
        {
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 15)
            Extra += 2;
        else if (Self.Tenacity > 5)
            Extra += 1;

        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            Extra += 1;
        Extra += MoraleBonus(3) + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();

        if (value <= 9)
        {
            ResultText = "大失败";
            return 1;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk10(Self), true);
        ResultText += "获得特质“复仇者”，心力回复50%，信念-10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += (int)(Self.Mentality * 0.5f);
        Self.ChangeCharacter(4, 10);
        string PerkName;
        if (Posb < 0.5f)
        {
            Self.InfoDetail.AddPerk(new Perk12(Self), true);
            PerkName = "鹰视狼顾";
        }
        else
        {
            Self.InfoDetail.AddPerk(new Perk13(Self), true);
            PerkName = "平凡之路";
        }
        ResultText += "获得特质" + PerkName + "，心力回复50%，信念+10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//派系交谈
public class Event18 : Event
{
    public Event18() : base()
    {
        EventName = "派系交谈";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 2;
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentClique != null && Self.CurrentClique.Members.Count > 1)
        {
            Target = Self.CurrentClique.Members[Random.Range(0, Self.CurrentClique.Members.Count)];
            while(Target == Self)
            {
                Target = Self.CurrentClique.Members[Random.Range(0, Self.CurrentClique.Members.Count)];
            }
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Gossip * 0.4);
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Strategy * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(2);
        Extra -= 3;//去除同派系的加成
        return Extra;
    }
    public override void EventFinish()
    {
        if (Self.CurrentClique == null || Target.CurrentClique == null || Self.CurrentClique != Target.CurrentClique)
            return;
        base.EventFinish();
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方好感下降20点，信念+10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感下降10点，信念+5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 10);
        Target.ChangeRelation(Self, 10);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升10点，信念+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 20);
        Target.ChangeRelation(Self, 20);
        Self.ChangeCharacter(4, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方好感上升20点，信念+10";
    }
}

//理念交谈!!!!!!!!!!!!!!!!!!!!!!!
public class Event19 : Event
{
    public Event19() : base()
    {
        EventName = "理念交谈";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        MoralRequire = 3;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.4);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        return true;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeCharacter(2, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方绝对律令倾向小幅增加（+10）";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeCharacter(2, 10);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方绝对律令倾向大幅增加（+30），信念+5";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "感到闷闷不乐，仿佛头顶聚集着一大片积雨云，直到在走廊上碰到你时，对方仍然在唉声叹气。“难道没有人在乎穷人了吗？”于是" +
              Self.Name + "希望你来主持公道。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,绝对律令)成功率:" + Posb + "%"; 
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,功利主义)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }

    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(2, 10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在绝对律令倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在绝对律令倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO在绝对律令倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(2, -10);
                Self.ChangeRelation(CEO, -10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在功利主义倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在功利主义倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO在功利主义倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.Mentality -= 15;
            Self.ChangeCharacter(2, -30);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，单方面心力下降15点，功利主义倾向大幅增加（+30）";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，单方面心力下降15点，功利主义倾向大幅增加（+30）";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.Mentality -= 5;
            Self.ChangeCharacter(2, -10);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "单方面心力下降5点，功利主义倾向小幅增加（+10）";
            AddHistory();
            return "(失败)" + Self.Name + "单方面心力下降5点，功利主义倾向小幅增加（+10）";
        }
    }

}

//友善交谈1
public class Event20 : Event
{
    public Event20() : base()
    {
        EventName = "友善交谈1";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        MoralRequire = 2;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.4);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, -5);
        Self.Mentality -= 15;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念-5，双方好感下降20点，单方面心力下降15点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感下降10点，单方面心力下降5点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Target.ChangeRelation(Self, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升了5点";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感上升了15点";
    }
}

//友善交谈2!!!!!!!!!!!!!!!!!!!!!
public class Event21 : Event
{
    public Event21() : base()
    {
        EventName = "友善交谈2";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        MoralRequire = 2;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Gossip * 0.4);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 10);
        Target.ChangeRelation(Self, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升了10点";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 20);
        Target.ChangeRelation(Self, 20);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感上升了20点";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Target.Name + "向你举报" + Self.Name + "无事生非，在办公室里传播八卦，抹黑CEO，但是被自己强烈谴责，所以向CEO说明情况";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Gossip * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(八卦)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Convince * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }

    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            value += (int)(CEO.Gossip * 0.2f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            value += (int)(CEO.Convince * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                Self.ChangeRelation(CEO, -10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.ChangeRelation(Target, -20);
            Target.ChangeRelation(Self, -20);
            Self.ChangeCharacter(4, -5);
            Self.Mentality -= 15;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，双方好感下降20点，单方面心力下降15点";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，双方好感下降20点，单方面心力下降15点";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Self.ChangeRelation(Target, -10);
            Target.ChangeRelation(Self, -10);
            Self.Mentality -= 5;
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "双方好感下降10点，单方面心力下降5点";
            AddHistory();
            return "(失败)" + Self.Name + "双方好感下降10点，单方面心力下降5点";
        }
    }
}

//友善交谈3
public class Event22 : Event
{
    public Event22() : base()
    {
        EventName = "友善交谈3";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
        MoralRequire = 2;
    }
    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for(int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 30)
                PotentialTargets.Add(Self.Relations[i].Target);
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.4);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeRelation(Target, -25);
        Target.ChangeRelation(Self, -25);
        Self.ChangeCharacter(4, -5);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念-5，双方好感下降25点，单方面心力下降20点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -15);
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感下降15点，单方面心力下降10点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 15);
        Target.ChangeRelation(Self, 15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感上升了15点";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, 25);
        Target.ChangeRelation(Self, 25);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感上升了25点";
    }
}

//炫耀交谈
public class Event23 : Event
{
    public Event23() : base()
    {
        EventName = "炫耀交谈";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        MoralRequire = 1;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return -10;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeCharacter(2, -10);
        Target.ChangeRelation(Self, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方功利主义倾向小幅增加（+10）单方面好感+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeCharacter(2, -30);
        Target.ChangeRelation(Self, 20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5 对方功利主义倾向小幅增加（+30）单方面好感+20";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "到处转来转去焦躁不安，就像是一个待喷发的火山。一边转悠一边还嘟囔着“现在的年轻人太不像话了！”之类的，" +
                "被烦扰的同事偷偷向你报告，希望你能解决一下。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,功利主义)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,绝对律令)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }
    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[2] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(2, -10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在功利主义倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在功利主义倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO在绝对律令倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[2] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(2, 10);
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO在绝对律令倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO在绝对律令倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO在绝对律令倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 15;
            Self.ChangeCharacter(2, 30);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，对方心力下降15点，绝对律令倾向大幅增加（+30）";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，对方心力下降15点，绝对律令倾向大幅增加（+30）";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 5;
            Self.ChangeCharacter(2, 10);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "对方心力下降5点，绝对律令倾向小幅增加（+10）";
            AddHistory();
            return "(失败)" + Self.Name + "对方心力下降5点，绝对律令倾向小幅增加（+10）";
        }
    }
}

//道德思考
public class Event24 : Event
{
    public Event24() : base()
    {
        EventName = "道德思考";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 4;
        HaveTarget = false;
        MoralRequire = 2;
    }
    public override int ExtraValue()
    {
        return 0;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(2, -30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "功利主义倾向 大幅增加（+30）";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(2, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "功利主义倾向 小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(2, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "绝对律令倾向 小幅增加（+10）";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(2, 30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "绝对律令倾向 大幅增加（+30）";
    }
}

//交流人的价值
public class Event25 : Event
{
    public Event25() : base()
    {
        EventName = "交流人的价值";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        ReligionRequire = 3;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 15;
        Self.ChangeCharacter(1, -30);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方心力下降15点，机械智能信仰大幅增加（+30），信念-5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Self.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方心力下降5点，机械智能信仰小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方人文主义信仰小幅增加（+10）单方面好感+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 20);
        Target.ChangeCharacter(1, 30);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方人文主义信仰小幅增加（+30）单方面好感+20，信念+5";
    }
}

//交流机械的价值
public class Event26 : Event
{
    public Event26() : base()
    {
        EventName = "交流机械的价值";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        ReligionRequire = 1;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 15;
        Self.ChangeCharacter(1, 30);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方心力下降15点，人文主义信仰大幅增加（+30），信念-5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Self.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方心力下降5点，人文主义信仰小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方机械智能信仰小幅增加（+10）单方面好感+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 20);
        Target.ChangeCharacter(1, -30);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方机械智能信仰小幅增加（+30）单方面好感+20，信念+5";
    }
}

//信仰怀疑
public class Event27 : Event
{
    public Event27() : base()
    {
        EventName = "信仰怀疑";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 4;
        HaveTarget = false;
        ReligionRequire = 2;
    }
    public override int ExtraValue()
    {
        return 0;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(1, -30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "机械智能信仰 大幅增加（+30）";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "机械智能信仰 小幅增加（+10）";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(1, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "人文主义信仰 小幅增加（+10）";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(1, 30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "人文主义信仰 大幅增加（+30）";
    }
}

//狂热传教1!!!!!!!!!!!!!!!!!!!!!!
public class Event28 : Event
{
    public Event28() : base()
    {
        EventName = "狂热传教1";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
        ReligionRequire = 1;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 11)
                return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Target.ChangeCharacter(1, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方机械智能信仰小幅增加（+10）单方面好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, -30);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方机械智能信仰大幅增加（+30）单方面好感+10，信念+5";
    }
    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "还在四处转悠寻找下一个猎物，“愚蠢！”“迷失的人类！”，" + Self.Name + "一边低声重复着一边抚摸手里的微型机器人，" +
                "不知不觉便走到了你面前，向你抱怨那些拒绝传教的“迷途羔羊”。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,机械飞升)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,人文主义)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }
    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(1, -10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO机械飞升倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO机械飞升倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO机械飞升倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(1, 10);
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO人文主义倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO人文主义倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO人文主义倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 15;
            Target.ChangeRelation(Self, -15);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，对方心力-15  对方单方面好感-15";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，对方心力-15  对方单方面好感-15";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 5;
            Target.ChangeRelation(Self, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "对方心力-5 对方单方面好感-5";
            AddHistory();
            return "(失败)" + Self.Name + "对方心力-5 对方单方面好感-5";
        }
    }
}

//狂热传教2 重点再查一下!!!!!!!!!!!
public class Event29 : Event
{
    public Event29() : base()
    {
        EventName = "狂热传教2";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
        ReligionRequire = 3;
    }
    public override bool SpecialCheck()
    {
        if (Self == GC.CurrentEmployees[0] || Target == GC.CurrentEmployees[0])
            return false;
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 11)
                return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        FailureNum = 1;
        AddSolvableEvent();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        FailureNum = 2;
        AddSolvableEvent();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Target.ChangeCharacter(1, 10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "对方人文主义信仰倾向小幅增加（+10%） 单方面好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.ChangeCharacter(1, 30);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "对方人文主义信仰大幅增加（+30）单方面好感+10";
    }

    public override string SetSolvableEventText(int type)
    {
        string Content = "";
        Employee CEO = GC.CurrentEmployees[0];
        if (type == 0)
            Content = Self.Name + "两个人的争吵让旁边的同事苦不堪言，虽然" +  Target.Name + "除了“松手！”之外什么也没说。" +
                "你抵达现场时"+ Self.Name + "还在死命拽着" + Target.Name + "不撒手，没完没了地说着什么“人类是永远不会消失的" +
                "除非所有人都信什么机械飞升”之类的话，" + Target.Name + "向你投来了求助的眼神。";
        else if (type == 1)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Self.Name + "(说服,人文主义)成功率:" + Posb + "%";
        }
        else if (type == 2)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "支持" + Target.Name + "(说服,机械飞升)成功率:" + Posb + "%";
        }
        else if (type == 3)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.Strategy * 0.2f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "激化矛盾" + "(谋略)成功率:" + Posb + "%";
        }
        else if (type == 4)
        {
            int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
            value += (int)(CEO.HR * 0.1f);
            int Posb = (int)((4 + value) / 11.0f * 100);
            if (Posb > 100)
                Posb = 100;
            Content = "调停矛盾" + "(人力)成功率:" + Posb + "%";
        }
        return Content;
    }

    public override string ConfirmEventSelect(int type)
    {
        Employee CEO = GC.CurrentEmployees[0];
        int value = (CEORelationBonus(Self) + CEORelationBonus(Target)) / 2;
        int RValue = Random.Range(2, 12);
        if (type == 1)
        {
            if (CEO.Character[1] > 50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, 5);
                Self.Mentality += 10;
                CEO.ChangeCharacter(1, 10);
                Target.ChangeRelation(CEO, -10);
                Target.ChangeCharacter(4, -5);
                Target.Mentality -= 10;
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO人文主义倾向+10");
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO人文主义倾向+10");
                return "(成功) " + Self.Name + "对CEO好感度+10，" + Self.Name + "信念+5 " + Self.Name + "心力+10，CEO人文主义倾向+10，"
                    + Target.Name + "对CEO好感度-10，" + Target.Name + "信念-5，" + Target.Name + "心力-10";
            }
        }
        else if (type == 2)
        {
            if (CEO.Character[1] < -50)
                value += 2;
            value += (int)(CEO.Convince * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(CEO, 10);
                Target.ChangeCharacter(4, 5);
                Target.Mentality += 10;
                CEO.ChangeCharacter(1, -10);
                Self.ChangeRelation(CEO, 10);
                Self.ChangeCharacter(4, -5);
                Self.Mentality -= 10;
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO支持,对CEO好感度+10，信念+5，心力+10，CEO机械飞升倾向+10");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO反对,对CEO好感度-10，信念-5，心力-10，CEO机械飞升倾向+10");
                return "(成功) " + Target.Name + "对CEO好感度+10，" + Target.Name + "信念+5 " + Target.Name + "心力+10，CEO机械飞升倾向+10，"
                    + Self.Name + "对CEO好感度-10，" + Self.Name + "信念-5，" + Self.Name + "心力-10";
            }
        }
        else if (type == 3)
        {
            value += (int)(CEO.Strategy * 0.2f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, -20);
                Self.ChangeRelation(Target, -20);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO挑拨,双方之间好感度-20");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度-20";
            }
        }
        else if (type == 4)
        {
            value += (int)(CEO.HR * 0.1f);
            if (RValue + value > 8)
            {
                Target.ChangeRelation(Self, 10);
                Target.ChangeRelation(CEO, 5);
                Self.ChangeRelation(Target, 10);
                Self.ChangeRelation(CEO, 5);
                Target.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                Self.InfoDetail.AddHistory("在" + EventName + "事件中受到CEO调停,双方之间好感度+10，对CEO好感度+5");
                return "(成功) " + Target.Name + "与" + Self.Name + "双方之间好感度+10，" + Target.Name + "对CEO好感度+5，" + Self.Name + "对CEO好感度+5";
            }
        }

        if (FailureNum == 1)
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 15;
            Target.ChangeRelation(Self, -15);
            Self.ChangeCharacter(4, -5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 30);
            else
                Self.ChangeCharacter(3, -30);
            ResultText += "信念-5，对方心力-15  对方单方面好感-15";
            AddHistory();
            return "(失败)" + Self.Name + "信念-5，对方心力-15  对方单方面好感-15";
        }
        else
        {
            float Posb = Random.Range(0.0f, 1.0f);
            Target.Mentality -= 5;
            Target.ChangeRelation(Self, -5);
            Self.ChangeCharacter(4, 5);
            if (Posb < 0.5f)
                Self.ChangeCharacter(0, 10);
            else
                Self.ChangeCharacter(3, -10);
            ResultText += "信念+5，对方心力-5 对方单方面好感-5";
            AddHistory();
            return "(失败)" + Self.Name + "信念+5，对方心力-5 对方单方面好感-5";
        }
    }
}

//恋爱狂热
public class Event30 : Event
{
    public Event30() : base()
    {
        EventName = "恋爱狂热";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 2;
    }

    public override bool RelationCheck()
    {
        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 3)
        {
            Target = Self.Lover;
            return true;
        }
        return false;
    }

    public override bool SpecialCheck()
    {
        for (int i = 0; i < Self.InfoDetail.PerksInfo.Count; i++)
        {
            if (Self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 24)
                return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 10;
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方好感-15，双方心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Target.ChangeRelation(Self, -5);
        Self.Mentality -= 5;
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感-5，双方心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.Mentality += 5;
        Target.ChangeRelation(Self, 5);
        Self.Mentality += 5;
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感+5，双方心力+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.Mentality += 10;
        Target.ChangeRelation(Self, 15);
        Self.Mentality += 10;
        Self.ChangeRelation(Target, 15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方好感+15，双方心力+10";
    }
}

//挚友搞基互动
public class Event31 : Event
{
    public Event31() : base()
    {
        EventName = "挚友搞基互动";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 2;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue == 2)
                PotentialTargets.Add(Self.Relations[i].Target);
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.Mentality -= 10;
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方好感-15，双方心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.Mentality -= 5;
        Target.ChangeRelation(Self, -5);
        Self.Mentality -= 5;
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方好感-5，双方心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.Mentality += 5;
        Target.ChangeRelation(Self, 5);
        Self.Mentality += 5;
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感+5，双方心力+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.Mentality += 10;
        Target.ChangeRelation(Self, 15);
        Self.Mentality += 10;
        Self.ChangeRelation(Target, 15);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方好感+15，双方心力+10";
    }
}

//潜在发展对象
public class Event32 : Event
{
    public Event32() : base()
    {
        EventName = "潜在发展对象";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].LoveValue == 1 || Self.Relations[i].Target == Self.RTarget)
                PotentialTargets.Add(Self.Relations[i].Target);
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);
        Extra += (int)(Self.Convince * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -15);
        Self.Mentality -= 10;
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念-5，对方好感-15，心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Self.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方好感-5，心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.Mentality += 5;
        Target.ChangeRelation(Self, 15);
        Self.Mentality += 5;
        Self.ChangeRelation(Target, 15);
        Self.ChangeCharacter(4, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，双方好感+15，双方心力+5";
    }
}

//拜师  技能加成少了一部分 而且特质里没有师承授业这个东西
public class Event33 : Event
{
    public Event33() : base()
    {
        EventName = "拜师";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Master != null)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue < 1 && Self.Relations[i].LoveValue < 3 && Self.Relations[i].MasterValue == 0 && Self.Relations[i].RPoint > 50)
            {
                Employee t = Self.Relations[i].Target;
                if(t.Skill1 > 15 || t.Skill2 > 15 || t.Skill3 > 15 || t.Observation > 15 || t.Tenacity > 15 || t.Strength > 15 || t.Manage > 15 ||
                    t.HR > 15 || t.Finance > 15 || t.Decision > 15 || t.Forecast > 15 || t.Strategy > 15 || t.Convince > 15 || t.Charm > 15 || t.Gossip > 15)
                {
                    for(int j = 0; j < 5; i++)
                    {
                        if (t.Levels[j] < Self.Levels[j])
                            break;
                        else if (j == 4)
                            PotentialTargets.Add(t);
                    }
                }
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.Mentality -= 30;
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-30，信念-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.Mentality -= 10;
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);

        ResultText += "心力-10，信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.Stamina -= 30;
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Master = Target;
        Self.FindRelation(Target).MasterValue = 2;
        Target.FindRelation(Self).MasterValue = 1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念+5，获得特质师承授业(没有这个特质)，体力-30," + Self.Name + "师承" + Target.Name;
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Master = Target;
        Relation r = Self.FindRelation(Target);
        r.MasterValue = 2;
        r.RPoint += 10;

        r = Target.FindRelation(Self);
        r.MasterValue = 1;
        r.RPoint += 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+10，获得特质师承授业(没有这个特质)，双方好感+10," + Self.Name + "师承" + Target.Name;
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//邂逅变为追求或倾慕
public class Event34 : Event
{
    public Event34() : base()
    {
        EventName = "邂逅变为追求或倾慕";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 1;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].FriendValue < 1 && Self.Relations[i].LoveValue == 0 && Self.Relations[i].MasterValue == 0)
            {
                Employee T = Self.Relations[i].Target;
                if (T == null || T.CurrentDep != Self.CurrentDep)
                {
                    if (T.Charm > 10 || T.Levels[0] > 30 || T.Levels[1] > 20 || T.Levels[2] > 20 || T.Levels[3] > 20 || T.Levels[4] > 20)
                        PotentialTargets.Add(T);
                }
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.RPoint -= 30;
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-10，单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name + "单方面好感度-30";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.RPoint -= 10;
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name + "单方面好感度-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name;
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Relation r = Self.FindRelation(Target);
        r.RPoint += 50;
        r.LoveValue = 1;

        r = Target.FindRelation(Self);
        r.RPoint += 20;
        r.LoveValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "单方面好感+50，" + Self.Name + "倾慕" + Target.Name + "," + Target.Name + "获得追求者"
            + Self.Name + "单方面好感度+20";
    }
}

//情侣
public class Event35 : Event
{
    public Event35() : base()
    {
        EventName = "情侣";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Lover != null)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 50 && Self.Relations[i].LoveValue < 3)
            {
                if (Self.Relations[i].Target.FindRelation(Self).RPoint > 50)
                    PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);        
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 5;
        Target.Mentality += 5;
        Self.FindRelation(Target).LoveValue = 3;
        Target.FindRelation(Self).LoveValue = 3;
        Self.Lover = Target;
        Target.Lover = Self;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+5，彼此结成情侣关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Target.Mentality += 10;
        Self.FindRelation(Target).LoveValue = 3;
        Target.FindRelation(Self).LoveValue = 3;
        Self.Lover = Target;
        Target.Lover = Self;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+10，彼此结成情侣关系";
    }
}

//伴侣
public class Event36 : Event
{
    public Event36() : base()
    {
        EventName = "伴侣";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Lover == null)
            return false;
        if (Self.Lover != null && Self.FindRelation(Self.Lover).LoveValue == 4)
            return false;
        if (Self.FindRelation(Self.Lover).RPoint > 80 && Self.Lover.FindRelation(Self).RPoint > 80)
        {
            Target = Self.Lover;
            return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(8);
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "无事发生";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(7);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 5;
        Target.Mentality += 5;
        Self.FindRelation(Target).LoveValue = 4;
        Target.FindRelation(Self).LoveValue = 4;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+5，彼此结成伴侣关系";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Target.Mentality += 10;
        Self.FindRelation(Target).LoveValue = 4;
        Target.FindRelation(Self).LoveValue = 4;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+10，彼此结成伴侣关系";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//朋友
public class Event37 : Event
{
    public Event37() : base()
    {
        EventName = "朋友";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 50 && Self.Relations[i].LoveValue < 3 && Self.Relations[i].FriendValue < 1)
            {
                if (Self.Relations[i].Target.FindRelation(Self).RPoint > 50)
                    PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 5;
        Self.ChangeCharacter(4, 5);
        Target.Mentality += 5;
        Target.ChangeCharacter(4, 5);
        Self.FindRelation(Target).FriendValue = 1;
        Target.FindRelation(Self).FriendValue = 1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+5，双方信念+5，彼此结成朋友关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Self.ChangeCharacter(4, 10);
        Target.Mentality += 10;
        Target.ChangeCharacter(4, 10);
        Self.FindRelation(Target).FriendValue = 1;
        Target.FindRelation(Self).FriendValue = 1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+10，双方信念+10，彼此结成朋友关系";
    }
}

//挚友
public class Event38 : Event
{
    public Event38() : base()
    {
        EventName = "挚友";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint > 80 && Self.Relations[i].FriendValue == 1)
            {
                if (Self.Relations[i].Target.FindRelation(Self).RPoint > 80)
                    PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "对方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.RTarget = Target;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 10;
        Self.ChangeCharacter(4, 5);
        Target.Mentality += 10;
        Target.ChangeCharacter(4, 5);
        Self.FindRelation(Target).FriendValue = 2;
        Target.FindRelation(Self).FriendValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方心力+10，双方信念+5，彼此结成挚友关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Self.RTarget == Target)
            Self.RTarget = null;
        Self.Mentality += 20;
        Self.ChangeCharacter(4, 10);
        Target.Mentality += 20;
        Target.ChangeCharacter(4, 10);
        Self.FindRelation(Target).FriendValue = 2;
        Target.FindRelation(Self).FriendValue = 2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方心力+20，双方信念+10，彼此结成挚友关系";
    }
}

//陌路
public class Event39 : Event
{
    public Event39() : base()
    {
        EventName = "陌路";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint < 0)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus(true) + MoraleBonus(2);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "单方面心力-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "单方面心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.ChangeCharacter(4, -5);
        Target.ChangeCharacter(4, -5);
        Self.FindRelation(Target).FriendValue = -1;
        Target.FindRelation(Self).FriendValue = -1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "彼此结成陌路关系，双方关系-10，双方信念-5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, -10);
        Target.ChangeCharacter(4, -10);
        Self.FindRelation(Target).FriendValue = -1;
        Target.FindRelation(Self).FriendValue = -1;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "彼此结成陌路关系，双方关系-20，双方信念-10";
    }
}

//仇敌
public class Event40 : Event
{
    public Event40() : base()
    {
        EventName = "仇敌";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint < -40)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += CRBonus(true) + MoraleBonus(2);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 10;
        Target.Mentality += 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "单方面心力-10，对方心力+5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 5;
        Target.Mentality -= 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "双方心力-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        Self.ChangeCharacter(4, -5);
        Target.ChangeCharacter(4, -5);
        Self.FindRelation(Target).FriendValue = -2;
        Target.FindRelation(Self).FriendValue = -2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "彼此结成仇敌关系，双方关系-10，双方信念-5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeRelation(Target, -20);
        Target.ChangeRelation(Self, -20);
        Self.ChangeCharacter(4, -10);
        Target.ChangeCharacter(4, -10);
        Self.FindRelation(Target).FriendValue = -2;
        Target.FindRelation(Self).FriendValue = -2;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "彼此结成仇敌关系，双方关系-20，双方信念-10";
    }
}

//解除关系
public class Event41 : Event
{
    public Event41() : base()
    {
        EventName = "解除关系";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 5;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].RPoint < 20 && (Self.Relations[i].FriendValue > 0 || Self.Relations[i].LoveValue > 2 || Self.Relations[i].MasterValue > 0))
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;

        Extra += (int)(Self.Charm * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += (int)(Self.Observation * 0.2);
        Extra += RelationBonus() + CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -5);
        Target.ChangeCharacter(4, -5);

        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "双方信念-5，双方关系下降10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "对方关系下降5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Relation r = Self.FindRelation(Target);
        if (r.LoveValue > 2)
        {
            r.LoveValue = 0;
            r = Target.FindRelation(Self);
            r.LoveValue = 0;
            Self.Lover = null;
            Target.Lover = null;
        }
        else if (r.MasterValue > 0)
        {
            r.MasterValue = 0;
            r = Target.FindRelation(Self);
            r.MasterValue = 0;
            if (Self.Master == Target)
                Self.Master = null;
            else if (Target.Master == Self)
                Target.Master = null;
        }
        else if (r.FriendValue > 0)
        {
            r.FriendValue = 0;
            r = Target.FindRelation(Self);
            r.FriendValue = 0;
        }

        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "双方解除了一个亲密关系";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 5);
        Target.ChangeCharacter(4, 5);
        Self.Mentality += 10;
        Target.Mentality += 10;
        Relation r = Self.FindRelation(Target);

        if (r.LoveValue > 2)
        {
            r.LoveValue = 0;
            r = Target.FindRelation(Self);
            r.LoveValue = 0;
            Self.Lover = null;
            Target.Lover = null;
        }
        else if (r.MasterValue > 0)
        {
            r.MasterValue = 0;
            r = Target.FindRelation(Self);
            r.MasterValue = 0;
            if (Self.Master == Target)
                Self.Master = null;
            else if (Target.Master == Self)
                Target.Master = null;
        }
        else if (r.FriendValue > 0)
        {
            r.FriendValue = 0;
            r = Target.FindRelation(Self);
            r.FriendValue = 0;
        }

        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "双方解除了一个亲密关系，双方信念+5，双方心力+10";
    }
}

//离职
public class Event42 : Event
{
    public Event42() : base()
    {
        EventName = "离职";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
        HaveTarget = false;
        MinFaith = 20;
    }

    public override void EventFinish()
    {
        //再检测一下事件是否还有效
        if (HaveTarget == true && Target == null)
        {
            return;
        }
        int result = FindResult();
        float Posb = Random.Range(0.0f, 1.0f);
        if (result == 1)
            MajorFailure(Posb);
        else if (result == 2)
            Failure(Posb);
        else if (result == 3)
            Success(Posb);
        else if (result == 4)
            MajorSuccess(Posb);
    }

    public override bool SpecialCheck()
    {
        if (Self.CurrentClique != null)
            return false;
        return true;
    }

    public override int ExtraValue()
    {
        int Extra = MoraleBonus(2);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        ResultText += "从公司离职";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Fire();
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        ResultText += "从公司离职";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Fire();
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "因为某些原因，决定再干一段时间";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "因为某些原因，决定再干一段时间";
    }
}

//寻求建议
public class Event43 : Event
{
    public Event43() : base()
    {
        EventName = "寻求建议";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 1;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentDep != null && Self.Relations[i].Target.CurrentDep.type != EmpType.HR)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        TargetBuilding = Target.CurrentDep.building;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.HR * 0.2);
        Extra += (int)(Target.Forecast * 0.2);
        Extra += (int)(Target.Observation * 0.2);

        if (Self.Levels[0] > 45)
            Extra += 2;
        else if (Self.Levels[1] > 30)
            Extra += 1;

        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5，单方面好感-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念+5，获得 状态 启发";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念+10，获得 状态 启发*2";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//交流工作
public class Event44 : Event
{
    public Event44() : base()
    {
        EventName = "交流工作";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 4;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentDep != null && Self.Relations[i].Target.CurrentDep.type != EmpType.HR)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        TargetBuilding = Target.CurrentDep.building;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.HR * 0.1);
        Extra += (int)(Target.Forecast * 0.1);
        Extra += (int)(Target.Observation * 0.1);

        Extra += (int)(Self.HR * 0.1);
        Extra += (int)(Self.Forecast * 0.1);
        Extra += (int)(Self.Observation * 0.1);

        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.ChangeRelation(Target, -5);
        Target.ChangeRelation(Self, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5，双方好感-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Target.ChangeCharacter(4, 5);
        Target.InfoDetail.AddPerk(new Perk3(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "信念+5，双方获得 状态 启发";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Target), true);
        Target.ChangeCharacter(4, 10);
        Target.InfoDetail.AddPerk(new Perk3(Target), true);
        Target.InfoDetail.AddPerk(new Perk3(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        GC.CreateMessage(ResultText);
        ResultText += "信念+10，双方获得 状态 启发*2";
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//头脑风暴
public class Event45 : Event
{
    public Event45() : base()
    {
        EventName = "头脑风暴";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 6;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0)
            return false;

        List<Employee> PotentialTargets = new List<Employee>();
        for (int i = 0; i < Self.Relations.Count; i++)
        {
            if (Self.Relations[i].Target.CurrentDep != null && Self.Relations[i].Target.CurrentDep.type != EmpType.HR)
            {
                PotentialTargets.Add(Self.Relations[i].Target);
            }
        }

        if (PotentialTargets.Count == 0)
            return false;

        Target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
        TargetBuilding = Target.CurrentDep.building;
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.Forecast * 0.1);
        Extra += (int)(Target.Observation * 0.1);

        Extra += (int)(Self.Forecast * 0.1);
        Extra += (int)(Self.Observation * 0.1);

        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.InfoDetail.AddPerk(new Perk7(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，获得“抑郁”状态";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        Self.ChangeRelation(Target, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5，单方面好感-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Target.ChangeCharacter(4, 5);
        Target.InfoDetail.AddPerk(new Perk4(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "信念+5，双方获得状态 头脑风暴*1";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(6);
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.ChangeRelation(Target, 10);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Target.ChangeCharacter(4, 10);
        Target.ChangeRelation(Self, 10);
        Target.InfoDetail.AddPerk(new Perk4(Target), true);
        Target.InfoDetail.AddPerk(new Perk4(Target), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念+10，双方获得状态 头脑风暴*2，双方好感+5";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//师承授业
public class Event46 : Event
{
    public Event46() : base()
    {
        EventName = "师承授业";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
    }

    public override bool RelationCheck()
    {
        if (Self.Relations.Count == 0 || Self.Master == null)
            return false;

        if(Self.Master != null)
        {
            if(Self.Master.CurrentDep != null && Self.Master.CurrentDep.type != EmpType.HR)
            {
                Target = Self.Master;
                return true;
            }
            else if (Self.Master.CurrentOffice != null && (Self.Master.CurrentOffice.building.Type == BuildingType.CEO办公室 || Self.Master.CurrentOffice.building.Type == BuildingType.高管办公室))
            {
                Target = Self.Master;
                return true;
            }
        }
        return true;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Target.Forecast * 0.2);

        Extra += (int)(Self.Observation * 0.2);

        Extra += CRBonus() + MoraleBonus(1);
        return Extra;
    }

    public override void EventFinish()
    {
        if (Self.Master == null)
            return;
        base.EventFinish();
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.ChangeCharacter(4, -10);
        Self.ChangeRelation(Target, -10);
        Target.ChangeRelation(Self, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "信念-10，双方好感-10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeCharacter(4, -5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "信念-5";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeCharacter(4, 5);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "信念+5，获得状态 头脑风暴*1 状态 启发*1";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.ChangeCharacter(4, 10);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk4(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        Self.InfoDetail.AddPerk(new Perk3(Self), true);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "信念+10，获得状态 头脑风暴*2 状态 启发*2";
        GC.CreateMessage(ResultText);
        Self.InfoDetail.Entity.ShowTips(5);
    }
}

//摸鱼
public class Event47 : Event
{
    public Event47() : base()
    {
        EventName = "摸鱼";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 3;
    }
    public override int ExtraValue()
    {
        int Extra = 0;

        Extra += (int)(Self.Strategy * 0.2);
        Extra += (int)(Self.Convince * 0.2);
        Extra += CRBonus() + MoraleBonus(2) + RelationBonus(true);
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += 10;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力+10";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += 20;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力+20";
    }
}

//抱怨
public class Event48 : Event
{
    public Event48() : base()
    {
        EventName = "抱怨";
        BuildingRequire = BuildingType.空;
        MotivationRequire = 2;
    }

    public override bool RelationCheck()
    {
        List<Employee> E = new List<Employee>();
        if (Self.CurrentDep == null)
            return false;
        for (int i = 0; i < Self.CurrentDep.CurrentEmps.Count; i++)
        {
            if (Self.CurrentDep.CurrentEmps[i] != Self)
                E.Add(Self.CurrentDep.CurrentEmps[i]);
        }
        if (E.Count > 0)
        {
            Target = E[Random.Range(0, E.Count)];
            return true;
        }
        return false;
    }

    public override int ExtraValue()
    {
        int Extra = 0;
        if (Self.Tenacity > 20)
            Extra += 2;
        else if (Self.Tenacity > 10)
            Extra += 1;
        if (Target.HR > 15)
            Extra += 3;
        else if (Target.HR > 10)
            Extra += 2;
        else if (Target.HR > 5)
            Extra += 1;
        Extra += CRBonus() + MoraleBonus(1) + RelationBonus();
        return Extra;
    }

    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.Mentality -= 20;
        Self.ChangeRelation(Target, -20);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -30);
        else
            Self.ChangeCharacter(3, -30);
        ResultText += "心力-20，单方面好感-20";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.Mentality -= 10;
        Self.ChangeRelation(Target, -10);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, -10);
        else
            Self.ChangeCharacter(3, -10);
        ResultText += "心力-10，单方面好感-10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.Mentality += 2;
        Self.ChangeRelation(Target, 5);
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 10);
        else
            Self.ChangeCharacter(3, 10);
        ResultText += "心力+2，单方面好感+5";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.Mentality += 5;
        if (Posb < 0.5f)
            Self.ChangeCharacter(0, 30);
        else
            Self.ChangeCharacter(3, 30);
        ResultText += "心力+5";
    }
}

#endregion

#region 事件版本3
//打招呼
public class Event2_1 : Event
{
    public Event2_1() : base()
    {
        EventName = "打招呼";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>(){ };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }

    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        ResultText += Self.Name + "与"+Target.Name+"在走廊里打了个招呼，对方获得了浅蓝色情绪";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 10);
        Target.ChangeRelation(Self, 10);
        //flag编号1：前置事件1的标记
        Self.FindRelation(Target).EventFlag[1] = 1;
        ResultText += Self.Name + "与" + Target.Name + "在走廊里打了个招呼，对方获得了浅黄色情绪，好感度上升5点";
    }
}
//解除朋友关系
public class Event2_2 : Event
{
    public Event2_2() : base()
    {
        EventName = "解除朋友关系";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 1;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > 0)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Relation r = Self.FindRelation(Target);

        if (r.MasterValue == 1)
            Extra += 1;
        else if (r.MasterValue == 2)
            Extra += 2;

        if (r.LoveValue == 3)
            Extra += 2;

        if (Self.CurrentClique == Target.CurrentClique)
            Extra += 3;

        Extra += MoraleBonus() + CRBonus();
        return Extra;
    }
    
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }

    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.FindRelation(Target).FriendValue = 0;
        Target.FindRelation(Self).FriendValue = 0;
        ResultText += "朋友关系解除";
        GC.CreateMessage(Self.Name + "完成了事件" + EventName);
        
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Target.ChangeRelation(Self, 5);
        ResultText += "好感度上升5";
    }
}
//提供帮助和鼓励
public class Event2_3 : Event
{
    public Event2_3() : base()
    {
        EventName = "提供帮助和鼓励";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DGreen };
        TargetEmotionRequire = new List<EColor>() { };
        MoralRequire = 0;
        ReligionRequire = 0;
        PerkRequire = 0;
        RelationRequire = 0;
        BuildingRequire = BuildingType.空;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint <= 10)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);
        Relation r = Self.FindRelation(Target);

        if (r.MasterValue == 1)
            Extra += 1;
        else if (r.MasterValue == 2)
            Extra += 2;

        if (r.LoveValue == 3)
            Extra += 2;

        if (Self.CurrentClique == Target.CurrentClique)
            Extra += 3;

        Extra += MoraleBonus() + CRBonus();
        return Extra;
    }

    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }

    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, 10);
        Target.ChangeRelation(Self, 10);
        ResultText += "朋友关系解除";
        ResultText += Self.Name + "与" + Target.Name + "朋友关系解除";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "与" + Target.Name + "尝试解除朋友关系，对方极力挽回，好感度上升5";
    }
}
//尬聊
public class Event2_4 : Event
{
    public Event2_4() : base()
    {
        EventName = "尬聊";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DBlue};
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if(Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 10)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "与" + Target.Name + "尬聊，对方好感度下降5点，对方获得蓝色情绪";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        ResultText += Self.Name + "与" + Target.Name + "尬聊，无事发生";
    }
}

public class Event2_5 : Event
{
    public Event2_5() : base()
    {
        EventName = "分享昨天发生的趣事";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DYellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 10)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "与" + Target.Name + "分享昨天发生的趣事，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "与" + Target.Name + "分享昨天发生的趣事，对方好感度下降5点，对方获得蓝色情绪";
    }
}

public class Event2_6 : Event
{
    public Event2_6() : base()
    {
        EventName = "讨论天气和路况";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        foreach (int i in Self.FindRelation(Target).EventFlag)
        {
            if (i == 1)
            {
                c = true;
            }
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 10)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "与" + Target.Name + "谈论天气和路况，对方获得浅黄色情绪，对方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "与" + Target.Name + "谈论天气和路况，对方获得浅红色情绪";
    }
}
//.
public class Event2_7 : Event
{
    public Event2_7() : base()
    {
        //要求：是自身关系发展目标
        EventName = "赠送礼物给对方";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 10)
            return false;
        if(Self.RelationTargets.Contains(Target) == false)//判断是否为额外发展对象
        {
            //判断是否为同事
            if (Self.CurrentDep != null && Self.CurrentDep.CurrentEmps.Contains(Target) == false)
                return false;
            //判断是否为上司
            if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
                return false;
            if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
                return false;
            if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
                return false;
        }
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        if (Self.CurrentDep != null && Self.CurrentDep.CommandingOffice != null && Self.CurrentDep.CommandingOffice.CurrentManager == Target)
            return true&c;
        else if (Self.CurrentDep != null && Self.CurrentDep.CurrentEmps.Contains(Target))
            return true&c;
        else if (Self.RelationTargets.Contains(Target))
            return true&c;
        return false;
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "赠送礼物给" + Target.Name + "，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "赠送礼物给" + Target.Name + "，对方获得蓝色情绪";
    }
}

public class Event2_8 : Event
{
    public Event2_8() : base()
    {
        EventName = "指出对方缺点";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 10)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "指出了" + Target.Name + "的缺点，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "指出了" + Target.Name + "的缺点，对方获得红色情绪";
    }
}

public class Event2_9 : Event
{
    public Event2_9() : base()
    {
        EventName = "邀请";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.Yellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        SubEvents = new List<Event> { new Event2_82(), new Event2_83(),new Event2_84(),new Event2_85() };
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "邀请" + Target.Name + "一起玩耍，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "邀请" + Target.Name + "一起玩耍，对方获得浅红色情绪";
    }
}

public class Event2_10 : Event
{
    public Event2_10() : base()
    {
        EventName = "指出对方缺点";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.Red };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "跟" + Target.Name + "吐槽别人，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "跟" + Target.Name + "吐槽别人，对方获得浅红色情绪";
    }
}

public class Event2_11 : Event
{
    public Event2_11() : base()
    {
        EventName = "闲聊";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.Red };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if(value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.Yellow);
        ResultText +=  Self.Name + "跟" + Target.Name + "闲聊，对方好感度上升10点，对方获得浅黄色情绪";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "跟" + Target.Name + "闲聊，对方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, 5);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "跟" + Target.Name + "闲聊，对方好感度下降5点，对方获得浅蓝色情绪";
    }
}

public class Event2_12 : Event
{
    public Event2_12() : base()
    {
        EventName = "结成朋友关系";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.FindRelation(Target).FriendValue = 1;
        Target.FindRelation(Self).FriendValue = 1;
        //flag编号2：前置事件12的标记
        Self.FindRelation(Target).EventFlag[2] = 1;
        ResultText += Self.Name + "跟" + Target.Name + "结成朋友关系";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "跟" + Target.Name + "没能结成朋友关系，好感度下降10点";
    }
}

public class Event2_13 : Event
{
    public Event2_13() : base()
    {
        EventName = "开开玩笑";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if(value < 2)
        {
            ResultText = "大失败";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if(value <12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功，";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.DYellow);
        ResultText += Self.Name + "跟" + Target.Name + "开了开玩笑，对方好感度上升10点，对方获得黄色情绪";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "跟" + Target.Name + "开了开玩笑，对方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "跟" + Target.Name + "开了开玩笑，对方获得浅蓝色情绪，对方好感度下降5点";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.AddEmotion(EColor.DBlue);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "跟" + Target.Name + "开了开玩笑，对方获得蓝色情绪，对方好感度下降10点";
    }
}

public class Event2_14 : Event
{
    public Event2_14() : base()
    {
        EventName = "安慰朋友";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { EColor.DBlue, EColor.Purple, EColor.DPurple};//蓝色和浅紫色也可以
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功，";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.RemoveEmotion(EColor.DPurple);
        ResultText += Self.Name + "安慰了" + Target.Name + "解除对方的紫色情绪（如果有），对方好感度上升10点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "安慰了" + Target.Name + "对方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self,- 5);
        ResultText += Self.Name + "安慰了" + Target.Name + "但没什么效果，对方好感度下降5点";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "安慰了" + Target.Name + "但碰到了对方的痛处，对方好感度下降10点";
    }
}

public class Event2_15 : Event
{
    public Event2_15() : base()
    {
        EventName = "分享经验";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { EColor.DGreen, EColor.Green };//浅绿色也可以
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功，";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 5);
        Target.RemoveEmotion(EColor.DYellow);
        ResultText += Self.Name + "与" + Target.Name + "分享了一些经验，对方获得黄色情绪，对方好感度上升5点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "与" + Target.Name + "分享了一些经验，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "与" + Target.Name + "分享了一些经验，对方获得浅蓝色情绪";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "与" + Target.Name + "分享了一些经验，对方获得蓝色情绪";
    }
}

public class Event2_16 : Event
{
    public Event2_16() : base()
    {
        EventName = "倾听烦恼";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { EColor.DRed };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功，";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.RemoveEmotion(EColor.DRed);
        ResultText += Self.Name + "倾听了" + Target.Name + "的烦恼，对方解除红色情绪，对方获得黄色情绪";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.RemoveEmotion(EColor.DRed);
        ResultText += Self.Name + "倾听了" + Target.Name + "的烦恼，对方红色情绪解除";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "倾听了" + Target.Name + "的烦恼，对方获得浅蓝色情绪";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "倾听了" + Target.Name + "的烦恼，对方获得蓝色情绪";
    }
}

public class Event2_17 : Event
{
    public Event2_17() : base()
    {
        EventName = "与对方在一起庆祝";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { EColor.Yellow };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功，";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        //flag编号17：前置事件17的标记
        Self.FindRelation(Target).EventFlag[3] = 1;
        ResultText += Self.Name + "与" + Target.Name + "一起庆祝，对方好感度上升10点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        //flag编号3：前置事件17的标记
        Self.FindRelation(Target).EventFlag[3] = 1;
        ResultText += Self.Name + "与" + Target.Name + "一起庆祝，对方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "与" + Target.Name + "一起庆祝，对方好感度下降5点";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "与" + Target.Name + "一起庆祝，对方获得浅蓝色情绪，对方好感度下降5点";
    }
}

public class Event2_18 : Event
{
    public Event2_18() : base()
    {
        EventName = "分享合影";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_17
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[3] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "跟" + Target.Name + "分享合影，对方好感度上升5点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "跟" + Target.Name + "分享合影，对方好感度下降5点，对方获得浅蓝色情绪";
    }
}

public class Event2_19 : Event
{
    public Event2_19() : base()
    {
        EventName = "赠送亲手制作的礼物";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "赠送了" + Target.Name + "自己亲手制作的礼物，对方好感度上升5点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "赠送了" + Target.Name + "自己亲手制作的礼物，对方好感度下降5点";
    }
}

public class Event2_20 : Event
{
    public Event2_20() : base()
    {
        EventName = "邀请对方来家里做客";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "邀请" + Target.Name + "来家里做客，对方好感度上升10点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "邀请" + Target.Name + "来家里做客，对方获得蓝色情绪";
    }
}

public class Event2_21 : Event
{
    public Event2_21() : base()
    {
        EventName = "询问对方近况";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        //flag编号4：前置事件21的标记
        Self.FindRelation(Target).EventFlag[4] = 1;
        ResultText += Self.Name + "询问了" + Target.Name + "的近况，对方好感度+5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        ResultText += Self.Name + "询问了" + Target.Name + "的近况，无事发生";
    }
}

public class Event2_22 : Event
{
    public Event2_22() : base()
    {
        EventName = "结成挚友";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_12
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[2] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.FindRelation(Self).FriendValue = 2;
        Self.FindRelation(Target).FriendValue = 2;
        //flag编号5：前置事件22的标记
        Self.FindRelation(Target).EventFlag[5] = 1;
        ResultText += Self.Name + "与" + Target.Name + "结成了挚友";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "想要与" + Target.Name + "结成挚友，但被对方婉拒了，好感度-10";
    }
}

public class Event2_23 : Event
{
    public Event2_23() : base()
    {
        EventName = "谈论婚姻和家庭";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //条件：本人有恋人
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_22
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[5] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.Lover == null)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value < 12)
        {
            ResultText = "成功,";
            return 3;
        }
        else
        {
            ResultText = "大成功，";
            return 4;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.AddEmotion(EColor.Yellow);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "与" + Target.Name + "讨论婚姻与家庭，达成一致，双方获得浅黄色情绪";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "与" + Target.Name + "讨论婚姻与家庭，达成一致，双方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "与" + Target.Name + "讨论婚姻与家庭，没能达成一致，双方获得浅蓝色情绪";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.AddEmotion(EColor.DBlue);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "与" + Target.Name + "讨论婚姻与家庭，没能达成一致，双方获得蓝色情绪";
    }
}

public class Event2_24 : Event
{
    public Event2_24() : base()
    {
        EventName = "提供工作上的帮助";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_21
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[4] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        ResultText += Self.Name + "为" + Target.Name + "提供工作上的帮助，对方获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "为" + Target.Name + "提供工作上的帮助，但获得了蓝色情绪";
    }
}

public class Event2_25 : Event
{
    public Event2_25() : base()
    {
        EventName = "向对方寻求帮助";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() {};
        TargetEmotionRequire = new List<EColor>() {};
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_21
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[4] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
       Self.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "向" + Target.Name + "寻求工作上的帮助，获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "向" + Target.Name + "寻求工作上的帮助，对方获得浅蓝色情绪";
    }
}

public class Event2_26 : Event
{
    public Event2_26() : base()
    {
        EventName = "提出分手";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 7;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > 20)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "向" + Target.Name + "提出分手，对方极力挽回，双方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.FindRelation(Target).LoveValue = 0;
        Target.FindRelation(Self).LoveValue = 0;
        Self.Lover = null;
        Target.Lover = null;
        ResultText += Self.Name + "向" + Target.Name + "提出分手，两人分手";
    }
}

public class Event2_27 : Event
{
    public Event2_27() : base()
    {
        EventName = "精心准备给对方的礼物";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > 20)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "精心准备了给" + Target.Name + "的礼物，对方好感度上升10点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "精心准备了给" + Target.Name + "的礼物，对方好感度下降5点，对方获得浅红色情绪";
    }
}

public class Event2_28 : Event
{
    public Event2_28() : base()
    {
        EventName = "邀请对方约会";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DGreen };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Self.ChangeRelation(Target, 10);
        ResultText += Self.Name + "邀请" + Target.Name + "约会，双方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Red);
        Self.AddEmotion(EColor.Red);
        ResultText += Self.Name + "邀请" + Target.Name + "约会，双方获得浅红色情绪";
    }
}

public class Event2_29 : Event
{
    public Event2_29() : base()
    {
        EventName = "假装偶遇对方";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Target.AddEmotion(EColor.Yellow);
        //flag编号6：前置事件29的标记
        Self.FindRelation(Target).EventFlag[6] = 1;
        ResultText += Self.Name + "假装在走廊里偶遇" + Target.Name + "对方好感度上升5点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "假装在走廊里偶遇" + Target.Name + "获得浅蓝色情绪，对方好感度下降10点";
    }
}

public class Event2_30 : Event
{
    public Event2_30() : base()
    {
        EventName = "送给对方自己身上的饰品";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_29
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[6] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.DYellow);
        //flag编号7：前置事件30的标记
        Self.FindRelation(Target).EventFlag[7] = 1;
        ResultText += Self.Name + "送给" + Target.Name + "自己身上的饰品，对方好感度上升10点，对方获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "送给" + Target.Name + "自己身上的饰品，对方好感度上升5点";
    }
}

public class Event2_31 : Event
{
    public Event2_31() : base()
    {
        EventName = "与对方佩戴一样的戒指";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_30
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[7] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.Yellow);
        //并且做过一次之后需要重新回到29，不然无法发生30和31
        //这里要移除标记29和30（如果存在）
        ResultText += Self.Name + "与" + Target.Name + "佩戴一样的戒指，对方好感度上升10点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "与" + Target.Name + "佩戴一样的戒指，对方好感度上升5点";
    }
}

public class Event2_32 : Event
{
    public Event2_32() : base()
    {
        EventName = "邀请对方去公共空间幽会";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.DYellow);
        Target.AddEmotion(EColor.DYellow);
        ResultText += Self.Name + "邀请" + Target.Name + "去公共空间幽会，双方获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Self.ChangeRelation(Target, -5);
        ResultText += Self.Name + "邀请" + Target.Name + "去公共空间幽会，双方好感度下降5点";
    }
}

public class Event2_33 : Event
{
    public Event2_33() : base()
    {
        EventName = "寻求安慰拥抱";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DPurple, EColor.DBlue };//蓝色情绪也可以
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.RemoveEmotion(EColor.DBlue);
        Self.ChangeRelation(Target, 10);
        //flag编号8：前置事件33的标记
        Self.FindRelation(Target).EventFlag[8] = 1;
        ResultText += Self.Name + "向" + Target.Name + "寻求安慰拥抱，解除蓝色情绪，好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "向" + Target.Name + "寻求安慰拥抱，对方好感度下降5点";
    }
}

public class Event2_34 : Event
{
    public Event2_34() : base()
    {
        //前置事件：33
        EventName = "相约一起月下散步";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_33
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[8] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.Yellow);
        Self.ChangeRelation(Target, 10);
        Target.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "与" + Target.Name + "相约月下散步，双方获得浅黄色情绪，双方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "与" + Target.Name + "相约月下散步，双方获得浅蓝色情绪";
    }
}

public class Event2_35 : Event
{
    public Event2_35() : base()
    {
        EventName = "希望与对方结成恋人";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //条件：没有恋人
    }
    public override bool RelationCheck()
    {
        if (Self.Lover != null)
            return false;
        if (Self.FindRelation(Target).RPoint < 60)
            return false;       
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.FindRelation(Target).LoveValue = 1;
        Target.FindRelation(Self).LoveValue = 1;
        Self.Lover = Target;
        Target.Lover = Self;
        //flag编号9：前置事件35的标记
        Self.FindRelation(Target).EventFlag[9] = 1;
        ResultText += Self.Name + "希望与" + Target.Name + "结为恋人，双方结成恋人";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "希望与" + Target.Name + "结为恋人，被对方拒绝，获得蓝色情绪，好感度下降10点";
    }
}

public class Event2_36 : Event
{
    public Event2_36() : base()
    {
        EventName = "求婚";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 7;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_35
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[9] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.FindRelation(Target).LoveValue = 2;
        Target.FindRelation(Self).LoveValue = 2;
        Self.Lover = Target;
        Target.Lover = Self;
        Self.AddEmotion(EColor.Yellow);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "向" + Target.Name + "求婚，双方结为伴侣，获得双方浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "向" + Target.Name + "求婚，但被拒绝，获得蓝色情绪，好感度下降10点";
    }
}

public class Event2_37 : Event
{
    public Event2_37() : base()
    {
        EventName = "解除仇人关系";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 0)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.FindRelation(Target).FriendValue = 0;
        Target.FindRelation(Self).FriendValue = 0;
        ResultText += Self.Name + "与" + Target.Name + "解除仇人关系";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Red);
        ResultText += Self.Name + "还是觉得" + Target.Name + "是自己的仇人";
    }
}

public class Event2_38 : Event
{
    public Event2_38() : base()
    {
        EventName = "不理会对方打招呼";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > -20)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //flag编号10：前置事件38的标记
        Self.FindRelation(Target).EventFlag[10] = 1;
        ResultText += Self.Name + "没有理会" + Target.Name + "打招呼，无事发生";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "没有理会" + Target.Name + "打招呼，对方获得浅红色情绪";
    }
}

public class Event2_39 : Event
{
    public Event2_39() : base()
    {
        EventName = "否认对方在工作上的努力";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > -20)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "否认" + Target.Name + "在工作上的努力，对方获得浅蓝色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DBlue);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "否认" + Target.Name + "在工作上的努力，对方获得蓝色情绪，对方好感度下降10点";
    }
}
//.
public class Event2_40 : Event
{
    public Event2_40() : base()
    {
        EventName = "在厕所弄湿了对方的鞋子但没有道歉";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire =0;
        //条件：对方不是己方上级
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_35
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[10] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {//此处略过了RelationCheck下面的
        if (Self.CurrentDep != null && Self.CurrentDep.CommandingOffice != null && Self.CurrentDep.CommandingOffice.CurrentManager != Target)
            return true;
        else
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "在厕所弄湿了" + Target.Name + "的鞋子但没有道歉，对方获得浅蓝色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Red);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "在厕所弄湿了" + Target.Name + "的鞋子但没有道歉，对方获得红色情绪，对方好感度下降5点";
    }
}

public class Event2_41 : Event
{
    public Event2_41() : base()
    {
        EventName = "攻击对方的信仰/文化";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > -60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "攻击" + Target.Name + "的信仰/文化，对方获得浅红色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "攻击" + Target.Name + "的信仰/文化，对方获得红色情绪，对方好感度下降5点";
    }
}
//.
public class Event2_42 : Event
{
    public Event2_42() : base()
    {
        EventName = "散播已知对方的秘密";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //条件：持有对方的秘密
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > -60)
            return false;
        return base.RelationCheck();
    }
    public override bool SpecialCheck()
    {
        for(int i = 0; i < Self.CurrentItems.Count; i++)
        {
            if (Self.CurrentItems[i].Target == Target)
                return true;
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, -5);
        //flag编号11：前置事件42的标记
        Self.FindRelation(Target).EventFlag[11] = 1;
        ResultText += Self.Name + "散播" + Target.Name + "的秘密，对方好感度下降5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "散播" + Target.Name + "的秘密，对方获得红色情绪，对方好感度下降10点";
    }
}

public class Event2_43 : Event
{
    public Event2_43() : base()
    {
        //前置事件：42
        EventName = "散播的秘密被否认";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_35
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[11] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Red);
        //flag编号12：前置事件43的标记
        Self.FindRelation(Target).EventFlag[12] = 1;
        ResultText += Self.Name + "散播" + Target.Name + "的秘密被否认，对方获得浅红色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Self.ChangeRelation(Target, -5);
        ResultText += Self.Name + "散播" + Target.Name + "的秘密被否认，对方好感度下降5点";
    }
}

public class Event2_44 : Event
{
    public Event2_44() : base()
    {
        //前置事件：43或45
        EventName = "拿出对方秘密的实锤";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //做一次之后会回到42，做完42之前不能做43和44
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_43或45
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[12] == 1 | Self.FindRelation(Target).EventFlag[13] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DBlue);
        Self.FindRelation(Target).EventFlag[11] = 0;
        Self.FindRelation(Target).EventFlag[12] = 0;
        Self.FindRelation(Target).EventFlag[13] = 0;
        ResultText += Self.Name + "拿出" + Target.Name + "秘密的实锤，对方获得蓝色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -10);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "拿出" + Target.Name + "秘密的实锤，对方好感度下降10点，对方获得红色情绪";
    }
}

public class Event2_45 : Event
{
    public Event2_45() : base()
    {
        EventName = "传说对方的生活作风有问题";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > -60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Red);
        //flag编号13：前置事件45的标记
        Self.FindRelation(Target).EventFlag[13] = 1;
        ResultText += Self.Name + "传说" + Target.Name + "的生活作风有问题，对方获得浅红色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -10);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "传说" + Target.Name + "的生活作风有问题，对方好感度下降10点，对方获得红色情绪";
    }
}

public class Event2_46 : Event
{
    public Event2_46() : base()
    {
        EventName = "结成仇人";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint > -60)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 5);
        Self.ChangeRelation(Target, 5);
        //flag编号13：前置事件45的标记
        Self.FindRelation(Target).EventFlag[14] = 1;
        ResultText += Self.Name + "与" + Target.Name + "的关系稍微缓和了一点，双方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.FindRelation(Self).FriendValue = -2;
        Self.FindRelation(Target).FriendValue = -2;
        ResultText += Self.Name + "与" + Target.Name + "结为仇人";
    }
}

public class Event2_47 : Event
{
    public Event2_47() : base()
    {
        EventName = "嘲笑仇人的品位";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_46
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[14] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "嘲笑" + Target.Name + "的品位，对方获得蓝色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "嘲笑" + Target.Name + "的品位，对方获得红色情绪";
    }
}

public class Event2_48 : Event
{
    public Event2_48() : base()
    {
        //前置事件：46
        EventName = "大声辱骂仇人";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DRed };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_46
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[14] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, -5);
        //flag编号15：前置事件48的标记
        Self.FindRelation(Target).EventFlag[15] = 1;
        ResultText += Self.Name + "大声辱骂" + Target.Name + "，对方获得浅红色情绪，对方好感度-5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "大声辱骂" + Target.Name + "，对方获得红色情绪，对方好感度-10";
    }
}

public class Event2_49 : Event
{
    public Event2_49() : base()
    {
        //前置事件：46
        EventName = "举报仇人密谋";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DOrange };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_46
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[14] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "向领导举报" + Target.Name + "图谋不轨，对方获得蓝色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "向领导举报" + Target.Name + "图谋不轨，对方获得红色情绪";
    }
}
//.
public class Event2_50 : Event
{
    public Event2_50() : base()
    {
        //前置事件：46
        EventName = "删除仇人的工作文件";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DPurple };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_46
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[14] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "删除了" + Target.Name + "的工作文件，对方获得蓝色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        //成功率下降1%，持续三个星期
        Target.InfoDetail.AddPerk(new Perk31(Target), true);
        ResultText += Self.Name + "删除了" + Target.Name + "的工作文件，对方获得红色情绪，成功率下降1%";
    }
}

public class Event2_51 : Event
{
    public Event2_51() : base()
    {
        //前置事件：48
        EventName = "与仇人发生争吵";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_48
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[15] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, -5);
        Self.ChangeRelation(Target,-5);
        ResultText += Self.Name + "与" + Target.Name + "发生了争吵，双方好感度-5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "与" + Target.Name + "发生了争吵，对方获得红色情绪";
    }
}
//.
public class Event2_52 : Event
{
    public Event2_52() : base()
    {
        EventName = "解除师徒关系";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
        //同时对方是师傅
    }
    public override bool RelationCheck()
    {
        if (Self.Master != Target)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        ResultText += Self.Name + "由于" + Target.Name + "是仇人，想要解除师徒关系，但想了想还是算了";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.FindRelation(Target).MasterValue = 0;
        Target.FindRelation(Self).MasterValue = 0;
        Self.Master = null;
        Target.Students.Remove(Self);
        ResultText += Self.Name + "由于" + Target.Name + "是仇人，解除了师徒关系";
    }
}
//.
public class Event2_53 : Event
{
    public Event2_53() : base()
    {
        EventName = "出师";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 4;
        //所处的办公室所需的技能，我方>对方
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override bool SpecialCheck()
    {
        if(Self.CurrentDep != null)
        {
            if (Self.CurrentDep.type == EmpType.HR && Self.HR > Target.HR)
                return true;
            else if (Self.CurrentDep.type == EmpType.Tech && Self.Skill1 > Target.Skill1)
                return true;
            else if (Self.CurrentDep.type == EmpType.Market && Self.Skill2 > Target.Skill2)
                return true;
            else if (Self.CurrentDep.type == EmpType.Product && Self.Skill3 > Target.Skill3)
                return true;
        }
        else if (Self.CurrentOffice != null)
        {
            if (Self.CurrentOffice.building.Type != BuildingType.CEO办公室 || Self.CurrentOffice.building.Type != BuildingType.高管办公室)
            {
                if (Self.CurrentOffice.building.effectValue == 1 && Self.HR > Target.HR)
                    return true;
                else if (Self.CurrentOffice.building.effectValue == 2 && Self.Gossip > Target.Gossip)
                    return true;
                else if (Self.CurrentOffice.building.effectValue == 3 && Self.Strength > Target.Strength)
                    return true;
                else if (Self.CurrentOffice.building.effectValue == 4 && Self.Strategy > Target.Strategy)
                    return true;
                else if (Self.CurrentOffice.building.effectValue == 5 && Self.Forecast > Target.Forecast)
                    return true;
                else if (Self.CurrentOffice.building.effectValue == 6 && Self.Decision > Target.Decision)
                    return true;
                else if (Self.CurrentOffice.building.effectValue == 7 && Self.Finance > Target.Finance)
                    return true;
                else if (Self.CurrentOffice.building.effectValue == 8 && Self.Manage > Target.Manage)
                    return true;
            }
            else
            {
                if (Self.CurrentOffice.OfficeMode == 1 && Self.Decision > Target.Decision)
                    return true;
                else if (Self.CurrentOffice.OfficeMode == 2 && Self.Convince > Target.Convince)
                    return true;
                else if (Self.CurrentOffice.OfficeMode == 3 && Self.Manage > Target.Manage)
                    return true;
                else if (Self.CurrentOffice.OfficeMode == 4 && Self.HR > Target.HR)
                    return true;
                else if (Self.CurrentOffice.OfficeMode == 5 && Self.Forecast > Target.Forecast)
                    return true;
            }
        }
        return false;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.FindRelation(Target).MasterValue = 0;
        Target.FindRelation(Self).MasterValue = 0;
        Self.Master = null;
        Target.Students.Remove(Self);
        //flag编号16：前置事件53的标记
        Self.FindRelation(Target).EventFlag[16] = 1;
        ResultText += Self.Name + "能力强于师傅" + Target.Name + "，选择了出师";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        ResultText += Self.Name + "能力强于师傅" + Target.Name + "，但还是想继续以师徒相处";
    }
}

public class Event2_54 : Event
{
    public Event2_54() : base()
    {
        //前置事件：53
        EventName = "答谢师傅";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_53
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[16] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Self.ChangeRelation(Target, 10);
        Self.AddEmotion(EColor.Yellow);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "答谢了曾经的师傅" + Target.Name + "，双方好感度上升10点，双方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, 5);
        Self.ChangeRelation(Target, 5);
        ResultText += Self.Name + "答谢了曾经的师傅" + Target.Name + "，双方好感度上升5点";
    }
}

public class Event2_55 : Event
{
    public Event2_55() : base()
    {
        EventName = "向对方寻求工作上的帮助";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Self.ChangeRelation(Target, 10);
        Self.AddEmotion(EColor.DYellow);
        Target.AddEmotion(EColor.DYellow);
        //flag编号17：前置事件55的标记
        Self.FindRelation(Target).EventFlag[17] = 1;
        ResultText += Self.Name + "向" + Target.Name + "寻求工作上的帮助，获得黄色情绪，好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "向" + Target.Name + "寻求工作上的帮助，双方获得浅蓝色情绪";
    }
}
//.
public class Event2_56 : Event
{
    public Event2_56() : base()
    {
        //前置事件：55
        EventName = "请求对方来办公室进行指导";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_55
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[17] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //成功率上升1%
        //办公室所需技能经验上升50点
        Self.InfoDetail.AddPerk(new Perk30(Self), true);
        //涨经验
        if (Self.CurrentDep != null)
        {
            if (Self.CurrentDep.type == EmpType.HR && Self.HR > Target.HR)
                Self.GainExp(50, 8);
            else if (Self.CurrentDep.type == EmpType.Tech && Self.Skill1 > Target.Skill1)
                Self.GainExp(50, 1);
            else if (Self.CurrentDep.type == EmpType.Market && Self.Skill2 > Target.Skill2)
                Self.GainExp(50, 2);
            else if (Self.CurrentDep.type == EmpType.Product && Self.Skill3 > Target.Skill3)
                Self.GainExp(50, 3);
        }
        else if (Self.CurrentOffice != null)
        {
            if (Self.CurrentOffice.building.effectValue == 1 && Self.HR > Target.HR)
                Self.GainExp(50, 8);
            else if (Self.CurrentOffice.building.effectValue == 2 && Self.Gossip > Target.Gossip)
                Self.GainExp(50, 15);
            else if (Self.CurrentOffice.building.effectValue == 3 && Self.Strength > Target.Strength)
                Self.GainExp(50, 6);
            else if (Self.CurrentOffice.building.effectValue == 4 && Self.Strategy > Target.Strategy)
                Self.GainExp(50, 12);
            else if (Self.CurrentOffice.building.effectValue == 5 && Self.Forecast > Target.Forecast)
                Self.GainExp(50, 11);
            else if (Self.CurrentOffice.building.effectValue == 6 && Self.Decision > Target.Decision)
                Self.GainExp(50, 10);
            else if (Self.CurrentOffice.building.effectValue == 7 && Self.Finance > Target.Finance)
                Self.GainExp(50, 9);
            else if (Self.CurrentOffice.building.effectValue == 8 && Self.Manage > Target.Manage)
                Self.GainExp(50, 7);
        }
        //flag编号18：前置事件56的标记
        Self.FindRelation(Target).EventFlag[18] = 1;

        ResultText += Self.Name + "请求" + Target.Name + "来办公室进行指导，成功率额外上升1%，获得办公室所需技能经验50点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "请求" + Target.Name + "来办公室进行指导，获得浅蓝色情绪，对方好感度下降5点";
    }
}

public class Event2_57 : Event
{
    public Event2_57() : base()
    {
        //前置事件：55
        EventName = "向对方倾诉工作中遇到的困难";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DBlue };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_55
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[17] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value > 12)
        {
            ResultText = "大成功，";
            return 4;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.RemoveEmotion(EColor.DBlue);
        Self.AddEmotion(EColor.DYellow);
        Self.ChangeRelation(Target, 10);
        ResultText += Self.Name + "向" + Target.Name + "倾诉工作中遇到的困难，解除蓝色情绪，获得黄色情绪，好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.RemoveEmotion(EColor.DBlue);
        Self.ChangeRelation(Target, 5);
        ResultText += Self.Name + "向" + Target.Name + "倾诉工作中遇到的困难，解除蓝色情绪，好感度上升5点";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "向" + Target.Name + "倾诉工作中遇到的困难，获得浅蓝色情绪，对方好感度下降5点";
    }
}

public class Event2_58 : Event
{
    public Event2_58() : base()
    {
        //前置事件：55
        EventName = "向对方讲述自己的学习计划";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DGreen };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_55
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[17] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.DYellow);
        Self.ChangeRelation(Target, 5);
        ResultText += Self.Name + "向" + Target.Name + "对方讲述自己的学习计划，获得黄色情绪，好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.DBlue);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "向" + Target.Name + "对方讲述自己的学习计划，获得蓝色情绪，好感度下降10点";
    }
}

public class Event2_59 : Event
{
    public Event2_59() : base()
    {
        //前置事件：56
        EventName = "对对方表示感谢";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_56
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[18] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "对" + Target.Name + "表示感谢，对方获得浅黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "对" + Target.Name + "表示感谢，对方获得浅黄色情绪";
    }
}
//.
public class Event2_60 : Event
{
    public Event2_60() : base()
    {
        //前置事件：55
        EventName = "拜对方为师";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方不能是师傅或者徒弟
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_55
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[171] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Target.Students.Count > 0 || Target.Master != null)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.FindRelation(Self).MasterValue = 1;
        Self.FindRelation(Target).MasterValue = 2;
        Self.Master = Target;
        Target.Students.Add(Self);
        ResultText += Self.Name + "拜" + Target.Name + "为师";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "拜" + Target.Name + "为师，但被拒绝，好感度下降10点";
    }
}
//.
public class Event2_61 : Event
{
    public Event2_61() : base()
    {
        //前置事件：60
        EventName = "向师傅吸取经验";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 4;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //部门所需技能经验增长100点
        if (Self.CurrentDep != null)
        {
            if (Self.CurrentDep.type == EmpType.HR && Self.HR > Target.HR)
                Self.GainExp(100, 8);
            else if (Self.CurrentDep.type == EmpType.Tech && Self.Skill1 > Target.Skill1)
                Self.GainExp(100, 1);
            else if (Self.CurrentDep.type == EmpType.Market && Self.Skill2 > Target.Skill2)
                Self.GainExp(100, 2);
            else if (Self.CurrentDep.type == EmpType.Product && Self.Skill3 > Target.Skill3)
                Self.GainExp(100, 3);
        }
        else if (Self.CurrentOffice != null)
        {
            if (Self.CurrentOffice.building.Type != BuildingType.CEO办公室 || Self.CurrentOffice.building.Type != BuildingType.高管办公室)
            {
                if (Self.CurrentOffice.building.effectValue == 1 && Self.HR > Target.HR)
                    Self.GainExp(100, 8);
                else if (Self.CurrentOffice.building.effectValue == 2 && Self.Gossip > Target.Gossip)
                    Self.GainExp(100, 15);
                else if (Self.CurrentOffice.building.effectValue == 3 && Self.Strength > Target.Strength)
                    Self.GainExp(100, 6);
                else if (Self.CurrentOffice.building.effectValue == 4 && Self.Strategy > Target.Strategy)
                    Self.GainExp(100, 12);
                else if (Self.CurrentOffice.building.effectValue == 5 && Self.Forecast > Target.Forecast)
                    Self.GainExp(100, 11);
                else if (Self.CurrentOffice.building.effectValue == 6 && Self.Decision > Target.Decision)
                    Self.GainExp(100, 10);
                else if (Self.CurrentOffice.building.effectValue == 7 && Self.Finance > Target.Finance)
                    Self.GainExp(100, 9);
                else if (Self.CurrentOffice.building.effectValue == 8 && Self.Manage > Target.Manage)
                    Self.GainExp(100, 7);
            }
            else
            {
                if (Self.CurrentOffice.OfficeMode == 1)
                    Self.GainExp(50, 10);
                else if (Self.CurrentOffice.OfficeMode == 2)
                    Self.GainExp(50, 13);
                else if (Self.CurrentOffice.OfficeMode == 3)
                    Self.GainExp(50, 7);
                else if (Self.CurrentOffice.OfficeMode == 4)
                    Self.GainExp(50, 8);
                else if (Self.CurrentOffice.OfficeMode == 5)
                    Self.GainExp(50, 11);
            }
        }
        Self.AddEmotion(EColor.DYellow);
        //flag编号19：前置事件60的标记
        Self.FindRelation(Target).EventFlag[19] = 1;
        ResultText += Self.Name + "向" + Target.Name + "吸取经验，部门所需技能经验增长100点，获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        //部门所需技能经验增长50点
        Self.AddEmotion(EColor.Blue);

        if (Self.CurrentDep != null)
        {
            if (Self.CurrentDep.type == EmpType.HR && Self.HR > Target.HR)
                Self.GainExp(50, 8);
            else if (Self.CurrentDep.type == EmpType.Tech && Self.Skill1 > Target.Skill1)
                Self.GainExp(50, 1);
            else if (Self.CurrentDep.type == EmpType.Market && Self.Skill2 > Target.Skill2)
                Self.GainExp(50, 2);
            else if (Self.CurrentDep.type == EmpType.Product && Self.Skill3 > Target.Skill3)
                Self.GainExp(50, 3);
        }
        else if (Self.CurrentOffice != null)
        {
            if (Self.CurrentOffice.building.Type != BuildingType.CEO办公室 || Self.CurrentOffice.building.Type != BuildingType.高管办公室)
            {
                if (Self.CurrentOffice.building.effectValue == 1 && Self.HR > Target.HR)
                    Self.GainExp(50, 8);
                else if (Self.CurrentOffice.building.effectValue == 2 && Self.Gossip > Target.Gossip)
                    Self.GainExp(50, 15);
                else if (Self.CurrentOffice.building.effectValue == 3 && Self.Strength > Target.Strength)
                    Self.GainExp(50, 6);
                else if (Self.CurrentOffice.building.effectValue == 4 && Self.Strategy > Target.Strategy)
                    Self.GainExp(50, 12);
                else if (Self.CurrentOffice.building.effectValue == 5 && Self.Forecast > Target.Forecast)
                    Self.GainExp(50, 11);
                else if (Self.CurrentOffice.building.effectValue == 6 && Self.Decision > Target.Decision)
                    Self.GainExp(50, 10);
                else if (Self.CurrentOffice.building.effectValue == 7 && Self.Finance > Target.Finance)
                    Self.GainExp(50, 9);
                else if (Self.CurrentOffice.building.effectValue == 8 && Self.Manage > Target.Manage)
                    Self.GainExp(50, 7);
            }
            else
            {
                if (Self.CurrentOffice.OfficeMode == 1)
                    Self.GainExp(50, 10);
                else if (Self.CurrentOffice.OfficeMode == 2)
                    Self.GainExp(50, 13);
                else if (Self.CurrentOffice.OfficeMode == 3)
                    Self.GainExp(50, 7);
                else if (Self.CurrentOffice.OfficeMode == 4)
                    Self.GainExp(50, 8);
                else if (Self.CurrentOffice.OfficeMode == 5)
                    Self.GainExp(50, 11);
            }
        }
        ResultText += Self.Name + "向" + Target.Name + "吸取经验，部门所需技能经验增长50点，获得浅蓝色情绪";
    }
}

public class Event2_62 : Event
{
    public Event2_62() : base()
    {
        //前置事件：60
        EventName = "与师傅讨论人生";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 4;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_60
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[19] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        Self.ChangeRelation(Target, 10);
        ResultText += Self.Name + "与师傅" + Target.Name + "讨论人生，获得黄色情绪，双方好感度增加10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "与师傅" + Target.Name + "讨论人生，获得蓝色情绪";
    }
}

public class Event2_63 : Event
{
    public Event2_63() : base()
    {
        //前置事件：60
        EventName = "给师傅送红包";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DYellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 4;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_60
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[19] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "给师傅" + Target.Name + "红包，对方获得浅黄色情绪，对方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Red);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "给师傅" + Target.Name + "红包，对方获得浅红色情绪，对方好感度下降10点";
    }
}

public class Event2_64 : Event
{
    public Event2_64() : base()
    {
        //前置事件：60
        EventName = "接受师傅训话";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 4;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_60
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[19] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.DYellow);
        ResultText += Self.Name + "接受师傅" + Target.Name + "训话，获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.DRed);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "接受师傅" + Target.Name + "训话，获得红色情绪，好感度下降10点";
    }
}

public class Event2_65 : Event
{
    public Event2_65() : base()
    {
        EventName = "布置工作";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的下属
    }
    public override bool RelationCheck()
    {
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value > 12)
        {
            ResultText = "大成功，";
            return 4;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 5);
        Self.ChangeRelation(Target, 5);
        ResultText += Self.Name + "给" + Target.Name + "布置工作，对方获得黄色情绪，双方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Red);
        ResultText += Self.Name + "给" + Target.Name + "布置工作，对方获得浅红色情绪";
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "给" + Target.Name + "布置工作，对方获得蓝色情绪，对方好感度下降5点";
    }
}
//.
public class Event2_66 : Event
{
    public Event2_66() : base()
    {
        EventName = "帮员工申请涨工资";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 1 ;
        //对方是自己的下属
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 20)
            return false;
        if (Self.CurrentOffice == null || (Target.CurrentDep == null && Target.CurrentOffice == null))
            return false;
        if (Target.CurrentDep != null && Self.CurrentOffice.ControledDeps.Contains(Target.CurrentDep) == false)
            return false;
        if (Target.CurrentOffice != null && Self.CurrentOffice.ControledOffices.Contains(Target.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //对方工资上升10
        Target.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 10);
        Target.SalaryExtra += 10;
        ResultText += Self.Name + "帮助" + Target.Name + "申请涨工资，对方工资上升10，对方好感度上升10点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "帮助" + Target.Name + "申请涨工资失败，对方好感度上升5点，获得浅蓝色情绪";
    }
}
//..
public class Event2_67 : Event
{
    public Event2_67() : base()
    {
        EventName = "帮员工申请升职";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 1;
        //对方是自己的下属
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 20)
            return false;
        if (Self.CurrentOffice == null || (Target.CurrentDep == null && Target.CurrentOffice == null))
            return false;
        if (Target.CurrentDep != null && Self.CurrentOffice.ControledDeps.Contains(Target.CurrentDep) == false)
            return false;
        if (Target.CurrentOffice != null && Self.CurrentOffice.ControledOffices.Contains(Target.CurrentOffice) == false)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //弹出对话框，向CEO申请升职
        Target.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 10);
        Self.InfoDetail.AddPerk(new Perk32(Self), true);
        GC.CreateMessage(Self.Name + "请求升职");
        ResultText += Self.Name + "帮助" + Target.Name + "申请升职，向CEO申请升职，对方好感度上升10点，对方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, 5);
        Self.InfoDetail.AddPerk(new Perk32(Self), true);
        GC.CreateMessage(Self.Name + "请求升职");
        ResultText += Self.Name + "帮助" + Target.Name + "申请升职，对方好感度上升5点，获得浅蓝色情绪";
    }
}
//.
public class Event2_68 : Event
{
    public Event2_68() : base()
    {
        EventName = "提醒员工注意工作状态";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { EColor.DPurple };
        RelationRequire = 0;
        //对方是自己的下属
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 20)
            return false;
        if (Self.CurrentOffice == null || (Target.CurrentDep == null && Target.CurrentOffice == null))
            return false;
        if (Target.CurrentDep != null && Self.CurrentOffice.ControledDeps.Contains(Target.CurrentDep) == false)
            return false;
        if (Target.CurrentOffice != null && Self.CurrentOffice.ControledOffices.Contains(Target.CurrentOffice) == false)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败，";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value > 12)
        {
            ResultText = "大成功，";
            return 4;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.RemoveEmotion(EColor.DPurple);
        Target.ChangeRelation(Self, 5);
        Target.AddEmotion(EColor.DYellow);
        ResultText += Self.Name + "提醒员工" + Target.Name + "注意工作状态，对方紫色情绪解除，对方好感度+5，对方获得黄色情绪";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.RemoveEmotion(EColor.DPurple);
        ResultText += Self.Name + "提醒员工" + Target.Name + "注意工作状态，对方紫色情绪解除";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "提醒员工" + Target.Name + "注意工作状态，对方获得蓝色情绪";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.AddEmotion(EColor.DRed);
        Target.ChangeRelation(Self,-5);
        ResultText += Self.Name + "提醒员工" + Target.Name + "注意工作状态，对方获得红色情绪，对方好感度-5";
    }
}
//.
public class Event2_69 : Event
{
    public Event2_69() : base()
    {
        EventName = "对部下进行鼓励和表扬";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的下属
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentOffice == null || (Target.CurrentDep == null && Target.CurrentOffice == null))
            return false;
        if (Target.CurrentDep != null && Self.CurrentOffice.ControledDeps.Contains(Target.CurrentDep) == false)
            return false;
        if (Target.CurrentOffice != null && Self.CurrentOffice.ControledOffices.Contains(Target.CurrentOffice) == false)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败，";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value > 12)
        {
            ResultText = "大成功，";
            return 4;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.DYellow);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "对部下" + Target.Name + "进行鼓励和表扬，对方获得黄色情绪，对方获得浅黄色情绪，对方好感度上升10点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "对部下" + Target.Name + "进行鼓励和表扬，对方获得浅黄色情绪，对方好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "对部下" + Target.Name + "进行鼓励和表扬，对方获得浅蓝色情绪";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Target.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "对部下" + Target.Name + "进行鼓励和表扬，对方获得浅蓝色情绪，对方好感度下降5点";
    }
}
//.
public class Event2_70 : Event
{
    public Event2_70() : base()
    {
        EventName = "向员工传授经验";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的下属
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        if (Self.CurrentOffice == null || (Target.CurrentDep == null && Target.CurrentOffice == null))
            return false;
        if (Target.CurrentDep != null && Self.CurrentOffice.ControledDeps.Contains(Target.CurrentDep) == false)
            return false;
        if (Target.CurrentOffice != null && Self.CurrentOffice.ControledOffices.Contains(Target.CurrentOffice) == false)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        //对方所在部门所需技能经验+50
        if (Self.CurrentDep != null)
        {
            if (Self.CurrentDep.type == EmpType.HR && Self.HR > Target.HR)
                Self.GainExp(50, 8);
            else if (Self.CurrentDep.type == EmpType.Tech && Self.Skill1 > Target.Skill1)
                Self.GainExp(50, 1);
            else if (Self.CurrentDep.type == EmpType.Market && Self.Skill2 > Target.Skill2)
                Self.GainExp(50, 2);
            else if (Self.CurrentDep.type == EmpType.Product && Self.Skill3 > Target.Skill3)
                Self.GainExp(50, 3);
        }
        else if (Self.CurrentOffice != null)
        {
            if (Self.CurrentOffice.building.effectValue == 1 && Self.HR > Target.HR)
                Self.GainExp(50, 8);
            else if (Self.CurrentOffice.building.effectValue == 2 && Self.Gossip > Target.Gossip)
                Self.GainExp(50, 15);
            else if (Self.CurrentOffice.building.effectValue == 3 && Self.Strength > Target.Strength)
                Self.GainExp(50, 6);
            else if (Self.CurrentOffice.building.effectValue == 4 && Self.Strategy > Target.Strategy)
                Self.GainExp(50, 12);
            else if (Self.CurrentOffice.building.effectValue == 5 && Self.Forecast > Target.Forecast)
                Self.GainExp(50, 11);
            else if (Self.CurrentOffice.building.effectValue == 6 && Self.Decision > Target.Decision)
                Self.GainExp(50, 10);
            else if (Self.CurrentOffice.building.effectValue == 7 && Self.Finance > Target.Finance)
                Self.GainExp(50, 9);
            else if (Self.CurrentOffice.building.effectValue == 8 && Self.Manage > Target.Manage)
                Self.GainExp(50, 7);
        }
        ResultText += Self.Name + "向员工" + Target.Name + "传授经验，对方所在部门所需的技能经验+50，对方获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "向员工" + Target.Name + "传授经验，对方获得浅蓝色情绪";
    }
}
//.
public class Event2_71 : Event
{
    public Event2_71() : base()
    {
        EventName = "启发员工想法";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DGreen };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的下属
    }
    public override bool RelationCheck()
    {
        if (Self.CurrentOffice == null || (Target.CurrentDep == null && Target.CurrentOffice == null))
            return false;
        if (Target.CurrentDep != null && Self.CurrentOffice.ControledDeps.Contains(Target.CurrentDep) == false)
            return false;
        if (Target.CurrentOffice != null && Self.CurrentOffice.ControledOffices.Contains(Target.CurrentOffice) == false)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        //对方成功率上升1%
        Target.InfoDetail.AddPerk(new Perk30(Target), true);
        ResultText += Self.Name + "启发员工" + Target.Name + "的想法，对方成功率上升1%，对方获得黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.DBlue);
        ResultText += Self.Name + "启发员工" + Target.Name + "的想法，对方获得蓝色情绪";
    }
}
//.
public class Event2_72 : Event
{
    public Event2_72() : base()
    {
        EventName = "对部下进行鼓励和表扬";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool RelationCheck()
    {
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败，";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value > 12)
        {
            ResultText = "大成功，";
            return 4;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Target.ChangeRelation(Self, 10);
        Self.ChangeRelation(Target, 10);
        Target.AddEmotion(EColor.DYellow);
        //flag编号20：前置事件72的标记
        Self.FindRelation(Target).EventFlag[20] = 1;
        ResultText += Self.Name + "恭迎领导" + Target.Name + "上任，双方获得黄色情绪，双方好感度上升10点";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Yellow);
        Self.AddEmotion(EColor.Yellow);
        //flag编号20：前置事件72的标记
        Self.FindRelation(Target).EventFlag[20] = 1;
        ResultText += Self.Name + "恭迎领导" + Target.Name + "上任，双方获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Yellow);
        Self.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "恭迎领导" + Target.Name + "上任，对方获得浅黄色情绪，获得浅蓝色情绪";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.AddEmotion(EColor.DBlue);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "恭迎领导" + Target.Name + "上任，获得蓝色情绪，对方获得浅红色情绪";
    }
}
//.!
public class Event2_73 : Event
{
    public Event2_73() : base()
    {
        //前置事件：72
        EventName = "尝试挖掘领导秘密";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DGreen };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = -2;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //获得对方的秘密
        Self.CurrentItems.Add(new EmpItem(0, -1, Target));
        ResultText += Self.Name + "尝试挖掘" + Target.Name + "的秘密，获得对方的秘密";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.DBlue);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "尝试挖掘" + Target.Name + "的秘密，对方获得红色情绪，获得蓝色情绪";
    }
}
//!?
public class Event2_74 : Event
{
    public Event2_74() : base()
    {
        //前置事件：72
        EventName = "与同事形成迫害领导的密谋派系";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DGreen };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //获得对方的秘密
        Self.CurrentItems.Add(new EmpItem(0, -1, Target));
        ResultText += Self.Name + "想要组成谋害" + Target.Name + "的派系，最后打消了主意，好感度上升5点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        //暂时先不实装，组建一个以谋害领导为目的的派系
        ResultText += Self.Name + "想要组成谋害" + Target.Name + "的派系，组建派系，阴谋谋害领导";
    }
}
//..
public class Event2_75 : Event
{
    public Event2_75() : base()
    {
        //前置事件：72
        EventName = "向领导申请涨工资";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DOrange };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        //对方工资增长10
        Self.SalaryExtra += 10;
        Self.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "向领导" + Target.Name + "申请涨工资，领导答应，工资增长10，获得浅黄色情绪";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.ChangeRelation(Self, -5);
        Self.ChangeRelation(Target, -5);
        Self.AddEmotion(EColor.Blue);
        Target.AddEmotion(EColor.Red);
        ResultText += Self.Name + "向领导" + Target.Name + "申请涨工资，双方好感度下降5，获得浅蓝色情绪，对方获得浅红色情绪";
    }
}
//.
public class Event2_76 : Event
{
    public Event2_76() : base()
    {
        //前置事件：72
        EventName = "指责领导无理取闹";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DRed };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -5);
        Self.ChangeRelation(Target, -5);
        ResultText += Self.Name + "指责领导" + Target.Name + "无理取闹，对方获得浅蓝色情绪，双方好感度下降5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        Target.ChangeRelation(Self, -10);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "指责领导" + Target.Name + "无理取闹，对方获得红色情绪，双方好感度-10";
    }
}
//.
public class Event2_77 : Event
{
    public Event2_77() : base()
    {
        //前置事件：72
        EventName = "向领导讲述工作中遇到的困难";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DPurple };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 2)
        {
            ResultText = "大失败，";
            return 1;
        }
        else if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else if (value > 12)
        {
            ResultText = "大成功，";
            return 4;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void MajorSuccess(float Posb)
    {
        base.MajorSuccess(Posb);
        Self.RemoveEmotion(EColor.DPurple);
        Self.AddEmotion(EColor.Yellow);
        Self.ChangeRelation(Target, 10);
        ResultText += Self.Name + "向领导" + Target.Name + "讲述工作中遇到的困难，解除紫色情绪，获得浅黄色情绪，好感度+10";
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Self.ChangeRelation(Target, 5);
        Self.RemoveEmotion(EColor.DPurple);
        ResultText += Self.Name + "向领导" + Target.Name + "讲述工作中遇到的困难，解除紫色情绪，好感度+5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.DBlue);
        Self.ChangeRelation(Target, -5);
        ResultText += Self.Name + "向领导" + Target.Name + "讲述工作中遇到的困难，获得蓝色情绪，好感度-5";
    }
    public override void MajorFailure(float Posb)
    {
        base.MajorFailure(Posb);
        Self.AddEmotion(EColor.DRed);
        Self.ChangeRelation(Target, -10);
        ResultText += Self.Name + "向领导" + Target.Name + "讲述工作中遇到的困难，获得红色情绪，好感度-10";
    }
}
//..
public class Event2_78 : Event
{
    public Event2_78() : base()
    {
        //前置事件：72
        EventName = "威胁领导";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DRed };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
        //持有对方秘密
    }
    public override bool RelationCheck()
    {
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        for (int i = 0; i < Self.CurrentItems.Count; i++)
        {
            if (Self.CurrentItems[i].Target == Target)
                return true&c;
        }
        return false&c;
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Red);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "用领导" + Target.Name + "的秘密进行威胁，对方获得浅红色情绪，对方好感度-5";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DBlue);
        Target.ChangeRelation(Self, -10);
        ResultText += Self.Name + "用领导" + Target.Name + "的秘密进行威胁，对方获得蓝色情绪，对方好感度-10";
    }
}
//.
public class Event2_79 : Event
{
    public Event2_79() : base()
    {
        //前置事件：72
        EventName = "给领导送红包";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DRed };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 20)
            return false;
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "给领导" + Target.Name + "送红包，对方好感度+10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Red);
        Self.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, -5);
        ResultText += Self.Name + "给领导" + Target.Name + "送红包，对方获得浅红色情绪，获得浅蓝色情绪，对方好感度-5";
    }
}
//.
public class Event2_80 : Event
{
    public Event2_80() : base()
    {
        //前置事件：72
        EventName = "给领导送礼物";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DRed };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 20)
            return false;
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "给领导" + Target.Name + "送红包，对方好感度+10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "给领导" + Target.Name + "送红包，获得浅蓝色情绪";
    }
}
//.
public class Event2_81 : Event
{
    public Event2_81() : base()
    {
        //前置事件：72
        EventName = "夸奖领导统领有方";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.DYellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
        //对方是自己的上司
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_72
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[20] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 60)
            return false;
        if (Target.CurrentOffice == null || (Self.CurrentDep == null && Self.CurrentOffice == null))
            return false;
        if (Self.CurrentDep != null && Target.CurrentOffice.ControledDeps.Contains(Self.CurrentDep) == false)
            return false;
        if (Self.CurrentOffice != null && Target.CurrentOffice.ControledOffices.Contains(Self.CurrentOffice) == false)
            return false;

        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.ChangeRelation(Self, 10);
        Target.AddEmotion(EColor.Yellow);
        ResultText += Self.Name + "夸奖领导" + Target.Name + "统领有方，对方获得浅黄色情绪，对方好感度+10";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Self.AddEmotion(EColor.Blue);
        Target.ChangeRelation(Self, 5);
        ResultText += Self.Name + "夸奖领导" + Target.Name + "统领有方，对方好感度+5，获得浅蓝色情绪";
    }
}
public class Event2_82 : Event
{
    public Event2_82() : base()
    {
        EventName = "邀请朋友一起吃饭";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.Yellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.Yellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "邀请" + Target.Name + "一起吃饭，对方获得浅黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "邀请" + Target.Name + "一起吃饭，对方获得浅蓝色情绪";
    }
}
public class Event2_83 : Event
{
    public Event2_83() : base()
    {
        EventName = "邀请朋友一起喝咖啡";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.Yellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "邀请" + Target.Name + "一起喝咖啡，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "邀请" + Target.Name + "一起喝咖啡，对方获得红色情绪";
    }
}
public class Event2_84 : Event
{
    public Event2_84() : base()
    {
        EventName = "邀请朋友一起玩游戏";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.Yellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "邀请" + Target.Name + "一起玩游戏，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.DRed);
        ResultText += Self.Name + "邀请" + Target.Name + "一起玩游戏，对方获得浅红色情绪";
    }
}
public class Event2_85 : Event
{
    public Event2_85() : base()
    {
        EventName = "邀请朋友去公共空间闲聊";
        HaveTarget = true;
        SelfEmotionRequire = new List<EColor>() { EColor.Yellow };
        TargetEmotionRequire = new List<EColor>() { };
        RelationRequire = 0;
    }
    public override bool SpecialCheck()
    {
        //前置事件：Event2_1
        bool c = false;
        if (Self.FindRelation(Target).EventFlag[1] == 1)
        {
            c = true;
        }
        return c;
    }
    public override bool RelationCheck()
    {
        if (Self.FindRelation(Target).RPoint < 40)
            return false;
        return base.RelationCheck();
    }
    public override int ExtraValue()
    {
        int Extra = 0;
        Extra += (int)(Self.Charm * 0.2);

        Extra += RelationBonus() + MoraleBonus() + CRBonus();
        return Extra;
    }
    public override int FindResult()
    {
        int value = Random.Range(2, 13);
        value += ExtraValue();
        if (value < 7)
        {
            ResultText = "失败,";
            return 2;
        }
        else
        {
            ResultText = "成功,";
            return 3;
        }
        //1大失败 2失败 3成功 4大成功
    }
    public override void Success(float Posb)
    {
        base.Success(Posb);
        Target.AddEmotion(EColor.DYellow);
        Target.ChangeRelation(Self, 10);
        ResultText += Self.Name + "邀请" + Target.Name + "去公共空间闲聊，对方获得黄色情绪，对方好感度上升10点";
    }
    public override void Failure(float Posb)
    {
        base.Failure(Posb);
        Target.AddEmotion(EColor.Blue);
        ResultText += Self.Name + "邀请" + Target.Name + "去公共空间闲聊，对方获得浅蓝色情绪";
    }
}
#endregion

static public class EventData
{
    #region 事件版本2内容
    //工作学习事件
    static public List<Event> StudyEvents = new List<Event>()
    {
        new Event43(), new Event44(), new Event45()
    };
    static public List<Event> StudyForceEvents = new List<Event>() { new Event46() };

    //心体恢复事件
    static public List<Event> RecoverEvent = new List<Event>()
    {
        new Event8(), new Event10(), new Event11(), new Event12(), new Event48()
    };
    static public List<Event> RecoverForceEvent = new List<Event>()
    {
        new Event14(), new Event15(), new Event16(), new Event17()
    };

    //谋划野心事件
    static public List<Event> AmbitionEvent = new List<Event>()
    {
        new Event1(), new Event2(), new Event3(), new Event4(), new Event5(), new Event6(), new Event7(), new Event47()
    };
    static public List<Event> AmbitionForceEvent = new List<Event>();

    //关系交往事件
    static public List<Event> SocialEvent = new List<Event>()
    {
        new Event18(), new Event19(), new Event20(), new Event21(), new Event22(), new Event23(), new Event24(),
        new Event25(), new Event26(), new Event27(), new Event31(), new Event32(), new Event33(), new Event34(),
        new Event35(), new Event36(), new Event37(), new Event38(), new Event39(), new Event41(),
    };
    static public List<Event> SocialForceEvent = new List<Event>()
    {
        new Event28(), new Event29(), new Event30(), new Event36(), new Event38(), new Event40()
    };

    static public void CopyList(List<Event> Self, List<Event> Target)
    {
        for(int i = 0; i < Target.Count; i++)
        {
            Self.Add(Target[i].Clone());
        }
    }
    #endregion

    //初始序列
    public static List<Event> InitialList = new List<Event>()
    {

    };

    //公司序列
    public static List<Event> CompanyList = new List<Event>()
    {
        
    };

    //个人关系序列
    public static List<Event> RelationList = new List<Event>()
    {
        
    };

    //权重1-4事件链表
    static public List<Event> E1 = new List<Event>()
    {
        new Event2_1(),new Event2_27(),new Event2_38(),new Event2_39(),new Event2_40(),new Event2_55(),new Event2_65()
    };
    static public List<Event> E2 = new List<Event>()
    {
        new Event2_3(),new Event2_4(),new Event2_5(),new Event2_6(), new Event2_7(),new Event2_8(),new Event2_9(),new Event2_10(),new Event2_11(),new Event2_12(),new Event2_28(),new Event2_41(),new Event2_42(),new Event2_45(),new Event2_46(),new Event2_56(),new Event2_57(),new Event2_58(),new Event2_59(),new Event2_60(),new Event2_66(),new Event2_67(),new Event2_76(),new Event2_77(),new Event2_78(),new Event2_79(),new Event2_80(),new Event2_29(),new Event2_30(),new Event2_31()
    };
    static public List<Event> E3 = new List<Event>()
    {
        new Event2_2(),new Event2_13(),new Event2_14(),new Event2_15(),new Event2_16(),new Event2_17(),new Event2_18(),new Event2_32(),new Event2_33(),new Event2_34(),new Event2_35(),new Event2_36(),new Event2_43(),new Event2_44(),new Event2_47(),new Event2_48(),new Event2_49(),new Event2_50(),new Event2_51(),new Event2_53(),new Event2_54(),new Event2_61(),new Event2_62(),new Event2_63(),new Event2_64(),new Event2_68(),new Event2_69(),new Event2_70(),new Event2_71(),new Event2_73(),new Event2_74(),new Event2_75(),new Event2_81()
    };
    static public List<Event> E4 = new List<Event>()
    {
        new Event2_19(),new Event2_20(),new Event2_21(),new Event2_22(),new Event2_23(),new Event2_24(),new Event2_25(),new Event2_26(),new Event2_37(),new Event2_52(),new Event2_72()
    };
}

public class EventGroup
{
    public bool Lock = false;
    public List<Event> eventList;

    /// <summary>
    /// 构造事件组
    /// </summary>
    /// <param name="list">包含的事件列表</param>
    /// <param name="lockGroup">锁定状态</param>
    public EventGroup(List<Event> list, bool lockGroup = false)
    {
        eventList = list;
        Lock = lockGroup;
    }
}