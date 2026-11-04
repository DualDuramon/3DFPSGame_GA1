using UnityEngine;

public class PlayerPrefsAccountRepository
{
    //계정 데이터 보관소
    private const string KeyPrefix = "Account_";
    private string GetKeyID(string key)
    {
        return KeyPrefix + key;
    }

    //회원가입
    public void Register(string id, string pw)
    {
        PlayerPrefs.SetString(GetKeyID(id), pw);
        PlayerPrefs.Save();
    }

    //아이디 중복 검사
    public bool Exist(string id)
    {
        return PlayerPrefs.HasKey(GetKeyID(id));
    }

    //비밀번호 검증
    public bool CheckPassword(string id, string pw)
    {
        string prefixedID = GetKeyID(id);
        return Exist(id) && PlayerPrefs.GetString(prefixedID) == pw;
    }
}
