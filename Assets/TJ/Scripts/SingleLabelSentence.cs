using UnityEngine;
using TMPro;

public class SingleLabelSentence : MonoBehaviour
{
    public TMP_Text target;

    [TextArea(2, 3)]
    public string format =
        "{label}{iga} 전에 나, 지금의 나, 미래의 나, 모두 소중해요.\n오늘 당신은 자신을 충분히 안아줄 수 있습니다.";

    public string fallbackLabel = "나";

    void Awake()
    {
        if (!target) target = GetComponent<TMP_Text>();
    }

    void OnEnable() => Render();

    public void Render()
    {
        if (!target) return;

        var rec = LocalJsonDB.Current;
        string label = (rec != null && !string.IsNullOrWhiteSpace(rec.identityLabel))
            ? rec.identityLabel : fallbackLabel;

        string iga = ChooseIga(label);
        target.SetText(format.Replace("{label}", label).Replace("{iga}", iga));
    }

    string ChooseIga(string word)
    {
        if (string.IsNullOrEmpty(word)) return "가";
        char last = word[word.Length - 1];
        if (last < 0xAC00 || last > 0xD7A3) return "가";
        int code = last - 0xAC00;
        int jong = code % 28;
        return (jong == 0) ? "가" : "이";
    }
}