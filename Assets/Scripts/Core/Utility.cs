public static class NumberUtility
{
    public static string FormatNumber(float number)
    {
        return number switch
        {
            >= 1000000 => $"{number / 1000000f:0.##}M",
            >= 1000 => $"{number / 1000f:0.##}K",
            _ => number.ToString("0")
        };
    }
}