using System;
using System.Collections;
using System.Text;
using UnityEngine;

public static class MiscFuntion
{
    public static string HandlingContentWithKeyWords(string input, bool color = false, params Array[] configs)
    {
        StringBuilder output = new StringBuilder();
        StringBuilder keyWordsGetter = new StringBuilder();
        bool startGetting = false;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '{' && i + 1 < input.Length)
            {
                startGetting = true;
                i++;
            }
            else if (input[i] == '}')
            {
                startGetting = false;
                output.Append(HandlingName(keyWordsGetter.ToString()));
                keyWordsGetter.Clear();
            }
            else if (!startGetting) output.Append(input[i]);
            if (startGetting) keyWordsGetter.Append(input[i]);
        }

        return output.ToString();

        string HandlingName(string keyWords)
        {
            if (keyWords.StartsWith("[NPC]"))//为了性能，建议多此一举
            {
                keyWords = keyWords.Replace("[NPC]", string.Empty);
                TalkerInformation[] talkers = null;
                if (configs != null)
                {
                    foreach (var array in configs)
                    {
                        if (array.GetType().IsArray)
                            if (array.GetType().GetElementType() == typeof(TalkerInformation))
                                talkers = array as TalkerInformation[];
                    }
                }
                if (talkers == null) talkers = Resources.LoadAll<TalkerInformation>("Configuration");
                var talker = Array.Find(talkers, x => x.ID == keyWords);
                if (talker) keyWords = talker.name;
                return color ? ZetanUtility.ColorText(keyWords, Color.green) : keyWords;
            }
            else if (keyWords.StartsWith("[ITEM]"))
            {
                keyWords = keyWords.Replace("[ITEM]", string.Empty);
                ItemBase[] items = null;
                if (configs != null)
                {
                    foreach (var array in configs)
                    {
                        if (array.GetType().IsArray)
                            if (array.GetType().GetElementType() == typeof(ItemBase))
                                items = array as ItemBase[];
                    }
                }
                if (items == null) items = Resources.LoadAll<ItemBase>("Configuration");
                var item = Array.Find(items, x => x.ID == keyWords);
                if (item) keyWords = item.name;
                return color ? ZetanUtility.ColorText(keyWords, Color.yellow) : keyWords;
            }
            else if (keyWords.StartsWith("[ENMY]"))
            {
                keyWords = keyWords.Replace("[ENMY]", string.Empty);
                EnemyInformation[] enemies = null;
                if (configs != null)
                {
                    foreach (var array in configs)
                    {
                        if (array.GetType().IsArray)
                            if (array.GetType().GetElementType() == typeof(EnemyInformation))
                                enemies = array as EnemyInformation[];
                    }
                }
                if (enemies == null) enemies = Resources.LoadAll<EnemyInformation>("Configuration");
                var enemy = Array.Find(enemies, x => x.ID == keyWords);
                if (enemy) keyWords = enemy.name;
                return color ? ZetanUtility.ColorText(keyWords, Color.red) : keyWords;
            }
            return keyWords;
        }
    }
}