namespace AlloyMvc.Atrributes
{
    public class SearchTextAttribute : SearchTextBaseAttribute, IUsingSearchTextAsDefault
    {
        public SearchTextAttribute(bool isSeachable = true) : base(isSeachable) { }
    }
}
