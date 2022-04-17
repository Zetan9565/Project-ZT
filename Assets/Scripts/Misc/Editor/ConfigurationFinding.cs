using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConfigurationFinding : EditorWindow
{
    private Vector2 scrollPos = Vector2.zero;

    private string[] strKeys;
    private int[] intKeys;
    private bool[] boolKeys;
    private UnityEngine.Object[] objKeys;
    private bool canSeek;
    private Action seekCallback;

    private bool needInit;
    private int barIndex;
    private SeekType type;

    private List<SearchResult> results = new List<SearchResult>();

    [MenuItem("Zetan Studio/工具/查找配置")]
    public static void CreateWindow()
    {
        ConfigurationFinding window = GetWindow<ConfigurationFinding>("查找配置文件");
        window.Show();
    }

    private void OnEnable()
    {
        needInit = true;
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        int oldIndex = barIndex;
        barIndex = EditorGUILayout.Popup(new GUIContent("查找对象"), barIndex, new string[] { "道具", "任务", "对话", "配方", "建筑", "掉落产出" });
        if (oldIndex != barIndex)
        {
            needInit = true;
            results.Clear();
            seekCallback = null;
        }
        switch (barIndex)
        {
            case 0:
                ItemView();
                break;
            case 1:
                QuestView();
                break;
            case 2:
                DialogueView();
                break;
            case 3:
                FormulationView();
                break;
            case 4:
                BuildingView();
                break;
            case 5:
                ProductView();
                break;
            default:
                break;
        }
        #region 查找结果
        if (canSeek)
        {
            if (GUILayout.Button("查找"))
            {
                results.Clear();
                seekCallback?.Invoke();
                Debug.Log($"查找结束，共找到个{results.Count}结果");
            }
        }
        if (results.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("清空"))
                results.Clear();
            EditorGUILayout.LabelField("查找结果：", $"共 {results.Count} 个");
            EditorGUILayout.EndHorizontal();
            GUI.enabled = false;
            for (int i = 0; i < results.Count; i++)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(new GUIContent(string.Empty), results[i].find, typeof(ScriptableObject), true);
                EditorGUILayout.LabelField(results[i].remark);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField($"路径：{AssetDatabase.GetAssetPath(results[i].find)}");
                EditorGUILayout.EndVertical();
            }
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.LabelField("暂无查找结果");
        }
        #endregion
        GUILayout.EndScrollView();
        needInit = false;
    }

    #region 道具查找
    private ItemKeyType itemType;
    private enum ItemKeyType
    {
        [InspectorName("ID")]
        ID,
        [InspectorName("名称")]
        Name,
        [InspectorName("描述")]
        Desc,
        [InspectorName("道具类型")]
        Type,
        [InspectorName("作为材料时的类型")]
        AsMater,
        [InspectorName("配方")]
        Formu,
        [InspectorName("配方材料")]
        Mater,
        [InspectorName("配方材料类型")]
        MatType,
    }
    private void ItemView()
    {
        var oldType = itemType;
        itemType = (ItemKeyType)EditorGUILayout.EnumPopup(new GUIContent("查找内容"), itemType);
        if (needInit || oldType != itemType)
        {
            strKeys = new string[] { string.Empty };
            intKeys = new int[1] { 0 };
            objKeys = new ScriptableObject[1];
            results.Clear();
        }
        switch (itemType)
        {
            case ItemKeyType.ID:
            case ItemKeyType.Name:
            case ItemKeyType.Desc:
                type = (SeekType)EditorGUILayout.EnumPopup(new GUIContent("查找方式"), type);
                strKeys[0] = EditorGUILayout.TextField(new GUIContent("关键字"), strKeys[0]);
                canSeek = !string.IsNullOrEmpty(strKeys[0]);
                break;
            case ItemKeyType.Type:
                intKeys[0] = (int)(ItemType)EditorGUILayout.EnumPopup("道具类型", (ItemType)intKeys[0]);
                canSeek = true;
                break;
            case ItemKeyType.AsMater:
                intKeys[0] = (int)(MaterialType)EditorGUILayout.EnumPopup("材料类型", (MaterialType)intKeys[0]);
                canSeek = true;
                break;
            case ItemKeyType.Formu:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("关联配方"), objKeys[0], typeof(Formulation), false);
                canSeek = objKeys[0];
                break;
            case ItemKeyType.Mater:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("包含道具"), objKeys[0], typeof(ItemBase), false);
                canSeek = objKeys[0];
                break;
            case ItemKeyType.MatType:
                intKeys[0] = (int)(MaterialType)EditorGUILayout.EnumPopup("包含类型", (MaterialType)intKeys[0]);
                canSeek = true;
                break;
            default:
                break;
        }
        seekCallback = SeekItem;
    }
    private void SeekItem()
    {
        var items = Resources.LoadAll<ItemBase>("Configuration");
        foreach (var item in items)
        {
            bool take = false;
            string remark = string.Empty;
            switch (itemType)
            {
                case ItemKeyType.ID:
                    if (CompareString(item.ID, strKeys[0]))
                    {
                        take = true;
                        remark = $"识别码：{item.ID}";
                    }
                    break;
                case ItemKeyType.Name:
                    if (CompareString(item.Name, strKeys[0]))
                    {
                        take = true;
                        remark = $"名称：{item.Name}";
                    }
                    break;
                case ItemKeyType.Desc:
                    if (CompareString(item.Description, strKeys[0]))
                    {
                        take = true;
                        if (strKeys[0].Length > 16 || type == SeekType.Equals)
                            remark = strKeys[0];
                        else
                        {
                            remark = "描述：" + TrimContentByKey(item.Description, strKeys[0], 16);
                        }
                    }
                    break;
                case ItemKeyType.Type:
                    if (item.ItemType == (ItemType)intKeys[0])
                    {
                        take = true;
                        remark = $"[名称：{item.Name}]是此类型";
                    }
                    break;
                case ItemKeyType.AsMater:
                    if (item.MaterialType == (MaterialType)intKeys[0])
                    {
                        take = true;
                        remark = $"[名称：{item.Name}]是此材料类型";
                    }
                    break;
                case ItemKeyType.Formu:
                    if (item.Formulation == objKeys[0])
                    {
                        take = true;
                        remark = $"[名称：{item.Name}]使用该配方";
                    }
                    break;
                case ItemKeyType.Mater:
                    if (item.Formulation)
                    {
                        int index = item.Formulation.Materials.FindIndex(x => x.MakingType == MakingType.SingleItem && x.Item == objKeys[0]);
                        if (index >= 0)
                        {
                            take = true;
                            remark = $"第[{index + 1}]个配方材料是该道具";
                        }
                    }
                    break;
                case ItemKeyType.MatType:
                    if (item.Formulation)
                    {
                        int index = item.Formulation.Materials.FindIndex(x => x.MakingType == MakingType.SameType && x.MaterialType == (MaterialType)intKeys[0]);
                        if (index >= 0)
                        {
                            take = true;
                            remark = $"第[{index + 1}]个配方材料是此类型";
                        }
                    }
                    break;
                default:
                    take = false;
                    break;
            }
            if (take) results.Add(new SearchResult(item, remark));
        }
    }
    #endregion

    #region 任务查找
    private QuestKeyType questType;
    private enum QuestKeyType
    {
        [InspectorName("ID")]
        ID,
        [InspectorName("标题")]
        Title,
        [InspectorName("描述")]
        Desc,
        [InspectorName("奖励")]
        Reward,
        [InspectorName("目标标题")]
        ObjTitle,
        [InspectorName("目标道具")]
        ObjItem,
        [InspectorName("目标谈话人")]
        ObjTalker,
    }
    private void QuestView()
    {
        var oldType = questType;
        questType = (QuestKeyType)EditorGUILayout.EnumPopup(new GUIContent("查找内容"), questType);
        if (needInit || oldType != questType)
        {
            strKeys = new string[] { string.Empty };
            objKeys = new ScriptableObject[1];
            results.Clear();
        }
        switch (questType)
        {
            case QuestKeyType.ID:
            case QuestKeyType.Title:
            case QuestKeyType.Desc:
            case QuestKeyType.ObjTitle:
                type = (SeekType)EditorGUILayout.EnumPopup(new GUIContent("查找方式"), type);
                strKeys[0] = EditorGUILayout.TextField(new GUIContent("关键字"), strKeys[0]);
                canSeek = !string.IsNullOrEmpty(strKeys[0]);
                break;
            case QuestKeyType.Reward:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("包含道具"), objKeys[0], typeof(ItemBase), false);
                canSeek = objKeys[0];
                break;
            case QuestKeyType.ObjItem:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("关联道具"), objKeys[0], typeof(ItemBase), false);
                canSeek = objKeys[0];
                break;
            case QuestKeyType.ObjTalker:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("谈话人"), objKeys[0], typeof(TalkerInformation), false);
                canSeek = objKeys[0];
                break;
            default:
                break;
        }
        seekCallback = SeekQuest;
    }
    private void SeekQuest()
    {
        var quests = Resources.LoadAll<Quest>("Configuration");
        foreach (var quest in quests)
        {
            bool take = false;
            string remark = string.Empty;
            switch (questType)
            {
                case QuestKeyType.ID:
                    if (CompareString(quest.ID, strKeys[0]))
                    {
                        take = true;
                        remark = $"识别码：{quest.ID}";
                    }
                    break;
                case QuestKeyType.Title:
                    if (CompareString(quest.Title, strKeys[0]))
                    {
                        take = true;
                        remark = $"标题：{quest.Title}";
                    }
                    break;
                case QuestKeyType.Desc:
                    if (CompareString(quest.Description, strKeys[0]))
                    {
                        take = true;
                        if (strKeys[0].Length > 16 || type == SeekType.Equals)
                            remark = strKeys[0];
                        else
                        {
                            remark = "描述：" + TrimContentByKey(quest.Description, strKeys[0], 16);
                        }
                    }
                    break;
                case QuestKeyType.Reward:
                    if (quest.RewardItems.Exists(x => x.item == objKeys[0]))
                        take = true;
                    break;
                case QuestKeyType.ObjTitle:
                    #region 对比标题目标
                    int index = quest.Objectives.FindIndex(obj => CompareString(obj.DisplayName, strKeys[0]));
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个目标";
                        break;
                    }
                    #endregion
                    break;
                case QuestKeyType.ObjItem:
                    #region 对比目标道具
                    index = quest.Objectives.FindIndex(obj => obj is CollectObjective co && co.ItemToCollect == objKeys[0] ||
                                                       obj is SubmitObjective so && so.ItemToSubmit == objKeys[0] ||
                                                       obj is MoveObjective mo && mo.ItemToUseHere == objKeys[0]);
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个目标";
                        break;
                    }
                    #endregion
                    break;
                case QuestKeyType.ObjTalker:
                    #region 对比目标谈话人
                    index = quest.Objectives.FindIndex(obj => obj is TalkObjective to && to.NPCToTalk == objKeys[0] ||
                                   obj is SubmitObjective so && so.NPCToSubmit == objKeys[0]);
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个目标";
                        break;
                    }
                    #endregion
                    break;
                default:
                    take = false;
                    break;
            }
            if (take) results.Add(new SearchResult(quest, remark));
        }
    }
    #endregion

    #region 对话查找
    private DialogueKeyType dialogueType;
    private enum DialogueKeyType
    {
        [InspectorName("ID")]
        ID,
        [InspectorName("语句")]
        Words,
        [InspectorName("语句对话人")]
        WordsSayer,
        [InspectorName("统一对话人")]
        UniNPC,
        [InspectorName("选项标题")]
        OptTitle,
        [InspectorName("分支语句")]
        BraWords,
        [InspectorName("分支语句对话人")]
        BrWoSayer,
        [InspectorName("分支对话")]
        BraDialog,
    }
    private void DialogueView()
    {
        var oldType = dialogueType;
        dialogueType = (DialogueKeyType)EditorGUILayout.EnumPopup(new GUIContent("查找内容"), dialogueType);
        if (needInit || oldType != dialogueType)
        {
            strKeys = new string[] { string.Empty };
            boolKeys = new bool[] { false };
            objKeys = new ScriptableObject[1];
            results.Clear();
        }
        switch (dialogueType)
        {
            case DialogueKeyType.ID:
            case DialogueKeyType.Words:
            case DialogueKeyType.OptTitle:
            case DialogueKeyType.BraWords:
                type = (SeekType)EditorGUILayout.EnumPopup(new GUIContent("查找方式"), type);
                strKeys[0] = EditorGUILayout.TextField(new GUIContent("关键字"), strKeys[0]);
                canSeek = !string.IsNullOrEmpty(strKeys[0]);
                break;
            case DialogueKeyType.WordsSayer:
            case DialogueKeyType.BrWoSayer:
                boolKeys[0] = EditorGUILayout.Toggle("玩家说", boolKeys[0]);
                if (!boolKeys[0])
                {
                    objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("对话人"), objKeys[0], typeof(TalkerInformation), false);
                    canSeek = objKeys[0];
                }
                else canSeek = true;
                break;
            case DialogueKeyType.UniNPC:
                boolKeys[0] = EditorGUILayout.Toggle("使用当前对话人", boolKeys[0]);
                if (!boolKeys[0])
                {
                    objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("统一对话人"), objKeys[0], typeof(TalkerInformation), false);
                    canSeek = objKeys[0];
                }
                else canSeek = true;
                break;
            case DialogueKeyType.BraDialog:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("使用对话"), objKeys[0], typeof(Dialogue), false);
                canSeek = objKeys[0];
                break;
            default:
                break;
        }
        seekCallback = SeekDialogue;
    }
    private void SeekDialogue()
    {
        Dialogue[] dialogues = Resources.LoadAll<Dialogue>("Configuration");
        foreach (var dialog in dialogues)
        {
            bool take = false;
            string remark = string.Empty;
            switch (dialogueType)
            {
                case DialogueKeyType.ID:
                    if (CompareString(dialog.ID, strKeys[0]))
                    {
                        take = true;
                        remark = $"识别码：{dialog.ID}";
                    }
                    break;
                case DialogueKeyType.Words:
                    int index = dialog.Words.FindIndex(x => CompareString(x.Content, strKeys[0]));
                    if (index >= 0)
                    {
                        take = true;
                        if (strKeys[0].Length > 16 || type == SeekType.Equals)
                            remark = strKeys[0];
                        else
                        {
                            remark = $"第[{index + 1}]句话：{TrimContentByKey(dialog.Words[index].Content, strKeys[0], 16)}";
                        }
                    }
                    break;
                case DialogueKeyType.WordsSayer:
                    if (boolKeys[0])
                    {
                        index = dialog.Words.FindIndex(x => x.TalkerType == TalkerType.Player);
                        if (index >= 0)
                        {
                            take = true;
                            remark = $"第[{index + 1}]句话是[玩家]说";
                        }
                    }
                    else
                    {
                        if (dialog.UseUnifiedNPC)
                        {
                            if (dialog.UnifiedNPC == objKeys[0])
                            {
                                take = true;
                                remark = $"统一对话人是[{dialog.UnifiedNPC.Name}]";
                            }
                        }
                        else
                        {
                            index = dialog.Words.FindIndex(x => x.TalkerInfo == objKeys[0]);
                            if (index >= 0)
                            {
                                take = true;
                                remark = $"第[{index + 1}]句话是[{dialog.Words[index].TalkerName}]说";
                            }
                        }
                    }
                    break;
                case DialogueKeyType.UniNPC:
                    if (!boolKeys[0])
                    {
                        if (dialog.UseUnifiedNPC && !dialog.UseCurrentTalkerInfo)
                        {
                            if (dialog.UnifiedNPC == objKeys[0])
                            {
                                take = true;
                                remark = $"统一对话人是[{dialog.UnifiedNPC.Name}]";
                            }
                        }
                    }
                    else
                    {
                        if (dialog.UseCurrentTalkerInfo)
                        {
                            take = true;
                            remark = $"识别码：{dialog.ID}";
                        }
                    }
                    break;
                case DialogueKeyType.OptTitle:
                    index = dialog.Words.FindIndex(x => x.Options.Exists(y => CompareString(y.Title, strKeys[0])));
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]句话";
                        index = dialog.Words[index].Options.FindIndex(x => CompareString(x.Title, strKeys[0]));
                        remark += $"第[{index + 1}]个选项";
                    }
                    break;
                case DialogueKeyType.BraWords:
                    index = dialog.Words.FindIndex(x => x.Options.Exists(y => CompareString(y.Words, strKeys[0])));
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]句话";
                        index = dialog.Words[index].Options.FindIndex(x => CompareString(x.Words, strKeys[0]));
                        remark += $"第[{index + 1}]个选项";
                    }
                    break;
                case DialogueKeyType.BrWoSayer:
                    if (boolKeys[0])
                    {
                        index = dialog.Words.FindIndex(x => x.Options.Exists(y => y.TalkerType == TalkerType.Player));
                        if (index >= 0)
                        {
                            take = true;
                            remark = $"第[{index + 1}]句话";
                            index = dialog.Words[index].Options.FindIndex(x => x.TalkerType == TalkerType.Player);
                            remark += $"第[{index + 1}]个选项是[玩家]说";
                        }
                    }
                    else
                    {
                        index = -1;
                        if (dialog.UseUnifiedNPC && dialog.UnifiedNPC == objKeys[0])
                            index = dialog.Words.FindIndex(x => x.Options.Exists(y => y.TalkerType == TalkerType.NPC));
                        else if (!dialog.UseUnifiedNPC)
                            index = dialog.Words.FindIndex(x => x.TalkerType == TalkerType.NPC && x.Options.Exists(y => y.TalkerType == TalkerType.NPC));
                        if (index >= 0)
                        {
                            take = true;
                            remark = $"第[{index + 1}]句话";
                            index = dialog.Words[index].Options.FindIndex(x => x.TalkerType == TalkerType.NPC);
                            remark += $"第[{index + 1}]个选项是[{(objKeys[0] as TalkerInformation).Name}]说";
                        }
                    }
                    break;
                case DialogueKeyType.BraDialog:
                    index = dialog.Words.FindIndex(x => x.Options.Exists(y => y.Dialogue == objKeys[0]));
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]句话";
                        index = dialog.Words[index].Options.FindIndex(x => x.Dialogue == objKeys[0]);
                        remark += $"第[{index + 1}]个选项";
                    }
                    break;
                default:
                    take = false;
                    break;
            }
            if (take) results.Add(new SearchResult(dialog, remark));
        }
    }
    #endregion

    #region 配方查找
    private FormulationKeyType formulationType;
    private enum FormulationKeyType
    {
        [InspectorName("备注")]
        Remark,
        [InspectorName("配方材料")]
        Mater,
        [InspectorName("配方材料类型")]
        MatType,
    }
    private void FormulationView()
    {
        var oldType = formulationType;
        formulationType = (FormulationKeyType)EditorGUILayout.EnumPopup(new GUIContent("查找内容"), formulationType);
        if (needInit || oldType != formulationType)
        {
            strKeys = new string[] { string.Empty };
            intKeys = new int[1] { 0 };
            objKeys = new ScriptableObject[1];
            results.Clear();
        }
        switch (formulationType)
        {
            case FormulationKeyType.Remark:
                type = (SeekType)EditorGUILayout.EnumPopup(new GUIContent("查找方式"), type);
                strKeys[0] = EditorGUILayout.TextField(new GUIContent("关键字"), strKeys[0]);
                canSeek = !string.IsNullOrEmpty(strKeys[0]);
                break;
            case FormulationKeyType.Mater:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("包含道具"), objKeys[0], typeof(ItemBase), false);
                canSeek = objKeys[0];
                break;
            case FormulationKeyType.MatType:
                intKeys[0] = (int)(MaterialType)EditorGUILayout.EnumPopup("包含类型", (MaterialType)intKeys[0]);
                canSeek = true;
                break;
            default:
                break;
        }
        seekCallback = SeekFormulation;
    }
    private void SeekFormulation()
    {
        var formulations = Resources.LoadAll<Formulation>("Configuration");
        var field = typeof(Formulation).GetField("remark", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        foreach (var formulation in formulations)
        {
            bool take = false;
            string remark = string.Empty;
            switch (formulationType)
            {
                case FormulationKeyType.Remark:
                    string fRemark = field.GetValue(formulation).ToString();
                    if (CompareString(fRemark, strKeys[0]))
                    {
                        take = true;
                        remark = $"备注：{TrimContentByKey(fRemark, strKeys[0], 10)}";
                    }
                    break;
                case FormulationKeyType.Mater:
                    int index = formulation.Materials.FindIndex(x => x.MakingType == MakingType.SingleItem && x.Item == objKeys[0]);
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个材料是该道具";
                    }
                    break;
                case FormulationKeyType.MatType:
                    index = formulation.Materials.FindIndex(x => x.MakingType == MakingType.SameType && x.MaterialType == (MaterialType)intKeys[0]);
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个材料是此类型";
                    }
                    break;
                default:
                    break;
            }
            if (take) results.Add(new SearchResult(formulation, remark));
        }
    }
    #endregion

    #region 建筑查找
    private BuildingKeyType buildingType;
    private enum BuildingKeyType
    {
        [InspectorName("ID前缀")]
        ID,
        [InspectorName("名称")]
        Name,
        [InspectorName("描述")]
        Desc,
        [InspectorName("配方")]
        Formu,
        [InspectorName("配方材料")]
        Mater,
        [InspectorName("配方材料类型")]
        MatType,
        [InspectorName("预制件")]
        Prefab,
        [InspectorName("预览预制件")]
        Preview,
    }
    private void BuildingView()
    {
        var oldType = buildingType;
        buildingType = (BuildingKeyType)EditorGUILayout.EnumPopup(new GUIContent("查找内容"), buildingType);
        if (needInit || oldType != buildingType)
        {
            strKeys = new string[] { string.Empty };
            intKeys = new int[1] { 0 };
            switch (buildingType)
            {
                case BuildingKeyType.Prefab:
                    objKeys = new Building2D[1];
                    break;
                case BuildingKeyType.Preview:
                    objKeys = new BuildingPreview2D[1];
                    break;
                default:
                    objKeys = new ScriptableObject[1];
                    break;
            }
            results.Clear();
        }
        switch (buildingType)
        {
            case BuildingKeyType.ID:
            case BuildingKeyType.Name:
            case BuildingKeyType.Desc:
                type = (SeekType)EditorGUILayout.EnumPopup(new GUIContent("查找方式"), type);
                strKeys[0] = EditorGUILayout.TextField(new GUIContent("关键字"), strKeys[0]);
                canSeek = !string.IsNullOrEmpty(strKeys[0]);
                break;
            case BuildingKeyType.Formu:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("关联配方"), objKeys[0], typeof(Formulation), false);
                canSeek = objKeys[0];
                break;
            case BuildingKeyType.Mater:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("包含道具"), objKeys[0], typeof(ItemBase), false);
                canSeek = objKeys[0];
                break;
            case BuildingKeyType.MatType:
                intKeys[0] = (int)(MaterialType)EditorGUILayout.EnumPopup("包含类型", (MaterialType)intKeys[0]);
                canSeek = true;
                break;
            case BuildingKeyType.Prefab:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("预制件"), objKeys[0], typeof(Building2D), false);
                canSeek = objKeys[0];
                break;
            case BuildingKeyType.Preview:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("预制件"), objKeys[0], typeof(BuildingPreview2D), false);
                canSeek = objKeys[0];
                break;
            default:
                break;
        }
        seekCallback = SeekBuilding;
    }
    private void SeekBuilding()
    {
        var buildings = Resources.LoadAll<BuildingInformation>("Configuration");
        foreach (var building in buildings)
        {
            bool take = false;
            string remark = string.Empty;
            switch (buildingType)
            {
                case BuildingKeyType.ID:
                    if (CompareString(building.ID, strKeys[0]))
                    {
                        take = true;
                        remark = $"识别码前缀：{building.ID}";
                    }
                    break;
                case BuildingKeyType.Name:
                    if (CompareString(building.Name, strKeys[0]))
                    {
                        take = true;
                        remark = $"名称：{building.Name}";
                    }
                    break;
                case BuildingKeyType.Desc:
                    if (CompareString(building.Description, strKeys[0]))
                    {
                        take = true;
                        remark = $"描述：{TrimContentByKey(building.Description, strKeys[0], 16)}";
                    }
                    break;
                case BuildingKeyType.Formu:
                    int index = building.Stages.FindIndex(x => x.Formulation == objKeys[0]);
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个阶段使用该配方";
                    }
                    break;
                case BuildingKeyType.Mater:
                    index = building.Stages.FindIndex(x => x.Formulation && x.Formulation.Materials.Exists(y => y.MakingType == MakingType.SingleItem && y.Item == objKeys[0]));
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个阶段的第[{building.Stages[index].Formulation.Materials.FindIndex(x => x.MakingType == MakingType.SingleItem && x.Item == objKeys[0]) + 1}]个材料是该道具";
                    }
                    break;
                case BuildingKeyType.MatType:
                    index = building.Stages.FindIndex(x => x.Formulation && x.Formulation.Materials.Exists(y => y.MakingType == MakingType.SameType && y.MaterialType == (MaterialType)intKeys[0]));
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个阶段的第[{building.Stages[index].Formulation.Materials.FindIndex(x => x.MakingType == MakingType.SameType && x.MaterialType == (MaterialType)intKeys[0]) + 1}]个材料是该类型";
                    }
                    break;
                case BuildingKeyType.Prefab:
                    if (building.Prefab == objKeys[0])
                    {
                        take = true;
                        remark = "使用该预制件";
                    }
                    break;
                case BuildingKeyType.Preview:
                    if (building.Preview == objKeys[0])
                    {
                        take = true;
                        remark = "使用该预制件作为预览";
                    }
                    break;
                default:
                    break;
            }
            if (take) results.Add(new SearchResult(building, remark));
        }
    }
    #endregion

    #region 产出查找
    private ProductKeyType productType;
    private enum ProductKeyType
    {
        [InspectorName("备注")]
        Remark,
        [InspectorName("产出道具")]
        Item,
    }
    private void ProductView()
    {
        var oldType = productType;
        productType = (ProductKeyType)EditorGUILayout.EnumPopup(new GUIContent("查找内容"), productType);
        if (needInit || oldType != productType)
        {
            strKeys = new string[] { string.Empty };
            objKeys = new ScriptableObject[1];
            results.Clear();
        }
        switch (productType)
        {
            case ProductKeyType.Remark:
                type = (SeekType)EditorGUILayout.EnumPopup(new GUIContent("查找方式"), type);
                strKeys[0] = EditorGUILayout.TextField(new GUIContent("关键字"), strKeys[0]);
                canSeek = !string.IsNullOrEmpty(strKeys[0]);
                break;
            case ProductKeyType.Item:
                objKeys[0] = EditorGUILayout.ObjectField(new GUIContent("包含道具"), objKeys[0], typeof(ItemBase), false);
                canSeek = objKeys[0];
                break;
            default:
                break;
        }
        seekCallback = SeekProduct;
    }
    private void SeekProduct()
    {
        var products = Resources.LoadAll<ProductInformation>("Configuration");
        var field = typeof(ProductInformation).GetField("remark", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        foreach (var product in products)
        {
            bool take = false;
            string remark = string.Empty;
            switch (productType)
            {
                case ProductKeyType.Remark:
                    string pRemark = field.GetValue(product).ToString();
                    if (CompareString(pRemark, strKeys[0]))
                    {
                        take = true;
                        remark = $"备注：{TrimContentByKey(pRemark, strKeys[0], 10)}";
                    }
                    break;
                case ProductKeyType.Item:
                    int index = product.Products.FindIndex(x => x.Item == objKeys[0]);
                    if (index >= 0)
                    {
                        take = true;
                        remark = $"第[{index + 1}]个产出是该道具";
                    }
                    break;
                default:
                    break;
            }
            if (take) results.Add(new SearchResult(product, remark));
        }
    }
    #endregion

    #region 工具
    private bool CompareString(string input, string key)
    {
        switch (type)
        {
            case SeekType.Contains:
                return input.Contains(key);
            case SeekType.Equals:
                return input == key;
            default:
                return false;
        }
    }
    private string TrimContentByKey(string input, string key, int length)
    {
        string output;
        int cut = (length - key.Length) / 2;
        int index = input.IndexOf(key);
        int start = index - cut;
        int end = index + key.Length + cut;
        while (start < 0)
        {
            start++;
            if (end < input.Length - 1) end++;
        }
        while (end > input.Length - 1)
        {
            end--;
            if (start > 0) start--;
        }
        start = start < 0 ? 0 : start;
        end = end > input.Length - 1 ? input.Length - 1 : end;
        int len = end - start + 1;
        output = input.Substring(start, Mathf.Min(len, input.Length - start));
        index = output.IndexOf(key);
        output = output.Insert(index, "<").Insert(index + 1 + key.Length, ">");
        return output;
    }

    private enum SeekType
    {
        [InspectorName("包含")]
        Contains,
        [InspectorName("等于")]
        Equals
    }

    private class SearchResult
    {
        public readonly ScriptableObject find;
        public readonly string remark;

        public SearchResult(ScriptableObject result, string remark)
        {
            this.find = result;
            this.remark = remark;
        }
    }
    #endregion
}