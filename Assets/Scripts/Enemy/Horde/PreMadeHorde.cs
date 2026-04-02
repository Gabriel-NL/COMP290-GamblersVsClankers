using System;
using System.Collections.Generic;

[Serializable]
public sealed class PreMadeHorde
{
    public List<PreMadeSummon> summons = new List<PreMadeSummon>();

    public PreMadeHorde()
    {
    }

    public PreMadeHorde(params PreMadeSummon[] summons)
    {
        if (summons != null)
        {
            this.summons.AddRange(summons);
        }
    }
}
