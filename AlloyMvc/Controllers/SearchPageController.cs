using AlloyMvc.Models.Find;
using AlloyMvc.Models.Pages;
using AlloyMvc.Models.ViewModels;
using EPiServer.Find;
using EPiServer.Find.Api;
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
            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new SearchContentModel(currentPage)
                {
                    Hits = new List<SearchContentModel.SearchHit>(),
                    NumberOfHits = 0,
                    SearchServiceDisabled = false,
                    SearchedQuery = ""
                });
            }

            var findClient = client.UnifiedSearch(ContentLanguage.PreferredCulture.GetLanguage())
                .For(q)
                .WithAndAsDefaultOperator()
                .UsingUnifiedWeights()
                .UsingSynonyms()
                .ApplyBestBets()
                .Include(x => x.SearchTitle.AnyWordBeginsWith(q))
                .BoostMatching(x => x.MatchType(typeof(ProductPage)), 4)
                .Filter(x => x.MatchTypeHierarchy(typeof(PageData)))
                .Filter(x => !((ProductPage)x).IsDiscontinued.Match(true))
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