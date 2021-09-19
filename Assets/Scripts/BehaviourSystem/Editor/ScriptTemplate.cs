using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public struct ScriptTemplate
    {
        public string fileName;
        public string folder;
        public TextAsset templateFile;

        public static ScriptTemplate Action => new ScriptTemplate { templateFile = BehaviourTreeSettings.GetOrCreate().scriptTemplateAction, fileName = "NewAction.cs", folder = "Action" };
        public static ScriptTemplate Conditional => new ScriptTemplate { templateFile = BehaviourTreeSettings.GetOrCreate().scriptTemplateConditional, fileName = "NewConditional.cs", folder = "Conditional" };
        public static ScriptTemplate Composite => new ScriptTemplate { templateFile = BehaviourTreeSettings.GetOrCreate().scriptTemplateComposite, fileName = "NewComposite.cs", folder = "Composite" };
        public static ScriptTemplate Decorator => new ScriptTemplate { templateFile = BehaviourTreeSettings.GetOrCreate().scriptTemplateDecorator, fileName = "NewDecorator.cs", folder = "Decorator" };
        public static ScriptTemplate Variable => new ScriptTemplate { templateFile = BehaviourTreeSettings.GetOrCreate().scriptTemplateVariable, fileName = "NewVariable.cs", folder = "" };
    }
}