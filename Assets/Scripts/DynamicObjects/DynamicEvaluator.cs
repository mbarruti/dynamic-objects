public static class DynamicEvaluator
{
    public static HitEvaluation Evaluate(Hit hit, DynamicObject data)
    {
        float finalDamage = 0f;

        switch (hit.HitType)
        {
            case HitType.Slash:
                if (data.Material == ObjectMaterial.Default)
                    finalDamage = hit.Damage;
                else if (data.Material == ObjectMaterial.Wood)
                    finalDamage = 0f;
                break;

            case HitType.Pierce:
                if (data.Material == ObjectMaterial.Default)
                    finalDamage = hit.Damage;
                else if (data.Material == ObjectMaterial.Wood)
                    finalDamage = 1f;
                break;

            case HitType.Blunt:
                if (data.Material == ObjectMaterial.Default)
                    finalDamage = hit.Damage;
                else if (data.Material == ObjectMaterial.Wood)
                    finalDamage = 10f;
                else if (data.Material == ObjectMaterial.Stone)
                    finalDamage = 10f;
                else if (data.Material == ObjectMaterial.Metal)
                    finalDamage = 5f;
                break;
        }

        return new(finalDamage, true);
    }
}