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
                    finalDamage = hit.Damage * 0.3f;
                break;

            case HitType.Pierce:
                if (data.Material == ObjectMaterial.Default)
                    finalDamage = hit.Damage;
                else if (data.Material == ObjectMaterial.Wood)
                    finalDamage = hit.Damage * 0.5f;
                break;

            case HitType.Blunt:
                if (data.Material == ObjectMaterial.Default)
                    finalDamage = hit.Damage;
                else if (data.Material == ObjectMaterial.Wood)
                    finalDamage = hit.Damage * 0.5f;
                else if (data.Material == ObjectMaterial.Stone)
                    finalDamage = hit.Damage * 3;
                else if (data.Material == ObjectMaterial.Metal)
                    finalDamage = hit.Damage * 0.3f;
                break;
        }

        return new(finalDamage, true);
    }
}