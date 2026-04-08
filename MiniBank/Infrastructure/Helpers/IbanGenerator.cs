namespace Infrastructure.Helpers;

public static class IbanGenerator
{
    public static string GenerateFakeIban()
    {
        var random = new Random();
        var digits = string.Concat(Enumerable.Range(0, 24)
            .Select(_ => random.Next(0, 10).ToString()));

        return "TR" + digits;
    }
}
