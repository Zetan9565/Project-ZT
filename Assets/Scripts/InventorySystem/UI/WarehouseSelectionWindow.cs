using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarehouseSelectionWindow : Window
{
    [SerializeField]
    private WarehouseList list;
    [SerializeField]
    private Button confirm;
    [SerializeField]
    private Button deselect;

    public float defaultCheckRange;
    public LayerMask defaultCheckLayer;

    private bool noneCheck;
    private Action<IWarehouseKeeper> onConfirm;
    private Action onCancel;
    private IWarehouseKeeper selected;
    private bool doConfirm;

    public static WarehouseSelectionWindow StartSelection(Action<IWarehouseKeeper> selectCallback, bool noneCheck, bool physics2D, Vector3 point, float? range = null, LayerMask? layer = null)
    {
        return StartSelection(selectCallback, null, null, noneCheck, physics2D, point, range, layer);
    }
    public static WarehouseSelectionWindow StartSelection(Action<IWarehouseKeeper> selectCallback, Action cancelCallback,
        bool noneCheck, bool physics2D, Vector3 point, float? range = null, LayerMask? layer = null)
    {
        return StartSelection(selectCallback, cancelCallback, null, noneCheck, physics2D, point, range, layer);
    }
    public static WarehouseSelectionWindow StartSelection(Action<IWarehouseKeeper> selectCallback, Predicate<IWarehouseKeeper> defaultSelector,
        bool noneCheck, bool physics2D, Vector3 point, float? range = null, LayerMask? layer = null)
    {
        return StartSelection(selectCallback, null, defaultSelector, noneCheck, physics2D, point, range, layer);
    }
    public static WarehouseSelectionWindow StartSelection(Action<IWarehouseKeeper> selectCallback, Action cancelCallback, Predicate<IWarehouseKeeper> defaultSelector,
        bool noneCheck, bool physics2D, Vector3 point, float? range = null, LayerMask? layer = null)
    {
        return WindowsManager.OpenWindow<WarehouseSelectionWindow>(selectCallback, cancelCallback, defaultSelector, noneCheck, physics2D, point, range, layer);
    }

    protected override bool OnOpen(params object[] args)
    {
        if (args == null || args.Length < 7) return false;
        List<IWarehouseKeeper> warehouses = new List<IWarehouseKeeper>();
        var par = (selectCallback: args[0] as Action<IWarehouseKeeper>, cancelCallback: args[1] as Action, defaultSelector: args[2] as Predicate<IWarehouseKeeper>,
            noneCheck: (bool)args[3], physics2D: (bool)args[4], point: (Vector3)args[5], range: args[6] as float?, layer: args[7] as LayerMask?);
        onConfirm = par.selectCallback;
        onCancel = par.cancelCallback;
        noneCheck = par.noneCheck;
        if (par.physics2D)
        {
            var colliders = Physics2D.OverlapCircleAll(par.point, par.range ?? defaultCheckRange, par.layer ?? defaultCheckLayer);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<Warehouse>(out var warehouse))
                    warehouses.Add(warehouse.WData);
                else if (collider.TryGetComponent<Talker>(out var talker))
                {
                    var td = talker.GetData<TalkerData>();
                    if (td.Info.IsWarehouseAgent)
                        warehouses.Add(td);
                }
            }
        }
        else
        {
            var colliders = Physics.OverlapSphere(par.point, par.range ?? defaultCheckRange, par.layer ?? defaultCheckLayer);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<Warehouse>(out var warehouse))
                    warehouses.Add(warehouse.WData);
                else if (collider.TryGetComponent<Talker>(out var talker))
                {
                    var td = talker.GetData<TalkerData>();
                    if (td.Info.IsWarehouseAgent)
                        warehouses.Add(td);
                }
            }
        }
        if (warehouses.Count < 1)
        {
            MessageManager.Instance.New("附近没有可用的仓库");
            return false;
        }
        list.Refresh(warehouses);
        list.SelectIf(par.defaultSelector);
        deselect.interactable = list.SelectedIndices.Count > 0;
        doConfirm = false;
        return true;
    }

    protected override void OnAwake()
    {
        list.SetSelectCallback(OnSelected);
        confirm.onClick.AddListener(Confirm);
        deselect.onClick.AddListener(Deselect);
        base.OnAwake();
    }

    private void OnSelected(WarehouseAgent warehouse)
    {
        if (warehouse.IsSelected) selected = warehouse.Data;
        else if (warehouse.Data == selected) selected = null;
        deselect.interactable = list.SelectedIndices.Count > 0;
    }

    private void Deselect()
    {
        list.DeselectAll();
    }

    private void Confirm()
    {
        if (!noneCheck || selected != null)
        {
            onConfirm?.Invoke(selected);
            doConfirm = true;
            Close();
        }
        else MessageManager.Instance.New("未选择仓库");
    }

    protected override bool OnClose(params object[] args)
    {
        onConfirm = null;
        selected = null;
        noneCheck = false;
        if (!doConfirm) onCancel?.Invoke();
        return base.OnClose(args);
    }

    protected override void RegisterNotify()
    {

    }
}
