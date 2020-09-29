﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 员工类的临时管理器
/// </summary>
public class EmpManager : MonoBehaviour
{
    private static EmpManager Instance;
    private GameObject empPrefabs;
    private Material[] empMaterials;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        empPrefabs = ResourcesLoader.LoadPrefab("Prefabs/Employee/Emp");
        empMaterials = ResourcesLoader.LoadAll<Material>("Image/Employee/Materials");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            CreateEmp(BuildingManage.Instance.AimingPosition);
        }
    }

    public void CreateEmp(Vector3 position)
    {
        EmpEntity emp = GameObject.Instantiate(empPrefabs, position, Quaternion.identity).GetComponentInChildren<EmpEntity>();
        emp.Init();
        emp.Renderer.material = empMaterials[Random.Range(0, empMaterials.Length)];
    }
}
