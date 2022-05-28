using UnityEngine;

namespace ZetanStudio.BehaviourTree.Editor
{
    public struct ScriptTemplate
    {
        public string fileName;
        public string folder;
        public TextAsset templateFile;

        public static ScriptTemplate Action => new ScriptTemplate { templateFile = BehaviourTreeEditorSettings.GetOrCreate().scriptTemplateAction, fileName = "NewAction.cs", folder = "Action" };
        public static ScriptTemplate Conditional => new ScriptTemplate { templateFile = BehaviourTreeEditorSettings.GetOrCreate().scriptTemplateConditional, fileName = "NewConditional.cs", folder = "Conditional" };
        public static ScriptTemplate Composite => new ScriptTemplate { templateFile = BehaviourTreeEditorSettings.GetOrCreate().scriptTemplateComposite, fileName = "NewComposite.cs", folder = "Composite" };
        public static ScriptTemplate Decorator => new ScriptTemplate { templateFile = BehaviourTreeEditorSettings.GetOrCreate().scriptTemplateDecorator, fileName = "NewDecorator.cs", folder = "Decorator" };
        public static ScriptTemplate Variable => new ScriptTemplate { templateFile = BehaviourTreeEditorSettings.GetOrCreate().scriptTemplateVariable, fileName = "NewVariable.cs", folder = "" };
    }
}