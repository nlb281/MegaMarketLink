using System;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using MegaMarketLink.Models;
using MegaMarketLink.Views;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using ReactiveUI;

namespace MegaMarketLink.ViewModels;

public class AuthVM : ViewModelBase
{
    string loginEmail = "";
    string loginPassword = "";

    string registerEmail = "";
    string phoneNumber = "";
    string registerPassword = "";
    string confirmPassword = "";

    string companyName = "";
    string inn = "";

    bool isLoginMode;
    bool isRegisterMode;
    bool isCustomer = true;

    public ReactiveCommand<Unit, Unit> ShowLoginCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowRegisterCommand { get; }

    public ReactiveCommand<Window, Unit> LoginCommand { get; }
    public ReactiveCommand<Window, Unit> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> VerifyINNCommand { get; }

    public AuthVM()
    {
        ShowLoginCommand = ReactiveCommand.Create(() =>
        {
            IsLoginMode = true;
            IsRegisterMode = false;
        });

        ShowRegisterCommand = ReactiveCommand.Create(() =>
        {
            IsRegisterMode = true;
            IsLoginMode = false;
        });

        LoginCommand = ReactiveCommand.CreateFromTask<Window>(login);
        RegisterCommand = ReactiveCommand.CreateFromTask<Window>(register);
        VerifyINNCommand = ReactiveCommand.Create(verifyINN);
    }
    
    // login
    private async Task login(Window authWindow)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", "Заполните email и пароль.").ShowWindowAsync();
            return;
        }

        StaticFields.user = await StaticFields.context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == LoginEmail.Trim());

        if (StaticFields.user == null || StaticFields.user.IsBanned)
        {
            StaticFields.user = null;
            await MessageBoxManager.GetMessageBoxStandard("Ошибка авторизации", "Неверный email или пароль.").ShowWindowAsync();
            return;
        }
        
        bool passwordIsValid = BCrypt.Net.BCrypt.Verify(
            LoginPassword,
            StaticFields.user.PasswordHash
        );
        
        if (!passwordIsValid)
        {
            StaticFields.user = null;
            await MessageBoxManager.GetMessageBoxStandard("Ошибка авторизации", "Неверный email или пароль.").ShowWindowAsync();
            return;
        }
        
        await StaticFields.LoadCart();
        
        var MainWindow = new MainWindow()
        {
            DataContext = new MainWindowVM()
        };
        MainWindow.Show();
        authWindow.Close();
    }

    // register
    private async Task register(Window authWindow)
    {
        if (string.IsNullOrWhiteSpace(RegisterEmail) || string.IsNullOrWhiteSpace(RegisterPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка регистрации", "Заполните все обязательные поля.").ShowWindowAsync();
            return;
        }

        if (!IsValidEmail(RegisterEmail))
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите корректный email.").ShowWindowAsync();
            return;
        }
        
        if (!string.IsNullOrWhiteSpace(PhoneNumber))
        {
            string phone = new string(PhoneNumber.Where(char.IsDigit).ToArray());

            if (phone.Length != 11)
            { 
                MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите корректный номер телефона.").ShowWindowAsync();
                return;
            }
        }
        
        if (RegisterPassword.Length < 8)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", "Пароль должен содержать минимум 8 символов.").ShowWindowAsync();
            return;
        }

        if (RegisterPassword != ConfirmPassword)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", "Пароли не совпадают.").ShowWindowAsync();
            return;
        }
        
        if (IsSeller)
        {
            if (string.IsNullOrWhiteSpace(CompanyName) || string.IsNullOrWhiteSpace(Inn))
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка регистрации", "Заполните данные продавца.").ShowWindowAsync();
                return;
            }

            if (!IsValidINN(Inn))
            {
                MessageBoxManager.GetMessageBoxStandard("Ошибка", "Некорректный ИНН.").ShowWindowAsync();
                return;
            }
            
            bool innExists = await StaticFields.context.Users.AnyAsync(x => x.Inn == Inn);

            if (innExists)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка регистрации", "Пользователь с таким ИНН уже существует.").ShowWindowAsync();
                return;
            }
        }
        
        bool emailExists = await StaticFields.context.Users.AnyAsync(x => x.Email == RegisterEmail);

        if (emailExists)
        {
            await MessageBoxManager.GetMessageBoxStandard("Ошибка регистрации", "Пользователь с таким email уже существует.").ShowWindowAsync();
            return;
        }
        
        string roleName = IsSeller ? "seller" : "customer";
        
        var role = await StaticFields.context.Roles
            .FirstOrDefaultAsync(x => x.Name == roleName);

        var user = new User
        {
            Email = RegisterEmail,
            Phone = PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(RegisterPassword),
            RoleId = role.Id,
            IsBanned = false,
            CreatedAt = DateTimeOffset.UtcNow,

            CompanyName = IsSeller ? CompanyName : null,
            Inn = IsSeller ? Inn : null
        };

        StaticFields.context.Users.Add(user);

        await StaticFields.context.SaveChangesAsync();
        
        StaticFields.user = await StaticFields.context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == user.Id);

        if (StaticFields.user == null)
            return;

        var MainWindow = new MainWindow()
        {
            DataContext = new MainWindowVM()
        };
        MainWindow.Show();
        authWindow.Close();
        
        await MessageBoxManager.GetMessageBoxStandard("Регистрация", "Аккаунт успешно создан!").ShowWindowAsync();
    }
    
    // supplementary methods
    private void verifyINN()
    {
        if (!IsValidINN(Inn))
        {
            MessageBoxManager.GetMessageBoxStandard("Проверка ИНН", "ИНН не прошёл проверку контрольных цифр.").ShowWindowAsync();
            return;
        }

        MessageBoxManager.GetMessageBoxStandard("Проверка ИНН", "Цифры ИНН корректны.").ShowWindowAsync();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            return new System.Net.Mail.MailAddress(email).Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidINN(string inn)
    {
        if (!Regex.IsMatch(inn, @"^\d{10}$|^\d{12}$"))
            return false;

        int[] weights10 = { 2, 4, 10, 3, 5, 9, 4, 6, 8 };
        int[] weights12First = { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
        int[] weights12Second = { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

        int checkDigit(string value, int[] weights)
        {
            int sum = 0;

            for (int i = 0; i < weights.Length; i++)
                sum += (value[i] - '0') * weights[i];

            return (sum % 11) % 10;
        }

        if (inn.Length == 10)
            return checkDigit(inn, weights10) == inn[9] - '0';

        return checkDigit(inn, weights12First) == inn[10] - '0'
            && checkDigit(inn, weights12Second) == inn[11] - '0';
    }

    public string LoginEmail
    {
        get => loginEmail;
        set => this.RaiseAndSetIfChanged(ref loginEmail, value);
    }

    public string LoginPassword
    {
        get => loginPassword;
        set => this.RaiseAndSetIfChanged(ref loginPassword, value);
    }

    public string RegisterEmail
    {
        get => registerEmail;
        set => this.RaiseAndSetIfChanged(ref registerEmail, value);
    }

    public string PhoneNumber
    {
        get => phoneNumber;
        set => this.RaiseAndSetIfChanged(ref phoneNumber, value);
    }

    public string RegisterPassword
    {
        get => registerPassword;
        set => this.RaiseAndSetIfChanged(ref registerPassword, value);
    }

    public string ConfirmPassword
    {
        get => confirmPassword;
        set => this.RaiseAndSetIfChanged(ref confirmPassword, value);
    }

    public string CompanyName
    {
        get => companyName;
        set => this.RaiseAndSetIfChanged(ref companyName, value);
    }

    public string Inn
    {
        get => inn;
        set => this.RaiseAndSetIfChanged(ref inn, value);
    }
    

    public bool IsLoginMode
    {
        get => isLoginMode;
        set
        {
            this.RaiseAndSetIfChanged(ref isLoginMode, value);
            this.RaisePropertyChanged(nameof(IsAuthSelectionVisible));
        }
    }
    
    public bool IsRegisterMode
    {
        get => isRegisterMode;
        set
        {
            this.RaiseAndSetIfChanged(ref isRegisterMode, value);
            this.RaisePropertyChanged(nameof(IsAuthSelectionVisible));
        }
    }
    
    public bool IsAuthSelectionVisible => !IsLoginMode && !IsRegisterMode;

    public bool IsCustomer
    {
        get => isCustomer;
        set
        {
            this.RaiseAndSetIfChanged(ref isCustomer, value);
            this.RaisePropertyChanged(nameof(IsSeller));
        }
    }

    public bool IsSeller => !IsCustomer;
    
}