using UnityEngine;

[DisallowMultipleComponent]
public class TalkTrigger2D : MonoBehaviour
{
#if UNITY_EDITOR
    [DisplayName("谈话人(必填)")]
#endif
    public Talker talker;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && talker)
            DialogueManager.Instance.CanTalk(talker);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && talker && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(talker);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && talker && DialogueManager.Instance.MTalker == talker)
            DialogueManager.Instance.CannotTalk();
    }
}
