namespace Domain.Other;

public static class DecimalMath
{
    private const decimal Epsilon = 0.000000000000000001m;
    private const decimal Ln2 = 0.693147180559945309m;

    public static decimal Sqrt(decimal value)
    {
        if (value < 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Square root is undefined for negative values.");

        if (value == 0m)
            return 0m;

        var estimate = value > 1m ? value : 1m;
        while (true)
        {
            var next = (estimate + value / estimate) / 2m;
            if (Math.Abs(next - estimate) <= Epsilon)
                return next;

            estimate = next;
        }
    }

    public static decimal Pow(decimal value, decimal exponent)
    {
        if (value < 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Negative bases are not supported for decimal power with fractional exponents.");

        if (value == 0m)
            return exponent > 0m ? 0m : throw new ArgumentOutOfRangeException(nameof(exponent), "Zero cannot be raised to a non-positive power.");

        if (exponent == 0m)
            return 1m;

        return Exp(exponent * Log(value));
    }

    private static decimal Exp(decimal value)
    {
        if (value == 0m)
            return 1m;

        if (value < 0m)
            return 1m / Exp(-value);

        var halvings = 0;
        while (value > 1m)
        {
            value /= 2m;
            halvings++;
        }

        decimal term = 1m;
        decimal sum = 1m;

        for (var i = 1; i < 80; i++)
        {
            term *= value / i;
            sum += term;

            if (Math.Abs(term) <= Epsilon)
                break;
        }

        while (halvings-- > 0)
        {
            sum *= sum;
        }

        return sum;
    }

    private static decimal Log(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Logarithm is undefined for non-positive values.");

        decimal scale = 0m;
        while (value > 1.5m)
        {
            value /= 2m;
            scale += Ln2;
        }

        while (value < 0.75m)
        {
            value *= 2m;
            scale -= Ln2;
        }

        var y = (value - 1m) / (value + 1m);
        var ySquared = y * y;

        decimal term = y;
        decimal series = y;

        for (var i = 3; i < 201; i += 2)
        {
            term *= ySquared;
            var addend = term / i;
            series += addend;

            if (Math.Abs(addend) <= Epsilon)
                break;
        }

        return scale + 2m * series;
    }
}