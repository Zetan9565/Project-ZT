using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("打印结点：使用Debug类进行打印，一般用于调试")]
    public class Log : Action
    {
        [DisplayName("类型")]
        public LogType logType;
        [DisplayName("消息")]
        public SharedString message = "Debug log";

        public override bool IsValid
        {
            get
            {
                return message != null && message.IsValid;
            }
        }

        protected override NodeStates OnUpdate()
        {
            switch (logType)
            {
                case LogType.Normal:
                    Debug.Log(message.Value);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message.Value);
                    break;
                case LogType.Error:
                    Debug.LogError(message.Value);
                    break;
                default:
                    break;
            }
            return NodeStates.Success;
        }

        public enum LogType
        {
            Normal,
            Warning,
            Error
        }
    }
}