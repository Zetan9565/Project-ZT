using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.DialogueSystem.Editor
{
    public class DialogueGroup : Group
    {
        public DialogueGroupData Group { get; private set; }

        private readonly Action onBeforeModify;

        public DialogueGroup(IEnumerable<Node> nodes, DialogueGroupData group, Action<DialogueGroup, ContextualMenuPopulateEvent> onRightClick, Action onBeforeModify)
        {
            this.Q("titleContainer").style.minHeight = 31f;
            var label = this.Q<Label>("titleLabel");
            label.style.fontSize = 16f;
            if (group != null)
            {
                title = group.name;
                Group = group;
                userData = group;
                style.left = group.position.x;
                style.top = group.position.y;
                if (nodes != null) AddElements(nodes);
            }
            this.onBeforeModify = onBeforeModify;
            headerContainer.AddManipulator(new ContextualMenuManipulator(evt => onRightClick?.Invoke(this, evt)));
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            base.OnGroupRenamed(oldName, newName);
            if (Group == null) return;
            onBeforeModify?.Invoke();
            Group.name = newName;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (Group == null) return;
            onBeforeModify?.Invoke();
            Group.position.x = newPos.xMin;
            Group.position.y = newPos.yMin;
        }
    }
}
