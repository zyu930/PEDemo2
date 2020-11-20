﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficeControl : MonoBehaviour
{
    public bool CanWork = false;
    public int ManageValue = 0, Progress = 100;
    public int OfficeMode = 1;//1决策 战略充能 2人力 心力回复(说服) 3管理 加启发 4招聘 5行业 刷新建筑
    public int ExpTime = 5;//每5工时加经验

    public GameObject OfficeWarning;
    public Transform SRateDetailPanel;
    public Employee CurrentManager;
    public OfficeControl CommandingOffice;
    public DepSelect DS;
    public GameControl GC;
    public Building building;
    public Text Text_OfficeName, Text_EmpName, Text_MAbility, Text_Progress, Text_OfficeMode, Text_TimeLeft, Text_SuccessRate, Text_SRateDetail;
    public Button ActiveButton, ModeChangeButton, SetCommaindgOfficeButton;

    public List<DepControl> ControledDeps = new List<DepControl>();
    public List<OfficeControl> ControledOffices = new List<OfficeControl>();
    public List<OfficeControl> InRangeOffices = new List<OfficeControl>();

    private void Start()
    {
        if (building.Type == BuildingType.CEO办公室)
        {
            GC.HourEvent.AddListener(TimePass);
            SRateDetailPanel.parent = GC.SRateDetailContent;
        }
        SetOfficeUI();
    }

    //放入和移除高管时调用
    public void SetOfficeStatus()
    {
        if (CurrentManager != null)
        {
            Text_EmpName.text = "当前高管:" + CurrentManager.Name;
            if (building.Type == BuildingType.高管办公室 || building.Type == BuildingType.CEO办公室)
            {
                Text_MAbility.text = "管理:" + CurrentManager.Manage;
                ManageValue = CurrentManager.Manage + GC.ManageExtra;
                CurrentManager.InfoDetail.CreateStrategy();
                CurrentManager.NoPromotionTime = 0;
                for(int i = 0; i < CurrentManager.InfoDetail.PerksInfo.Count; i++)
                {
                    if (CurrentManager.InfoDetail.PerksInfo[i].CurrentPerk.Num == 32)
                    {
                        CurrentManager.InfoDetail.PerksInfo[i].CurrentPerk.RemoveEffect();
                        break;
                    }
                }
                CheckManage();
                SetOfficeUI();
            }
            else if (building.effectValue == 1)
                Text_MAbility.text = "人力:" + CurrentManager.HR;
            else if (building.effectValue == 2)
                Text_MAbility.text = "八卦:" + CurrentManager.Gossip;
            else if (building.effectValue == 3)
                Text_MAbility.text = "强壮:" + CurrentManager.Strength;
            else if (building.effectValue == 4)
                Text_MAbility.text = "谋略:" + CurrentManager.Strategy;
            else if (building.effectValue == 5)
                Text_MAbility.text = "行业:" + CurrentManager.Forecast;
            else if (building.effectValue == 6)
                Text_MAbility.text = "决策:" + CurrentManager.Decision;
            else if (building.effectValue == 7)
                Text_MAbility.text = "财务:" + CurrentManager.Finance;
            else if (building.effectValue == 8)
                Text_MAbility.text = "管理:" + CurrentManager.Finance;

        }
        else
        {
            Text_EmpName.text = "当前高管:无";
            if (building.effectValue == 1)
                Text_MAbility.text = "人力:--";
            else if (building.effectValue == 2)
                Text_MAbility.text = "八卦:--";
            else if (building.effectValue == 3)
                Text_MAbility.text = "强壮:--";
            else if (building.effectValue == 4)
                Text_MAbility.text = "谋略:--";
            else if (building.effectValue == 5)
                Text_MAbility.text = "行业:--";
            else if (building.effectValue == 6)
                Text_MAbility.text = "决策:--";
            else if (building.effectValue == 7)
                Text_MAbility.text = "财务:--";
            else if (building.effectValue == 8)
                Text_MAbility.text = "管理:--";
            ManageValue = 0;
        }

    }

    public void CheckManage()
    {
        if (CurrentManager != null)
            ManageValue = CurrentManager.Manage + GC.ManageExtra;
        for(int i = 0; i < ControledDeps.Count; i++)
        {
            if (i < ManageValue)
            {
                ControledDeps[i].canWork = true;
                ControledDeps[i].OfficeWarning.SetActive(false);
            }
            else
            {
                ControledDeps[i].canWork = false;
                ControledDeps[i].OfficeWarning.SetActive(true);
            }
        }
        for (int i = 0; i < ControledOffices.Count; i++)
        {
            if (i < ManageValue - ControledOffices.Count)
            {
                ControledOffices[i].CanWork = true;
                ControledOffices[i].OfficeWarning.SetActive(false);
            }
            else
            {
                ControledOffices[i].CanWork = false;
                ControledOffices[i].OfficeWarning.SetActive(true);
            }
        }
    }

    public void SetName()
    {
        int num = 1;
        GC.HourEvent.AddListener(TimePass);
        if (building.Type != BuildingType.高管办公室 && building.Type != BuildingType.CEO办公室)
        {
            Text_Progress.gameObject.SetActive(true);
            SetCommaindgOfficeButton.gameObject.SetActive(true);
            OfficeWarning.SetActive(true);
            ModeChangeButton.gameObject.SetActive(false);
            ActiveButton.gameObject.SetActive(true);
            Text_SuccessRate.gameObject.SetActive(false);
            Text_OfficeMode.gameObject.SetActive(false);
            Text_TimeLeft.gameObject.SetActive(false);
            num = 1;
        }
        for (int i = 0; i < GC.CurrentOffices.Count; i++)
        {
            if (GC.CurrentOffices[i].building.Type == building.Type)
                num += 1;
        }
        Text_OfficeName.text = building.Type.ToString() + num;

    }

    public void BuildingActive()
    {
        //建筑物特效
        bool ActiveSuccess = true;
        float Posb = Random.Range(0.0f, 1.0f);
        float extra = GC.BuildingSkillSuccessExtra, extra2 = 0;
        if (extra > 0.2f)
            extra2 = extra - 0.2f;

        if (building.Type == BuildingType.人力资源部A)
        {       
            int value = 0;
            if (Posb < 0.2f - extra)
                value = 20;
            else if (Posb < 0.8f - extra2)
                value = 10;
            else
                value = 5;
            value = (int)(value * GC.HRBuildingMentalityExtra);
            DepControl d = GC.CurrentDep;
            for (int i = 0; i < d.CurrentEmps.Count; i++)
            {
                d.CurrentEmps[i].Mentality += value;
                int StarNum = Random.Range(0, 6);
                if (d.CurrentEmps[i].Stars[StarNum] < d.CurrentEmps[i].StarLimit[StarNum] * 5)
                {
                    d.CurrentEmps[i].Stars[StarNum] += 1;                   
                }
            }
        }
        else if(building.Type == BuildingType.心理咨询室)
        {
            //还没写！
        }
        else if(building.Type == BuildingType.按摩房)
        {
            List<DepControl> TDep = new List<DepControl>();
            for(int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if(building.effect.AffectedBuildings[i].Department != null)
                {
                    TDep.Add(building.effect.AffectedBuildings[i].Department);
                }
            }
            int value = 0;
            if (Posb < 0.2f - extra)
                value = 50;
            else if (Posb < 0.8f - extra2)
                value = 30;
            else
                value = 10;
            for(int a = 0; a < TDep.Count; a++)
            {
                for (int i = 0; i < TDep[a].CurrentEmps.Count; i++)
                {
                    TDep[a].CurrentEmps[i].Stamina += value;
                }
            }
            if (TDep.Count == 0)
                ActiveSuccess = false;
        }
        else if(building.Type == BuildingType.健身房)
        {
            List<DepControl> TDep = new List<DepControl>();
            for (int i = 0; i < building.effect.AffectedBuildings.Count; i++)
            {
                if (building.effect.AffectedBuildings[i].Department != null)
                {
                    TDep.Add(building.effect.AffectedBuildings[i].Department);
                }
            }
            int value = 0;
            if (Posb < 0.2f)
                value = 30;
            else if (Posb < 0.8f)
                value = 20;
            else
                value = 10;
            for (int a = 0; a < TDep.Count; a++)
            {
                for (int i = 0; i < TDep[a].CurrentEmps.Count; i++)
                {
                    TDep[a].CurrentEmps[i].Stamina += value;
                    float Posb2 = Random.Range(0.0f, 1.0f);
                    if (Posb2 < 0.3f)
                        TDep[a].CurrentEmps[i].InfoDetail.AddPerk(new Perk3(TDep[a].CurrentEmps[i]));
                }
            }
            if (TDep.Count == 0)
                ActiveSuccess = false;
        }
        else if(building.Type == BuildingType.目标修正小组)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 3)
            {
                GC.SC.TotalValue = 0;
                for (int i = 0; i < GC.SC.SelectedDices.Count; i++)
                {
                    GC.SC.SelectedDices[i].RandomValue();
                    GC.SC.TotalValue += GC.SC.SelectedDices[i].value;
                }
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.档案管理室)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 1)
            {
                GC.SC.SelectedDices[0].value += 1;
                GC.SC.SelectedDices[0].Text_Value.text = GC.SC.SelectedDices[0].value.ToString();
                GC.SC.TotalValue += 1;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.效能研究室)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building))
            {
                GC.SC.Sp1Multiply += 1;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.财务部)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 1)
            {
                GC.SC.SelectedDices[0].value -= 1;
                GC.SC.SelectedDices[0].Text_Value.text = GC.SC.SelectedDices[0].value.ToString();
                GC.SC.TotalValue -= 1;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.战略咨询部B)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 2)
            {
                GC.SC.Dices.Remove(GC.SC.SelectedDices[0]);
                GC.SC.Dices.Remove(GC.SC.SelectedDices[1]);
                Destroy(GC.SC.SelectedDices[0].gameObject);
                Destroy(GC.SC.SelectedDices[1].gameObject);
                GC.SC.SelectedDices.Clear();
                GC.SC.TotalValue = 0;
                GC.SC.CreateDice(2, 6);
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.精确标准委员会)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.SC.SelectedDices.Count == 1)
            {
                int num = GC.SC.SelectedDices[0].value;
                GC.SC.Dices.Remove(GC.SC.SelectedDices[0]);
                Destroy(GC.SC.SelectedDices[0].gameObject);
                GC.SC.SelectedDices.Clear();
                GC.SC.CreateDice(num, 1);
                GC.SC.TotalValue = 0;
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.高级财务部A)
        {
            if (GC.SC.TargetDep != null && building.effect.AffectedBuildings.Contains(GC.SC.TargetDep.building) && GC.Money > 100000)
            {
                GC.Money -= 100000;
                GC.SC.CreateDice(1);
            }
            else
                ActiveSuccess = false;
        }
        else if (building.Type == BuildingType.高级财务部B)
        {
            if (GC.Money > 100000)
            {
                GC.Money -= 100000;
                GC.CurrentEmpInfo.emp.Mentality += 15;
                GC.SelectMode = 1;
                GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
            }
            else
            {
                ActiveSuccess = false;
                GC.SelectMode = 1;
                GC.TotalEmpContent.parent.parent.gameObject.SetActive(false);
            }
        }

        if (ActiveSuccess == true)
        {
            Progress = 0;
            Text_Progress.text = "激活进度:0%";
            ActiveButton.interactable = false;
            GC.Stamina -= building.StaminaRequest;
        }
    }

    //需要选择部门的建筑物特效，无需选择就直接施加效果
    public void ShowAvailableDeps()
    {
        if (GC.Stamina >= building.StaminaRequest)
        {
            GC.CurrentOffice = this;
            GC.SelectMode = 5;
            if (building.Type == BuildingType.人力资源部A)
                GC.ShowDepSelectPanel(GC.CurrentDeps);
            else if (building.Type == BuildingType.高级财务部B)
                GC.TotalEmpContent.parent.parent.gameObject.SetActive(true);
            else
                BuildingActive();
        }
    }

    //显示所有能当上级的办公室
    public void ShowAvailableOffices()
    {
        GC.SelectMode = 8;
        GC.ShowDepSelectPanel(this);
        GC.CurrentOffice = this;
    }

    public void ShowModeSelectPanel()
    {
        GC.OfficeModeSelectPanel.SetActive(true);
        GC.CurrentOffice = this;
        if (building.Type == BuildingType.CEO办公室)
            GC.OfficeModeHireOptionButton.SetActive(true);
        else
            GC.OfficeModeHireOptionButton.SetActive(false);
    }

    public float CalcSuccessRate()
    {
        float BaseSRate = 0.6f;
        Text_SRateDetail.text = "";
        if(CurrentManager != null)
        {
            int value = 0;
            float EValue = 0;
            if (OfficeMode == 2 || OfficeMode == 4)
            {
                value = CurrentManager.HR;
                Text_SRateDetail.text += CurrentManager.Name + "人力技能:";
            }
            else if (OfficeMode == 3 || OfficeMode == 5)
            { 
                value = CurrentManager.Manage;
                Text_SRateDetail.text += CurrentManager.Name + "管理技能:";
            }
            else if (OfficeMode == 1)
            { 
                value = CurrentManager.Decision;
                Text_SRateDetail.text += CurrentManager.Name + "决策技能:";
            }
            if (value <= 5)
                EValue -= 0.15f;
            else if (value <= 9)
                EValue -= 0.05f;
            else if (value <= 13)
                EValue += 0.0f;
            else if (value <= 17)
                EValue += 0.02f;
            else if (value <= 21)
                EValue += 0.06f;
            else if (value > 21)
                EValue += 0.1f;
            BaseSRate += EValue;
            BaseSRate += CurrentManager.ExtraSuccessRate;

            //文字显示
            Text_SRateDetail.text += (EValue * 100) + "%";
            if (CurrentManager.ExtraSuccessRate > 0.001f || CurrentManager.ExtraSuccessRate < -0.001f)
                Text_SRateDetail.text += " +" + (CurrentManager.ExtraSuccessRate * 100) + "%";
        }
        return BaseSRate;
    }

    public void TimePass()
    {
        if (building.Type != BuildingType.高管办公室 && building.Type != BuildingType.CEO办公室)
        {
            if (CurrentManager != null && Progress < 100 && CanWork == true)
            {
                if (building.effectValue == 1)
                    Progress += CurrentManager.HR;
                else if (building.effectValue == 2)
                    Progress += CurrentManager.Gossip;
                else if (building.effectValue == 3)
                    Progress += CurrentManager.Strength;
                else if (building.effectValue == 4)
                    Progress += CurrentManager.Strategy;
                else if (building.effectValue == 5)
                    Progress += CurrentManager.Forecast;
                else if (building.effectValue == 6)
                    Progress += CurrentManager.Decision;
                else if (building.effectValue == 7)
                    Progress += CurrentManager.Finance;
                else if (building.effectValue == 8)
                    Progress += CurrentManager.Manage;

                if (Progress >= 100)
                {
                    Progress = 100;
                    ActiveButton.interactable = true;
                }
                Text_Progress.text = "激活进度:" + Progress + "%";
            }
        }
        else
        {
            if (CurrentManager != null)
            {
                //原本的战略充能
                CurrentManager.InfoDetail.ReChargeStrategy();

                SetOfficeUI();
                //高管（CEO）办公室的四种模式效果

                if (Progress > 0)
                {
                    Progress -= 5;
                }
                else
                {
                    Progress = 100;
                    float BaseSRate = CalcSuccessRate();
                    if (Random.Range(0.0f, 1.0f) < BaseSRate)
                        return;
                    if (OfficeMode == 1)
                    {
                        CurrentManager.InfoDetail.DirectAddStrategy();
                        GC.CreateMessage("(" + Text_OfficeName.text + "添加了新战略");
                    }
                    else if (OfficeMode == 2)
                    {
                        Employee E = null;
                        for (int i = 0; i < ControledDeps.Count; i++)
                        {
                            for (int j = 0; j < ControledDeps[i].CurrentEmps.Count; j++)
                            {
                                if (E == null)
                                    E = ControledDeps[i].CurrentEmps[j];
                                else if (E != null && ControledDeps[i].CurrentEmps[j].Mentality < E.Mentality)
                                    E = ControledDeps[i].CurrentEmps[j];
                            }
                        }
                        if (E != null)
                        {
                            E.Mentality += 20;
                            GC.CreateMessage("(" + Text_OfficeName.text + ")" + E.Name + "回复了20点心力");
                        }

                    }
                    else if (OfficeMode == 3)
                    {
                        List<Employee> E = new List<Employee>();
                        for (int i = 0; i < ControledDeps.Count; i++)
                        {
                            for (int j = 0; j < ControledDeps[i].CurrentEmps.Count; j++)
                            {
                                E.Add(ControledDeps[i].CurrentEmps[j]);
                            }
                        }
                        if (E.Count > 0)
                        {
                            Employee E2 = E[Random.Range(0, E.Count)];
                            E2.InfoDetail.AddPerk(new Perk3(E2), true);
                            GC.CreateMessage("(" + Text_OfficeName.text + ")" + E2.Name + "获得了一层启发");
                        }

                    }
                    else if (OfficeMode == 4)
                    {
                        HireType ht = new HireType(0);
                        ht.SetHeadHuntStatus();
                        GC.HC.AddHireTypes(ht);
                        GC.CreateMessage("(" + Text_OfficeName.text + ")完成了招聘");
                    }
                    else if (OfficeMode == 5)
                    {
                        if (Random.Range(0.0f, 1.0f) < 0.1f)
                            GC.BM.Lottery(3);
                        else
                            GC.BM.Lottery(2);
                        GC.CreateMessage("(" + Text_OfficeName.text + ")完成了部门研究");
                    }
                }

            }
        }

        //经验获取
        if (CurrentManager != null)
        {
            ExpTime -= 1;
            if (ExpTime == 0 && CurrentManager != null)
            {
                ExpTime = 5;
                if (building.Type == BuildingType.CEO办公室 || building.Type == BuildingType.高管办公室)
                {
                    if(OfficeMode == 1)
                        CurrentManager.GainExp(250, 10);
                    else if(OfficeMode == 2)
                        CurrentManager.GainExp(250, 13);
                    else if (OfficeMode == 3)
                        CurrentManager.GainExp(250, 7);
                    else if (OfficeMode == 4)
                        CurrentManager.GainExp(250, 8);
                    else if (OfficeMode == 5)
                        CurrentManager.GainExp(250, 11);
                }

                else if (building.Type == BuildingType.目标修正小组)
                    CurrentManager.GainExp(50, 13);
                else if (building.Type == BuildingType.高级财务部A)
                    CurrentManager.GainExp(50, 14);
                else if (building.Type == BuildingType.精确标准委员会)
                    CurrentManager.GainExp(50, 15);
                else if (building.Type == BuildingType.高级财务部B)
                    CurrentManager.GainExp(50, 10);
                else if (building.Type == BuildingType.人力资源部A)
                    CurrentManager.GainExp(50, 11);
                else if (building.Type == BuildingType.效能研究室)
                    CurrentManager.GainExp(50, 12);
                else if (building.Type == BuildingType.人力资源部B)
                    CurrentManager.GainExp(50, 8);
                else if (building.Type == BuildingType.财务部)
                    CurrentManager.GainExp(50, 9);
                else if (building.Type == BuildingType.档案管理室)
                    CurrentManager.GainExp(50, 4);
                else if (building.Type == BuildingType.按摩房)
                    CurrentManager.GainExp(50, 5);
                else if (building.Type == BuildingType.健身房)
                    CurrentManager.GainExp(50, 6);
            }
        }
    }

    public void SetOfficeUI()
    {
        if (CurrentManager == null)
        {
            Text_SuccessRate.text = "成功率:----";
            Text_TimeLeft.text = "剩余时间:----";
        }
        else
        {
            float BaseSRate = CalcSuccessRate();
            Text_SuccessRate.text = "成功率:" + (BaseSRate * 100) + "%";
            Text_TimeLeft.text = "剩余时间:" + (Progress / 5) + "时";
        }
    }

    public void ClearOffice()
    {
        if(CurrentManager != null)
        {
            GC.CurrentEmpInfo = CurrentManager.InfoA;
            GC.CurrentEmpInfo.transform.parent = GC.StandbyContent;
            GC.ResetOldAssignment();
        }
        for(int i = 0; i < ControledDeps.Count; i++)
        {
            ControledDeps[i].CommandingOffice = null;
        }
        GC.HourEvent.RemoveListener(TimePass);
        GC.CurrentOffices.Remove(this);
        if (SRateDetailPanel != null)
            Destroy(SRateDetailPanel.gameObject);
        Destroy(DS.gameObject);
        Destroy(this.gameObject);
    }

    public void ShowSRateDetailPanel()
    {
        CalcSuccessRate();
        SRateDetailPanel.transform.position = Input.mousePosition;
        SRateDetailPanel.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(SRateDetailPanel.gameObject.GetComponent<RectTransform>());
    }

    public void CloseSRateDetailPanel()
    {
        SRateDetailPanel.gameObject.SetActive(false);
    }
}
