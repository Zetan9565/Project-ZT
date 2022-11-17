using System;
using UnityEngine;
using UnityEditor;

namespace ZetanStudio.CharacterSystem
{
    internal class RegexHelpBoxAttribute : PropertyAttribute
    {
        public readonly string pattern;
        public readonly string message;
        public readonly MessageType type;

        public RegexHelpBoxAttribute(string pattern, string message, MessageType type = MessageType.Error)
        {
            this.pattern = pattern;
            this.message = message;
            this.type = type;
        }
    }
}