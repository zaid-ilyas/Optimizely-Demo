using AlloyMvc.Atrributes;
using EPiServer.Find.Helpers;
using EPiServer.Find.Helpers.Text;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;
using ILogger = EPiServer.Logging.ILogger;

namespace AlloyMvc.Extensions
{
    public static class PropertyExtensions
    {
        public static Lazy<ILogger> LoggerLazy = new(() => LogManager.GetLogger(typeof(PropertyExtensions)));

        public static ILogger Logger => LoggerLazy.Value;
        public static string GetText<T>(this IContentData contentData) where T : SearchTextBaseAttribute
        {
            if (contentData is IContent content)
            {
                return content.GetText<T>();
            }

            return string.Empty;
        }

        public static string GetText<T>(this IContent content, ConcurrentDictionary<ContentReference, string> contentWithText = null) where T : SearchTextBaseAttribute
        {
            try
            {
                if (content == null)
                {
                    return string.Empty;
                }
                contentWithText ??= new ConcurrentDictionary<ContentReference, string>();
                // prevent self reference
                if (contentWithText.TryGetValue(content.ContentLink, out var existedValue))
                {
                    return existedValue ?? string.Empty;
                }
                // mark content is getting text
                contentWithText.TryAdd(content.ContentLink, string.Empty);

                var result = new StringBuilder();
                var contentText = content.GetTextOfCurrentContent<T>();
                if (!string.IsNullOrWhiteSpace(contentText))
                {
                    result.Append(contentText);
                }

                var blockProps = content.Property.Where(m => m.Type == PropertyDataType.Block).ToList();
                var blockPropsStringList = blockProps.Select(b =>
                {
                    return (b.Value as IContentData).GetTextOfCurrentContent<T>();
                });
                var blockPropsString = string.Join(" ", blockPropsStringList);
                if (!string.IsNullOrWhiteSpace(blockPropsString))
                {
                    result.Append($" {blockPropsString}");
                }
                var contentAreasText = GetTextFromContentAreas<T>(content, contentWithText);
                if (!string.IsNullOrWhiteSpace(contentAreasText))
                {
                    result.Append($" {contentAreasText}");
                }
                var stringResult = FormatSearchText(result.ToString());
                //re assign content text value
                contentWithText[content.ContentLink] = stringResult;
                return stringResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"{typeof(PropertyExtensions)} Get search value", ex);
                return string.Empty;
            }
        }

        private static string GetTextFromContentAreas<T>(IContentData content, ConcurrentDictionary<ContentReference, string> contentWithText) where T : SearchTextBaseAttribute
        {
            if (content == null)
            {
                return string.Empty;
            }
            var result = new StringBuilder();
            var contentAreaProps = content.Property.Where(p => !p.IsMetaData && p.Value is ContentArea contentArea && contentArea.Items?.Any() == true).ToList();

            var contentReferencesInContentAreas = new List<ContentReference>();

            contentAreaProps.ForEach(m =>
            {
                var contentAreaItems = (m.Value as ContentArea)?.Items?.Select(i => i.ContentLink);
                (m.Value as ContentArea)?.Items?.ForEach(cAreaItem =>
                {
                    if (contentWithText.TryGetValue(cAreaItem.ContentLink, out var itemText))
                    {
                        if (!string.IsNullOrWhiteSpace(itemText))
                        {
                            result.Append($" {itemText}");
                        }
                    }
                    else
                    {
                        if (!ContentReference.IsNullOrEmpty(cAreaItem.ContentLink))
                        {
                            contentReferencesInContentAreas.Add(cAreaItem.ContentLink);
                        }
                    }
                });
            });

            // only get text if item is blockdata, if is page => wont add to search text
            var itemsToGetText = ServiceLocator.Current.GetInstance<IContentRepository>()
                .GetItems(contentReferencesInContentAreas.Distinct(), CultureInfo.InvariantCulture).OfType<BlockData>();

            var contentListText = itemsToGetText.Select(m => (m as IContent).GetText<T>());
            var contentAreaText = string.Join(" ", contentListText);
            if (!string.IsNullOrWhiteSpace(contentAreaText))
            {
                result.Append(contentAreaText);
            }
            return FormatSearchText(result.ToString());
        }

        public static string GetTextOfCurrentContent<T>(this IContentData content) where T : SearchTextBaseAttribute
        {
            //var propertyNameList = new List<string>();
            //foreach (var property in content.Property)
            //{
            //    if (PropertyOfContentIsSearchable<T>(property, content))
            //    {
            //        propertyNameList.Add(property.Name);
            //    }
            //}

            //var textProps = new List<PropertyData>();
            //foreach (var property in content.Property)
            //{
            //    if (PropertyOfContentIsSearchable<T>(property, content))
            //    {
            //        if (property.Name != "PageLink" && property.Name != "PageTypeName" && property.Name != "PageName" && property.Name != "MetaTitle")
            //        {
            //            textProps.Add(property);
            //        }
            //    }
            //}

            var textProps = content.Property.Where(m => PropertyOfContentIsSearchable<T>(m, content)).ToList();
            var result = new StringBuilder();

            textProps.ForEach(textProp =>
            {
                if (textProp.Name != "PageLink" && textProp.Name != "PageTypeName" && textProp.Name != "PageName" && textProp.Name != "MetaTitle")
                {
                    var textValueOfProp = textProp.Value as string;
                    if (textValueOfProp.IsNotNullOrEmpty())
                    {
                        result.Append($" {textValueOfProp}");
                    }
                }
            });
            return FormatSearchText(result.ToString());
        }

        private static bool PropertyOfContentIsSearchable<T>(PropertyData propertyData, IContentData content) where T : SearchTextBaseAttribute
        {
            if (propertyData == null || content == null)
            {
                return false;
            }
            var reflectProps = content.GetOriginalType().GetProperties();
            var reflectProp =
                reflectProps.FirstOrDefault(r => r.Name.Equals(propertyData.Name, StringComparison.CurrentCultureIgnoreCase));
            return reflectProp.PropertyIsSearchable<T>();
        }

        private static bool PropertyIsSearchable<T>(this PropertyInfo propertyInfo) where T : SearchTextBaseAttribute
        {
            if (propertyInfo == null)
            {
                return false;
            }
            var searchAttribute = propertyInfo.GetCustomAttribute<SearchTextBaseAttribute>();
            if (searchAttribute == null)
            {
                var usingAsDefault = typeof(IUsingSearchTextAsDefault).IsAssignableFrom(typeof(T));
                return usingAsDefault;
            }
            return searchAttribute.IsSearch;
        }

        private static string FormatSearchText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = text.Trim();
            while (text.IndexOf("  ", StringComparison.OrdinalIgnoreCase) > -1)
            {
                text = text.Replace("  ", " ");
            }

            return text;
        }
    }
}
