using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Name("随机选项顺序"), Width(50f)]
    [Description("按随机顺序显示这些选项。")]
    public sealed class RandomOptions : ExternalOptionsNode
    {
        public override bool IsValid => true;

        [field: SerializeField]
        public bool Always { get; private set; }

        public override ReadOnlyCollection<DialogueOption> GetOptions(DialogueData entryData, DialogueNode owner)
        {
            var order = getOptionOrder(entryData);
            var options = new DialogueOption[order.Count];
            for (int i = 0; i < order.Count; i++)
            {
                options[i] = this.options[order[i]];
            }
            return new ReadOnlyCollection<DialogueOption>(options);

            IList<int> getOptionOrder(DialogueData entryData)
            {
                if (Always) return Utility.RandomOrder(getIndices());
                var data = entryData[this];
                if (!data.Accessed) return data.AdditionalData.Write("order", new GenericData()).WriteAll(Utility.RandomOrder(getIndices()));
                else return data.AdditionalData?.ReadData("order")?.ReadIntList() as IList<int> ?? getIndices();

                int[] getIndices()
                {
                    int[] indices = new int[this.options.Length];
                    for (int i = 0; i < this.options.Length; i++)
                    {
                        indices[i] = i;
                    }
                    return indices;
                }
            }
        }
    }
}