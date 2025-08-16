using DocxJaTranslator.Core;
using Xunit;

namespace DocxJaTranslator.Tests;

public class DntProtectorTests
{
    [Fact]
    public void Mask_WithUrls_ShouldMaskUrls()
    {
        // Arrange
        var protector = new DntProtector();
        var text = "Visit https://example.com and http://test.org for more info.";

        // Act
        var masked = protector.Mask(text, out var tokenCount);

        // Assert
        Assert.Equal(2, tokenCount);
        Assert.Contains("{DNT0}", masked);
        Assert.Contains("{DNT1}", masked);
        Assert.DoesNotContain("https://example.com", masked);
        Assert.DoesNotContain("http://test.org", masked);
    }

    [Fact]
    public void Mask_WithEmails_ShouldMaskEmails()
    {
        // Arrange
        var protector = new DntProtector();
        var text = "Contact john.doe@example.com or jane@test.org";

        // Act
        var masked = protector.Mask(text, out var tokenCount);

        // Assert
        Assert.Equal(2, tokenCount);
        Assert.Contains("{DNT0}", masked);
        Assert.Contains("{DNT1}", masked);
        Assert.DoesNotContain("john.doe@example.com", masked);
        Assert.DoesNotContain("jane@test.org", masked);
    }

    [Fact]
    public void Mask_WithVersions_ShouldMaskVersions()
    {
        // Arrange
        var protector = new DntProtector();
        var text = "Version 1.2.3 and v2.0.1 are available";

        // Act
        var masked = protector.Mask(text, out var tokenCount);

        // Assert
        Assert.Equal(2, tokenCount);
        Assert.Contains("{DNT0}", masked);
        Assert.Contains("{DNT1}", masked);
        Assert.DoesNotContain("1.2.3", masked);
        Assert.DoesNotContain("v2.0.1", masked);
    }

    [Fact]
    public void Mask_WithNumbersAndUnits_ShouldMaskNumbersAndUnits()
    {
        // Arrange
        var protector = new DntProtector();
        var text = "Voltage: 220V, Current: 5A, Power: 1.5kW";

        // Act
        var masked = protector.Mask(text, out var tokenCount);

        // Assert
        Assert.Equal(3, tokenCount);
        Assert.Contains("{DNT0}", masked);
        Assert.Contains("{DNT1}", masked);
        Assert.Contains("{DNT2}", masked);
        Assert.DoesNotContain("220V", masked);
        Assert.DoesNotContain("5A", masked);
        Assert.DoesNotContain("1.5kW", masked);
    }

    [Fact]
    public void Mask_WithCode_ShouldMaskCode()
    {
        // Arrange
        var protector = new DntProtector();
        var text = "Use `getData()` function and <div> tags";

        // Act
        var masked = protector.Mask(text, out var tokenCount);

        // Assert
        Assert.Equal(2, tokenCount);
        Assert.Contains("{DNT0}", masked);
        Assert.Contains("{DNT1}", masked);
        Assert.DoesNotContain("`getData()`", masked);
        Assert.DoesNotContain("<div>", masked);
    }

    [Fact]
    public void Unmask_ShouldRestoreOriginalContent()
    {
        // Arrange
        var protector = new DntProtector();
        var originalText = "Visit https://example.com and contact john@test.org";
        var masked = protector.Mask(originalText, out var tokenCount);

        // Act
        var unmasked = protector.Unmask(masked);

        // Assert
        Assert.Equal(originalText, unmasked);
    }

    [Fact]
    public void Mask_WithMixedContent_ShouldHandleAllTypes()
    {
        // Arrange
        var protector = new DntProtector();
        var text = "API v1.2.3 at https://api.example.com uses 220V power and <config> tags";

        // Act
        var masked = protector.Mask(text, out var tokenCount);

        // Assert
        Assert.True(tokenCount > 0);
        Assert.DoesNotContain("v1.2.3", masked);
        Assert.DoesNotContain("https://api.example.com", masked);
        Assert.DoesNotContain("220V", masked);
        Assert.DoesNotContain("<config>", masked);
    }

    [Fact]
    public void Mask_WithNoDntContent_ShouldReturnOriginalText()
    {
        // Arrange
        var protector = new DntProtector();
        var text = "This is plain text without any special content.";

        // Act
        var masked = protector.Mask(text, out var tokenCount);

        // Assert
        Assert.Equal(0, tokenCount);
        Assert.Equal(text, masked);
    }
}
