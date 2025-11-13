using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class InfoFormPage : MonoBehaviour
{
    [SerializeField] TMP_InputField nicknameIF;
    [SerializeField] TMP_InputField birthIF;
    [SerializeField] Button saveButton;
    [SerializeField] GameObject nextPage;
    [SerializeField] GameObject thisPage;

    void Awake()
    {
        if (!thisPage) thisPage = gameObject;
        
        saveButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(OnClickSaveAndNext);

        nicknameIF.onValueChanged.RemoveAllListeners();
        birthIF.onValueChanged.RemoveAllListeners();
        nicknameIF.onValueChanged.AddListener(_ => RefreshInteractable());
        birthIF.onValueChanged.AddListener(OnBirthChanged);

        RefreshInteractable();
    }

    void OnBirthChanged(string _)
    {
        string raw = birthIF.text;
        System.Text.StringBuilder sb = new System.Text.StringBuilder(6);
        foreach (char c in raw)
        {
            if (char.IsDigit(c)) { sb.Append(c); if (sb.Length >= 6) break; }
        }
        string cleaned = sb.ToString();
        if (cleaned != raw) { birthIF.text = cleaned; birthIF.caretPosition = birthIF.text.Length; }
        RefreshInteractable();
    }

    void RefreshInteractable()
    {
        saveButton.interactable = IsValid();
    }

    bool IsValid()
    {
        // 닉네임 1자 이상
        if (string.IsNullOrWhiteSpace(nicknameIF.text)) return false;

        // 생년월일 YYMMDD (6자리 숫자)
        string s = birthIF.text;
        return IsValidDateYYMMDD(s, out _);
    }
    bool IsValidDateYYMMDD(string s, out DateTime parsedDate)
    {
        parsedDate = default;
        
        // 값이 비어있는지 체크
        if (string.IsNullOrEmpty(s)) return false;

        // 6글자 인지 체크
        if (s.Length != 6) return false;

        // 숫자인지 체크
        for (int i = 0; i < 6; i++)
            if (!char.IsDigit(s[i])) return false;

        // 입력값 날짜 변환
        if (DateTime.TryParseExact(
                s,
                "yyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
        {
            parsedDate = dt;
            return true;
        }
        return false;
    }

    void OnClickSaveAndNext()
    {
        if (!IsValid()) { RefreshInteractable(); return; }

        // 각각 저장
        string nick = nicknameIF.text.Trim();
        string birth = birthIF.text.Trim();
        
        if (LocalJsonDB.Current == null) LocalJsonDB.StartNewSession();
        LocalJsonDB.UpdateCurrent(r => {
            r.nickname     = nick;
            r.birthYYMMDD  = birth;
        });

        // 페이지 전환 (즉시)
        if (nextPage)
        {
            nextPage.SetActive(true);
            (thisPage ? thisPage : gameObject).SetActive(false);
        }
    }
}
