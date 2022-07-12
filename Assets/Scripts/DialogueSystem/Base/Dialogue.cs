using System;
using System.Text;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    [CreateAssetMenu(fileName = "dialogue", menuName = "Zetan Studio/剧情/对话")]
    public sealed class Dialogue : ScriptableObject
    {
        public string ID => Entry?.ID ?? string.Empty;

        [SerializeReference]
        private DialogueContent[] contents = { };
        public ReadOnlyCollection<DialogueContent> Contents => new ReadOnlyCollection<DialogueContent>(contents);

        public EntryContent Entry => contents[0] as EntryContent;

        public bool Exitable => Traverse(Entry, n => n.ExitHere);

        public Dialogue() => contents = new DialogueContent[] { new EntryContent() };

        public bool Reachable(DialogueContent content) => Reachable(Entry, content);
        public static bool Reachable(DialogueContent from, DialogueContent to)
        {
            if (!to) return false;
            bool reachable = false;
            Traverse(from, c =>
            {
                reachable = c == to;
                return reachable;
            });
            return reachable;
        }

        public static void Traverse(DialogueContent content, Action<DialogueContent> onAccess, bool normalOnly = false)
        {
            if (content)
            {
                if (!normalOnly || DialogueContent.IsNormal(content)) onAccess?.Invoke(content);
                foreach (var option in content.Options)
                {
                    Traverse(option.Content, onAccess, normalOnly);
                }
            }
        }

        ///<param name="onAccess">带中止条件的访问器，返回 <i>true</i> 时将中止遍历</param>
        /// <returns>是否在遍历时产生中止</returns>
        public static bool Traverse(DialogueContent content, Func<DialogueContent, bool> onAccess, bool normalOnly = false)
        {
            if (onAccess != null && content)
            {
                if (!normalOnly || DialogueContent.IsNormal(content))
                    if (onAccess(content)) return true;
                foreach (var option in content.Options)
                {
                    if (Traverse(option.Content, onAccess, normalOnly))
                        return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 用于在编辑器中记录操作退出点，不应在游戏逻辑中使用
        /// </summary>
        public ExitContent exit = new ExitContent();
        /// <summary>
        /// 用于在编辑器中备注本段对话的用途，不应在游戏逻辑中使用
        /// </summary>
        [TextArea]
        public string description;

        public static class Editor
        {
            public static DialogueContent AddContent(Dialogue dialogue, Type type)
            {
                if (!typeof(DialogueContent).IsAssignableFrom(type)) return null;
                var content = Activator.CreateInstance(type) as DialogueContent;
                UnityEditor.ArrayUtility.Add(ref dialogue.contents, content);
                return content;
            }
            public static DialogueContent RemoveContent(Dialogue dialogue, DialogueContent content)
            {
                UnityEditor.ArrayUtility.Remove(ref dialogue.contents, content);
                return content;
            }
            public static void CopyFromOld(Dialogue dialogue, OldDialogue old)
            {
                dialogue.contents = new DialogueContent[] { };
                DialogueContent parent = null;
                for (int i = 0; i < old.Words.Count; i++)
                {
                    var words = old.Words[i];
                    bool last = i == old.Words.Count - 1;
                    DialogueContent content;
                    string talker;
                    if (words.TalkerType == TalkerType.NPC) talker = Keywords.Generate(words.TalkerInfo);
                    else if (words.TalkerType == TalkerType.UnifiedNPC) talker = old.UseCurrentTalkerInfo ? "[NPC]" : Keywords.Generate(old.UnifiedNPC);
                    else talker = "[PLAYER]";
                    if (i == 0)
                    {
                        parent = content = addContent(new EntryContent(talker, words.Content));
                        if (old.Words.Count > 1 || old.Words[i].Options.Count > 0)
                        {
                            DialogueContent.Editor.SetAsExit(parent, false);
                            DialogueContent.Editor.RemoveOption(parent, parent[0]);
                        }
                    }
                    else
                    {
                        DialogueOption.Editor.SetContent(DialogueContent.Editor.AddOption(parent, true), content = addContent(new WordsContent(talker, words.Content)));
                        content._position = new Vector2(parent._position.x + 360f, parent._position.y);
                        parent = content;
                    }
                    if (words.Options.Count > 0)
                    {
                        float y = content._position.y;
                        for (int j = 0; j < words.Options.Count; j++)
                        {
                            var oopt = words.Options[j];
                            var opt = DialogueContent.Editor.AddOption(content, false, oopt.Title);
                            DialogueContent child;
                            switch (oopt.OptionType)
                            {
                                case WordsOptionType.BranchDialogue:
                                    DialogueOption.Editor.SetContent(opt, child = addContent(new BranchContent() { _position = new Vector2(content._position.x + 360f, y) }));
                                    parent = child;
                                    if (last) DialogueContent.Editor.SetAsExit(child);
                                    break;
                                case WordsOptionType.Choice:
                                    if (oopt.DeleteWhenCmplt)
                                    {
                                        var delete = addContent(new DeleteOnDoneDecorator());
                                        delete._position = new Vector2(content._position.x + 360f, y);
                                        DialogueOption.Editor.SetContent(opt, delete);
                                        DialogueOption.Editor.SetContent(delete[0],
                                            child = addContent(new WordsContent(oopt.TalkerType == TalkerType.NPC ? "[NPC]" : "[PLAYER]", oopt.Words)
                                            {
                                                _position = new Vector2(delete._position.x + 360f, y)
                                            }));
                                    }
                                    else
                                    {
                                        DialogueOption.Editor.SetContent(opt,
                                            child = addContent(new WordsContent(oopt.TalkerType == TalkerType.NPC ? "[NPC]" : "[PLAYER]", oopt.Words)
                                            {
                                                _position = new Vector2(content._position.x + 360f, y)
                                            }));
                                    }
                                    if (words.NeedToChusCorrectOption && words.IndexOfCorrectOption != j)
                                    {
                                        DialogueContent wrong = addContent(new WordsContent("[NPC]", words.WrongChoiceWords));
                                        wrong._position = new Vector2(child._position.x + 360f, child._position.y);
                                        DialogueOption.Editor.SetContent(DialogueContent.Editor.AddOption(wrong, true),
                                            addContent(new RecursionSuffix(3) { _position = new Vector2(wrong._position.x + 360f, wrong._position.y) }));
                                        DialogueOption.Editor.SetContent(DialogueContent.Editor.AddOption(child, true), wrong);
                                    }
                                    else
                                    {
                                        parent = child;
                                        if (last) DialogueContent.Editor.SetAsExit(child);
                                    }
                                    break;
                                case WordsOptionType.SubmitAndGet:
                                    if (oopt.ItemCanGet?.Item)
                                        DialogueOption.Editor.SetContent(opt,
                                            child = addContent(new SubmitAndGetItemContent("[NPC]", oopt.Words, new ItemInfo[] { oopt.ItemToSubmit }, new ItemInfo[] { oopt.ItemCanGet })
                                            {
                                                _position = new Vector2(content._position.x + 360f, y)
                                            }));
                                    else DialogueOption.Editor.SetContent(opt,
                                        child = addContent(new SubmitItemContent("[NPC]", oopt.Words, oopt.ItemToSubmit) { _position = new Vector2(content._position.x + 360f, y) }));
                                    parent = child;
                                    if (last) DialogueContent.Editor.SetAsExit(child);
                                    break;
                                case WordsOptionType.OnlyGet:
                                    DialogueOption.Editor.SetContent(opt,
                                        child = addContent(new GetItemContent("[NPC]", oopt.Words, oopt.ItemCanGet) { _position = new Vector2(content._position.x + 360f, y) }));
                                    parent = child;
                                    if (last) DialogueContent.Editor.SetAsExit(child);
                                    break;
                                default:
                                    DialogueOption.Editor.SetContent(opt,
                                        child = addContent(new WordsContent(oopt.TalkerType == TalkerType.NPC ? "[NPC]" : "[PLAYER]", oopt.Words)
                                        {
                                            _position = new Vector2(content._position.x + 360f, y)
                                        }));
                                    if (!oopt.GoBack)
                                    {
                                        parent = child;
                                        if (last) DialogueContent.Editor.SetAsExit(child);
                                    }
                                    else
                                    {
                                        DialogueOption.Editor.SetContent(DialogueContent.Editor.AddOption(child, true),
                                            addContent(new RecursionSuffix(oopt.IndexToGoBack < 0 ? 2 : 2 + (i - oopt.IndexToGoBack))
                                            {
                                                _position = new Vector2(child._position.x + 360f, child._position.y)
                                            }));
                                    }
                                    break;
                            }
                            y += 100;
                        }
                    }
                    else if (last) DialogueContent.Editor.SetAsExit(content);
                }
                dialogue.exit._position = new Vector2(dialogue.contents[^1]._position.x + 360f, 0);
                DialogueContent addContent(DialogueContent content)
                {
                    UnityEditor.ArrayUtility.Add(ref dialogue.contents, content);
                    return content;
                }
            }

            public static string Preview(Dialogue dialogue)
            {
                if (!dialogue) return null;
                StringBuilder sb = new StringBuilder();
                foreach (var content in dialogue.contents)
                {
                    if (content is TextContent text)
                    {
                        sb.Append(Keywords.Editor.HandleKeyWords(text.Talker));
                        sb.Append(": ");
                        sb.Append(Keywords.Editor.HandleKeyWords(text.Text));
                        if (dialogue.contents[^1] != content) sb.Append('\n');
                    }
                    else if (content is BranchContent branch && branch.Dialogue)
                    {
                        sb.Append(Keywords.Editor.HandleKeyWords(branch.Dialogue.Entry.Talker));
                        sb.Append(": ");
                        sb.Append(Keywords.Editor.HandleKeyWords(branch.Dialogue.Entry.Text));
                        if (dialogue.contents[^1] != content) sb.Append('\n');
                    }
                }
                return sb.ToString();
            }
        }
#endif
    }
}