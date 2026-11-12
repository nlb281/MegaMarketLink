using MegaMarketLink.Helpers;

namespace MegaMarketLink.Tests;

public class ValidationTests
{
    [Fact]
    public void IsValidEmail_Valid_ReturnsTrue()
    {
        // Arrange
        string email = "user@example.com";

        // Act
        bool actual = Validation.IsValidEmail(email);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValidEmail_WithoutAtSign_ReturnsFalse()
    {
        // Arrange
        string email = "userexample.com";

        // Act
        bool actual = Validation.IsValidEmail(email);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValidEmail_WithSpace_ReturnsFalse()
    {
        // Arrange
        string email = "user @example.com";

        // Act
        bool actual = Validation.IsValidEmail(email);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValidPassword_WithEightCharacters_ReturnsTrue()
    {
        // Arrange
        string password = "password";

        // Act
        bool actual = Validation.IsValidPassword(password);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValidPassword_TooShort_ReturnsFalse()
    {
        // Arrange
        string password = "abc123";

        // Act
        bool actual = Validation.IsValidPassword(password);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValidPhone_ElevenDigits_ReturnsTrue()
    {
        // Arrange
        string phone = "+7 (999) 123-45-67";

        // Act
        bool actual = Validation.IsValidPhone(phone);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValidPhone_TenDigits_ReturnsFalse()
    {
        // Arrange
        string phone = "9991234567";

        // Act
        bool actual = Validation.IsValidPhone(phone);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValidInn_ValidTenDigits_ReturnsTrue()
    {
        // Arrange
        string inn = "7707083893";

        // Act
        bool actual = Validation.IsValidInn(inn);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsValidInn_InvalidCheckDigit_ReturnsFalse()
    {
        // Arrange
        string inn = "7707083894";

        // Act
        bool actual = Validation.IsValidInn(inn);

        // Assert
        Assert.False(actual);
    }
}
