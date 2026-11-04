using System.Text.RegularExpressions;

public class AccountSpecification
{
    private const int PasswordMinLength = 8;
    private const int PasswordMaxLength = 30;

    // ID: 영문·숫자·-·@·_로 이루어진 3~20글자
    private static readonly Regex IdRegex = new Regex(@"^[A-Za-z0-9\-@_]{3,20}$");
    // 이메일: '@' 하나, 도메인은 '.'으로 시작하거나 끝나지 않고 '.'을 포함, 공백 없음
    private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s.][^@\s]*\.[^@\s.]+$");
    // 암호 허용 문자: 공백을 뺀 ASCII 영문·숫자·기호
    private static readonly Regex PasswordCharsRegex = new Regex(@"^[!-~]*$");
    // 암호 조합: 대문자·소문자·숫자·기호를 각각 하나 이상 포함
    private static readonly Regex PasswordComplexityRegex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9]).*$");

    public bool IsSaticfiedBy(string email, out string errorMessage)
    {
        errorMessage = string.Empty;
        bool result = EmailRegex.IsMatch(email);

        if (!result)
        {
            errorMessage = "이메일이 만족하지 않습니다";

        }

        return result;
    }

    // ID 규칙: 이메일 형식이거나, 영문·숫자·-·@·_로 이루어진 3~20글자
    public bool IsIdSatisfiedBy(string id, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrEmpty(id) || (!IdRegex.IsMatch(id) && !EmailRegex.IsMatch(id)))
        {
            errorMessage = "ID는 이메일 또는 영문, 숫자, -, @, _로 된 3~20글자여야 합니다.";
            return false;
        }

        return true;
    }

    // 암호 규칙: 대문자·소문자·숫자·기호를 모두 포함한 8~30글자 (공백 제외 ASCII만 허용)
    public bool IsPasswordSatisfiedBy(string password, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrEmpty(password) || password.Length < PasswordMinLength || password.Length > PasswordMaxLength)
        {
            errorMessage = $"암호는 {PasswordMinLength}~{PasswordMaxLength}글자여야 합니다.";
            return false;
        }

        if (!PasswordCharsRegex.IsMatch(password))
        {
            errorMessage = "암호에는 영문, 숫자, 기호만 사용할 수 있습니다.";
            return false;
        }

        if (!PasswordComplexityRegex.IsMatch(password))
        {
            errorMessage = "암호에는 대문자, 소문자, 숫자, 기호가 모두 포함되어야 합니다.";
            return false;
        }

        return true;
    }

    // 암호 확인 규칙: 암호와 암호 확인 입력이 같아야 한다
    public bool IsPasswordConfirmSatisfiedBy(string password, string confirm, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (password != confirm)
        {
            errorMessage = "암호가 일치하지 않습니다.";
            return false;
        }

        return true;
    }
}
