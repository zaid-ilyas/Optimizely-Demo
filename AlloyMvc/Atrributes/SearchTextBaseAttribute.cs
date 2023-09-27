namespace AlloyMvc.Atrributes
{
    public abstract class SearchTextBaseAttribute : Attribute
    {
        protected SearchTextBaseAttribute(bool isSeachable = true)
        {
            IsSearch = isSeachable;
        }
        public bool IsSearch { get; set; }
    }

    /// <summary>
    /// All text field will be included to SearchField
    /// </summary>
    public interface IUsingSearchTextAsDefault
    {

    }
}
