using UnityEngine;

public class TalkTestButton : MonoBehaviour {

    public Talker talker;
	
    public void OnClick()
    {
        DialogueManager.Instance.StartNormalDialogue(talker);
    }
}
