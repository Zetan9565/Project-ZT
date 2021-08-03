using System.Collections.Generic;

public class DialogueData
{
    public readonly Dialogue origin;

    public readonly List<DialogueWordsData> wordsDatas = new List<DialogueWordsData>();

    public DialogueData(Dialogue dialogue)
    {
        origin = dialogue;
        foreach (DialogueWords words in origin.Words)
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
    public readonly DialogueWords origin;
    public readonly DialogueData parent;

    public bool IsDone => optionDatas.TrueForAll(x => x.isDone);

    public readonly List<WordsOptionData> optionDatas = new List<WordsOptionData>();

    public DialogueWordsData(DialogueWords words, DialogueData parent)
    {
        origin = words;
        this.parent = parent;
        if (origin.Options.Count > 0 && parent)
        {
            int index = parent.origin.Words.IndexOf(words);
            for (int i = 0; i < origin.Options.Count; i++)
            {
                int indexBack = index;
                WordsOption option = origin.Options[i];
                if (words.NeedToChusCorrectOption)
                {
                    if (origin.IndexOfCorrectOption == i)
                        indexBack++;
                }
                else
                {
                    indexBack = option.IndexToGoBack > 0 && option.OptionType != WordsOptionType.SubmitAndGet && option.OptionType != WordsOptionType.OnlyGet &&
                        parent.origin.StoryDialogue && (option.OptionType == WordsOptionType.BranchDialogue || option.OptionType == WordsOptionType.BranchWords) && !option.GoBack
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
    public readonly WordsOption origin;
    public readonly DialogueWordsData parent;
    public readonly int indexToGoBack;

    public bool isDone;

    public WordsOptionData(WordsOption option, DialogueWordsData parent, int indexToGoBack)
    {
        origin = option;
        this.parent = parent;
        this.indexToGoBack = indexToGoBack;
    }

    public static implicit operator bool(WordsOptionData self)
    {
        return self != null;
    }
}