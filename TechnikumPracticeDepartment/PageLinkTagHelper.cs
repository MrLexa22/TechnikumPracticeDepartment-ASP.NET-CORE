using TechnikumPracticeDepartment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Diagnostics;

namespace TechnikumPracticeDepartment
{
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public PageViewModel PageModel { get; set; }
        public string PageAction { get; set; }

        [HtmlAttributeName(DictionaryAttributePrefix = "page-url-")]
        public Dictionary<string, object> PageUrlValues { get; set; } = new Dictionary<string, object>();

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            output.TagName = "div";

            // набор ссылок будет представлять список ul
            TagBuilder tag = new TagBuilder("ul");
            tag.AddCssClass("pagination");

            // формируем три ссылки - на текущую, предыдущую и следующую
            TagBuilder currentItem = CreateTag(PageModel.PageNumber, urlHelper);

            // создаем ссылку на предыдущую страницу, если она есть
            if (PageModel.HasPreviousPage)
            {
                TagBuilder prevItem = CreateTag(PageModel.PageNumber - 1, urlHelper);
                tag.InnerHtml.AppendHtml(prevItem);
            }

            tag.InnerHtml.AppendHtml(currentItem);
            // создаем ссылку на следующую страницу, если она есть
            if (PageModel.HasNextPage)
            {
                TagBuilder nextItem = CreateTag(PageModel.PageNumber + 1, urlHelper);
                tag.InnerHtml.AppendHtml(nextItem);
            }
            output.Content.AppendHtml(tag);
        }

        TagBuilder CreateTag(int pageNumber, IUrlHelper urlHelper)
        {
            TagBuilder form = new TagBuilder("form");
            form.Attributes["method"] = "get";
            form.Attributes["data-ajax"] = "true";
            form.Attributes["data-ajax-method"] = "get";
            form.Attributes["data-ajax-update"] = "#panel";
            form.Attributes["data-ajax-mode"] = "replace";
            TagBuilder button = new TagBuilder("input");
            if (pageNumber == this.PageModel.PageNumber)
            {
                form.AddCssClass("active");
                form.Attributes["data-ajax-url"] = urlHelper.Action(PageAction, PageUrlValues);
                button.Attributes["type"] = "submit";
                button.Attributes["active"] = "active";
                button.Attributes["value"] = pageNumber.ToString();
                form.InnerHtml.AppendHtml(button);
            }
            else
            {
                PageUrlValues["page"] = pageNumber;
                form.Attributes["data-ajax-url"] = urlHelper.Action(PageAction, PageUrlValues);
                button.Attributes["type"] = "submit";
                button.Attributes["value"] = pageNumber.ToString();
                form.InnerHtml.AppendHtml(button);
            }
            form.AddCssClass("page-item");
            button.AddCssClass("page-link");
            return form;
        }
    }
}
