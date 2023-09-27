using AlloyMvc.Atrributes;
using AlloyMvc.Extensions;
using AlloyMvc.Models.Find;

namespace AlloyMvc.Models.Blocks
{
    /// <summary>
    /// Base class for all block types on the site
    /// </summary>
    public abstract class SiteBlockData : BlockData
    {
        //[Ignore]
        //public string SearchTitle => (this as IContent)?.Name;
        //[Ignore]
        //public string SearchText => this.GetText<SearchTextAttribute>();
        //[Ignore]
        //public string SearchSummary => this.GetText<SearchSummaryAttribute>();
    }
}