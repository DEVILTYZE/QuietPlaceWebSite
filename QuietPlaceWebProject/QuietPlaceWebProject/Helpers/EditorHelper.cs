using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.AspNetCore.Html;

namespace QuietPlaceWebProject.Helpers
{
    public static class EditorHelper
    {
        public static HtmlString BuildEditorTools()
        {
            var row = new TagBuilder("div");
            row.AddCssClass("row");
            
            var styles = new[] {"underline", "overline", "line-through"};
            var divs = new TagBuilder[6];
            var tools = new TagBuilder[6];
            var classAttribute = new KeyValuePair<string, string>("class", "btn");
            var styleAttributes = new KeyValuePair<string, string>[3];

            for (var i = 0; i < styleAttributes.Length; ++i)
                styleAttributes[i] = new KeyValuePair<string, string>("style", $"text-decoration: {styles[i]}");

            for (var i = 0; i < divs.Length; ++i)
                divs[i] = GetTagBuilder("div", null, classAttribute);

            tools[0] = GetTagBuilder("b", "BOLD");
            tools[1] = GetTagBuilder("i", "ITALIC");
            tools[2] = GetTagBuilder("div", "UNDERLINE", styleAttributes[0]);
            tools[3] = GetTagBuilder("div", "OVERLINE", styleAttributes[1]);
            tools[4] = GetTagBuilder("div", "LINE-THROUGH", styleAttributes[2]);
            tools[5] = GetTagBuilder("div", "SPOILER", 
                new KeyValuePair<string, string>("onmouseenter", "unspoiler(this)"));
            tools[5].AddCssClass("spoiler");
            tools[5].Attributes.Add(new KeyValuePair<string, string>("onmouseout", "spoiler(this)"));

            for (var i = 0; i < divs.Length; ++i)
            {
                divs[i].InnerHtml = tools[i].ToString();
                row.InnerHtml += divs[i].ToString();
            }

            return new HtmlString(row.ToString());
        }

        private static TagBuilder GetTagBuilder(string tagName, string innerText = null,
            KeyValuePair<string, string> attribute = default)
        {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.SetInnerText(innerText);
            tagBuilder.Attributes.Add(attribute);

            return tagBuilder;
        }
    }
}