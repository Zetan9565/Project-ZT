using System.Collections.Generic;

public class DialogueData
{
    public string ID => model ? model.ID : string.Empty;

    public readonly Dialogue model;

    public readonly List<DialogueWordsData> wordsDatas = new List<DialogueWordsData>();

    public DialogueData(Dialogue dialogue)
    {
        model = dialogue;
        foreach (DialogueWords words in model.Words)
        {
            wordsDatas.Add(new DialogueWordsData(words, this));
        }
    }

    public static implicit operator bool(DialogueData self)
    {
        return self != null;
    }
}

public class DialogueWordsData
{
    public readonly DialogueWords model;
    public readonly DialogueData parent;

    public bool IsDone => optionDatas.TrueForAll(x => x.isDone);

    public readonly List<WordsOptionData> optionDatas = new List<WordsOptionData>();

    public DialogueWordsData(DialogueWords words, DialogueData parent)
    {
        model = words;
        this.parent = parent;
        if (model.Options.Count > 0 && parent)
        {
            int index = parent.model.Words.IndexOf(words);
            for (int i = 0; i < model.Options.Count; i++)
            {
                int indexBack = index;
                WordsOption option = model.Options[i];
                if (words.NeedToChusCorrectOption)
                {
                    if (model.IndexOfCorrectOption == i)
                        indexBack++;
                }
                else
                {
                    indexBack = option.IndexToGoBack > 0 && option.OptionType != WordsOptionType.SubmitAndGet && option.OptionType != WordsOptionType.OnlyGet &&
                        parent.model.StoryDialogue && (option.OptionType == WordsOptionType.BranchDialogue || option.OptionType == WordsOptionType.BranchWords) && !option.GoBack
                        ? option.IndexToGoBack : indexBack;
                }
                WordsOptionData od = new WordsOptionData(option, this, indexBack);
                optionDatas.Add(od);
            }
        }
    }

    public static implicit operator bool(DialogueWordsData self)
    {
        return self != null;
    }
}

public class WordsOptionData
{
    public readonly WordsOption model;
    public readonly DialogueWordsData parent;
    public readonly int indexToGoBack;

    public bool isDone;

    public WordsOptionData(WordsOption option, DialogueWordsData parent, int indexToGoBack)
    {
        model = option;
        this.parent = parent;
        this.indexToGoBack = indexToGoBack;
    }

    public static implicit operator bool(WordsOptionData self)
    {
        return self != null;
    }
}