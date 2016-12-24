namespace CKAN
{
    /// <summary>
    /// Allows certain modules to pass as compatible even though
    /// they might not be.
    /// </summary>
    public interface IGameComparatorExemptions
    {
        bool IsExempt(CkanModule module);
    }
}