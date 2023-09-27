using AlloyMvc.Models.Find;
using EPiServer.Find.Cms.Module;
using EPiServer.Find.Framework;
using EPiServer.Find;
using EPiServer.Framework.Initialization;
using EPiServer.Framework;
using EPiServer.ServiceLocation;
using EPiServer.Find.ClientConventions;
using EPiServer.Shell.UI.Messaging.Internal;
using EPiServer.Find.Cms;

namespace AlloyMvc.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(IndexingModule))]
    public class FindInitialization : IConfigurableModule
    {
        //public static Dictionary<string, Func<ISearchable, object>> OrderDictionary = new();
        public void InitializeConvention()
        {
            IncludeFields();
            ExcludeFields();
        }

        private void IncludeFields()
        {
            SearchClient.Instance.Conventions.ForInstancesOf<ICustomUnifiedSearchContent>().IncludeField(m => m.SearchText);
            SearchClient.Instance.Conventions.ForInstancesOf<ICustomUnifiedSearchContent>().IncludeField(m => m.SearchSummary);
            SearchClient.Instance.Conventions.ForInstancesOf<ICustomUnifiedSearchContent>().IncludeField(m => m.SearchTitle);
            SearchClient.Instance.Conventions.ForInstancesOf<object>().FieldsOfType<string>().StripHtml();
            //SearchClient.Instance.Conventions.ForInstancesOf<ISearchable>().IncludeField(m => m.GetTypesName());
            //SearchClient.Instance.Conventions.ForInstancesOf<ISearchable>().IncludeField(m => m.GetCategoryIds());
        }
        private void ExcludeFields()
        {
            //SearchClient.Instance.Conventions.ForInstancesOf<IContent>().ExcludeField(x => x.ContentTypeName());
            //SearchClient.Instance.Conventions.ForInstancesOf<ICategorizable>().ExcludeField(m => m.Category);
            //SearchClient.Instance.Conventions.ForInstancesOf<IContent>().ExcludeField(m => m.ContentApiModel());
        }

        //public static void RegisterOrderField()
        //{
        //    OrderDictionary.Add(nameof(PageData.StartPublish), m => m.PublishDate());
        //    OrderDictionary.Add(nameof(PageData.Name), m => ((IContent)m).Name);
        //}

        public void Initialize(InitializationEngine context)
        {
            InitializeConvention();
        }


        public void Uninitialize(InitializationEngine context)
        {
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.Configure<FindOptions>(options =>
            {
                options.IndexQueueInterval = 10;
            });
        }
    }
}
