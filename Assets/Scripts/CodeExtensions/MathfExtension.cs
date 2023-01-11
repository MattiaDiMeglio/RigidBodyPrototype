using System.Runtime.CompilerServices;

public static class MathfExtension
{
    /// <summary>
    /// Ritorna true se il float ha valore tra min e max compresi
    /// </summary>
    /// <param name="val">Il valore che si vuole controllare</param>
    /// <param name="min">limite inferiore dell'intervallo</param>
    /// <param name="max">limite superiore dell'intervallo</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool Between(this float val, float min, float max) => (min<max? (val >= min && val <= max) : (val <= min && val >= max));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]public static bool IsOver(this float val, float direction, float max)
    {
        if(direction > 0f)
        {
            if(val > max) 
            { 
                return true; 
            } else
            {
                return false;
            }
        } else if(direction < 0f)
        {
            if (val < max)
            {
                return true;
            } else
            {
                return false;
            }
        }
        return false;
    }
}
