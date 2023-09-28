using AlloyMvc.Models.Find;
using AlloyMvc.Models.Pages;
using AlloyMvc.Models.ViewModels;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Find.UnifiedSearch;
using EPiServer.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace AlloyMvc.Controllers
{
    public class SearchPageController : PageControllerBase<SearchPage>
    {
        private readonly IClient client;

        public SearchPageController(IClient client)
        {
            this.client = client;
        }
        public ViewResult Index(SearchPage currentPage, string q)
        {
            //client.Conventions.UnifiedSearchRegistry.Add<ICustomUnifiedSearchContent>().ProjectTitleFrom(x => x.SearchTitle);

            var findClient = client.UnifiedSearch(ContentLanguage.PreferredCulture.GetLanguage())
                .For(q)
                .WithAndAsDefaultOperator()
                .UsingUnifiedWeights()
                .Include(x => x.SearchTitle.AnyWordBeginsWith(q), 2)
                .BoostMatching(x => x.MatchType(typeof(ProductPage)), 4)
                .GetResult(
                new HitSpecification
                {
                    HighlightExcerpt = true,
                    HighlightTitle = true,
                    ExcerptLength = 400,
                    PreTagForAllHighlights = "<em><b>",
                    PostTagForAllHighlights = "</em></b>",
                    EncodeExcerpt = true,
                    EncodeTitle = true,
                });

            var hits = findClient.Select(x =>
            {
                return new SearchContentModel.SearchHit
                {
                    Title = x.Title,
                    Excerpt = x.Excerpt,
                    Url = x.Url
                };
            });

            var model = new SearchContentModel(currentPage)
            {
                Hits = hits,
                NumberOfHits = hits.Count(),
                SearchServiceDisabled = false,
                SearchedQuery = q
            };

            return View(model);
        }
    }
}