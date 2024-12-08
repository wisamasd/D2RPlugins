namespace D2R_Plugins.Helpers;

public static class ExstensionsHelper
{
    public static bool Invert(this bool value) => !value;

    public static string[] Skip(this string[] val, int skipCount)
    {
        var count = val.Length - skipCount;

        if (count <= 0) return new string[0];

        var result = new string[count];

        var indexTo = 0;
        for (var i = skipCount; i < val.Length; i++ ) 
        {
            result[indexTo] = val[i];
            indexTo++;
        }

        return result;
    }
}
