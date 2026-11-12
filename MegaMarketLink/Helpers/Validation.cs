using System.Net.Mail;
using System.Linq;
using System.Text.RegularExpressions;

namespace MegaMarketLink.Helpers;

public static class Validation
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPassword(string password) => password.Length >= 8;

    public static bool IsValidPhone(string phone)
    {
        string digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 11;
    }

    public static bool IsValidInn(string inn)
    {
        if (!Regex.IsMatch(inn, @"^\d{10}$|^\d{12}$"))
            return false;

        int[] weights10 = { 2, 4, 10, 3, 5, 9, 4, 6, 8 };
        int[] weights12First = { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
        int[] weights12Second = { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

        int CheckDigit(string value, int[] weights)
        {
            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
                sum += (value[i] - '0') * weights[i];

            return (sum % 11) % 10;
        }

        if (inn.Length == 10)
            return CheckDigit(inn, weights10) == inn[9] - '0';

        return CheckDigit(inn, weights12First) == inn[10] - '0'
            && CheckDigit(inn, weights12Second) == inn[11] - '0';
    }
}
