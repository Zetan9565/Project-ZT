using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
//[AddComponentMenu("Zetan Studio/管理器/技能管理器")]
public class SkillManager : SingletonMonoBehaviour<SkillManager>
{
    //[SerializeField]
    //private string animaAttackIndexName = "Action Index";
    //private int animaAttackIndexNameHash;

    //private readonly Dictionary<string, List<SkillActionBehaviour>> skillActionSMBs = new Dictionary<string, List<SkillActionBehaviour>>();
    //private readonly Dictionary<string, SkillInformation> allSkillInfos = new Dictionary<string, SkillInformation>();

    //private SkillAgent currentSkill;
    //private readonly Queue<SkillAction> currentskillActions = new Queue<SkillAction>();

    //private SkillAction currentAction;
    //private SkillAction nextAction;

    //public Animator PlayerAnimator
    //{
    //    get
    //    {
    //        if (PlayerManager.Instance && PlayerManager.Instance.Controller)
    //            return PlayerManager.Instance.Controller.Animator.GetAnimatorComponent();
    //        else return null;
    //    }
    //}

    //private void Awake()
    //{
    //    var skillInfos = Resources.LoadAll<SkillInformation>("Configuration");
    //    foreach (var info in skillInfos)
    //    {
    //        if (!allSkillInfos.ContainsKey(info.ID))
    //            allSkillInfos.Add(info.ID, Instantiate(info));
    //    }
    //    if (!PlayerAnimator) return;
    //    var actionSMBs = PlayerAnimator.GetBehaviours<SkillActionBehaviour>();
    //    foreach (var behaviour in actionSMBs)
    //        if (behaviour.parentSkill)
    //        {
    //            behaviour.parentSkill = allSkillInfos[behaviour.parentSkill.ID];
    //            behaviour.enterCallback = OnActionEnter;
    //            behaviour.updateCallback = OnActionUpdate;
    //            behaviour.exitCallback = OnActionExit;
    //            skillActionSMBs.TryGetValue(behaviour.parentSkill.ID, out var actionBehaviours);
    //            if (actionBehaviours != null && actionBehaviours.Count > 0)
    //                actionBehaviours.Add(behaviour);
    //            else skillActionSMBs.Add(behaviour.parentSkill.ID, new List<SkillActionBehaviour>() { behaviour });
    //        }
    //    foreach (var actionList in skillActionSMBs.Values)
    //        actionList.Sort((x, y) =>
    //        {
    //            if (x.actionIndex < y.actionIndex) return -1;
    //            else if (x.actionIndex > y.actionIndex) return 1;
    //            else return 0;
    //        });
    //    animaAttackIndexNameHash = Animator.StringToHash(animaAttackIndexName);
    //}

    //public void UseSkill(SkillAgent skill)
    //{
    //    currentSkill = skill;
    //    currentskillActions.Clear();
    //    using (var actionsEnum = skill.info.SkillActions.GetEnumerator())
    //        while (actionsEnum.MoveNext())
    //            currentskillActions.Enqueue(actionsEnum.Current);
    //    skill.OnUse();
    //    PlayerAnimator.SetTrigger(skill.info.AnimaTriggerName);
    //    PlayerAnimator.SetInteger(animaAttackIndexNameHash, 0);
    //}

    //private void TryActNext(int currentIndex)
    //{
    //    if (nextAction) return;
    //    if (currentskillActions.Count > 0)
    //    {
    //        nextAction = currentskillActions.Dequeue();
    //        PlayerAnimator.SetInteger(animaAttackIndexNameHash, currentIndex + 1);
    //    }
    //}

    //private void OnActionEnter(SkillActionBehaviour behaviour)
    //{
    //    if (behaviour.parentSkill != currentSkill.info) return;
    //    if (behaviour.actionIndex < 0 || behaviour.actionIndex >= currentSkill.info.SkillActions.Count) return;
    //    if (currentSkill.info.SkillActions[behaviour.actionIndex] != currentAction) return;
    //    behaviour.runtimeAction = currentAction;
    //    //TODO 消耗MP之类的操作
    //}
    //private void OnActionUpdate(float normalizedTime)
    //{
    //    if (currentAction && normalizedTime > currentAction.InputListenBeginNrmlzTime && normalizedTime < currentAction.InputTimeOutNrmlzTime)
    //        if (Input.GetButtonDown("Attack"))
    //            TryActNext(PlayerAnimator.GetInteger(animaAttackIndexNameHash));
    //}
    //private void OnActionExit(SkillActionBehaviour behaviour)
    //{
    //    if (!nextAction)//这种情况下，技能已经施放完毕或中断
    //    {
    //        PlayerAnimator.SetInteger(animaAttackIndexNameHash, -1);
    //        currentAction = null;
    //        currentSkill = null;
    //        return;
    //    }
    //    if (behaviour.runtimeAction != currentAction) return;
    //    currentAction = nextAction;
    //    nextAction = null;
    //}
}