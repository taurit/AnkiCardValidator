using AnkiCardValidator.Utilities;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class StringHelpersTests
{
    [DataTestMethod]
    [DataRow("Hola, ¿cómo estás?")]
    [DataRow("la bamba")]
    [DataRow("En El Salvador")]
    [DataRow("En casa")]
    [DataRow("Estos pantalones le quedan demasiado cortos")]
    [DataRow("Un trago de vino")]
    [DataRow("Un trago de vino")]
    [DataRow("mío")]
    [DataRow("mi bolso")]
    [DataRow("su bolso")]
    [DataRow("nuestro bolso")]
    [DataRow("vuestro bolso")]
    public void WhenStringContainsSpanishCharactersOrCharacteristicWords_ShouldReturnTrue(string query)
    {
        // Arrange
        // Act
        bool result = StringHelpers.IsStringLikelyInSpanishLanguage(query);

        // Assert
        result.Should().BeTrue();
    }


    [DataTestMethod]
    [DataRow("Dzień dobry")]
    [DataRow("Nie wiem")]
    [DataRow("This is an English sentence")]
    [DataRow("laboratorium")]
    [DataRow("na Elce")]
    [DataRow("a car")]
    [DataRow("rozróżniać")]
    [DataRow("los cebula i krokodyle łzy")]
    [DataRow("las w którym straszy")]
    [DataRow("moje ulubione warzywo to por i seler")]
    public void WhenStringContainsPolishCharactersOrCharacteristicWords_ShouldReturnFalse(string query)
    {
        // Arrange
        // Act
        bool result = StringHelpers.IsStringLikelyInSpanishLanguage(query);

        // Assert
        result.Should().BeFalse();
    }

}
