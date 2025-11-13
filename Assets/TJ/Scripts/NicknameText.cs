using TMPro;
using UnityEngine;

public class NicknameText : MonoBehaviour
{
    [SerializeField] TMP_Text target;
    [SerializeField] string format = "{0}";

    void OnEnable(){
        if (!target) target = GetComponent<TMP_Text>();
        var rec = LocalJsonDB.Current;
        var nick = (rec != null && !string.IsNullOrWhiteSpace(rec.nickname)) ? rec.nickname : "닉네임";
        target.SetText(string.Format(format, nick));
    }
}