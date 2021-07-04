using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.AspNetCore.Html;

namespace QuietPlaceWebProject.Helpers
{
    public static class EditorHelper
    {
        public static HtmlString BuildCaptchaTool(string imageName)
        {
            if (string.CompareOrdinal(imageName, "NULL") == 0)
                return new HtmlString(string.Empty);
            
            var img = new TagBuilder("img");
            img.Attributes.Add(new KeyValuePair<string, string>("src", "../captcha/images/" + imageName));

            var input = new TagBuilder("input");
            input.AddCssClass("bg-dark text-light");
            input.Attributes.Add(new KeyValuePair<string, string>("name", "captchaWord"));
            input.Attributes.Add(new KeyValuePair<string, string>("type", "text"));
            input.Attributes.Add(new KeyValuePair<string, string>("placeholder", "Введите капчу..."));

            var colImg = new TagBuilder("div");
            colImg.AddCssClass("col");
            colImg.InnerHtml = img.ToString();
            
            var rowImg = new TagBuilder("div");
            rowImg.AddCssClass("row");
            rowImg.InnerHtml = colImg.ToString();

            var colInput = new TagBuilder("div");
            colInput.AddCssClass("col");
            colInput.InnerHtml = input.ToString();
            
            var rowInput = new TagBuilder("div");
            rowInput.AddCssClass("row");
            rowInput.InnerHtml = colInput.ToString();
            
            var col = new TagBuilder("div");
            col.AddCssClass("col");
            col.InnerHtml = rowImg.ToString();
            col.InnerHtml += rowInput.ToString();
            
            var row = new TagBuilder("div");
            row.AddCssClass("row");
            row.InnerHtml += col.ToString();

            return new HtmlString(row.ToString());
        }
        
        public static HtmlString BuildMediafileTool()
        {
            var img = new TagBuilder("img");
            img.Attributes.Add(new KeyValuePair<string, string>("src", 
                "../images/file.png"));

            var div = new TagBuilder("div");

            var input = new TagBuilder("input");
            input.Attributes.Add(new KeyValuePair<string, string>("id", "inputFile"));
            input.Attributes.Add(new KeyValuePair<string, string>("type", "file"));

            var label = new TagBuilder("label");
            label.Attributes.Add(new KeyValuePair<string, string>("for", "inputFile"));
            label.SetInnerText("Выберите файл");

            var span = new TagBuilder("div");
            span.SetInnerText("или перетащите его сюда");

            div.InnerHtml += input.ToString(TagRenderMode.SelfClosing);
            div.InnerHtml += label.ToString();
            div.InnerHtml += span.ToString();

            var mainDiv = new TagBuilder("div");
            mainDiv.AddCssClass("upload-container");
            mainDiv.Attributes.Add(new KeyValuePair<string, string>(
                "id", "inputForm"));
            mainDiv.Attributes.Add(new KeyValuePair<string, string>(
                "ondragenter", "changeClass(this, 'dragover')"));
            mainDiv.Attributes.Add(new KeyValuePair<string, string>(
                "ondragleave", "changeClass(this, 'dragover')"));
            mainDiv.Attributes.Add(new KeyValuePair<string, string>(
                "ondragend", "changeClass(this, 'dragover')"));
            mainDiv.Attributes.Add(new KeyValuePair<string, string>(
                "ondrop", "dropHandler(event);"));
            mainDiv.InnerHtml += img.ToString(TagRenderMode.SelfClosing);
            mainDiv.InnerHtml += div.ToString();

            var col = new TagBuilder("div");
            col.AddCssClass("col");
            col.InnerHtml = mainDiv.ToString();
            
            var row = new TagBuilder("div");
            row.AddCssClass("row");
            row.InnerHtml += col.ToString();

            return new HtmlString(row.ToString());
        }
        
        public static HtmlString BuildEditorTools()
        {
            var styles = new[] {"underline", "overline", "line-through"};
            var divs = new TagBuilder[6];
            var tools = new TagBuilder[6];
            var classAttribute = new KeyValuePair<string, string>("class", "btn btn-dark");
            var styleAttributes = new KeyValuePair<string, string>[3];

            for (var i = 0; i < styleAttributes.Length; ++i)
                styleAttributes[i] = new KeyValuePair<string, string>("style", $"text-decoration: {styles[i]}");

            for (var i = 0; i < divs.Length; ++i)
                divs[i] = GetTagBuilder("div", null, classAttribute);

            tools[0] = GetTagBuilder("b", "BOLD", 
                new KeyValuePair<string, string>("onclick", "setTextTag(\'[b]\', \'[/b]\')"));
            
            tools[1] = GetTagBuilder("i", "ITALIC", 
                new KeyValuePair<string, string>("onclick" , "setTextTag(\'[i]\', \'[/i]\')"));
            
            tools[2] = GetTagBuilder("div", "UNDERLINE", styleAttributes[0]);
            tools[2].Attributes.Add(new KeyValuePair<string, string>("onclick", "setTextTag(\'[ul]\', \'[/ul]\')"));
            
            tools[3] = GetTagBuilder("div", "OVERLINE", styleAttributes[1]);
            tools[3].Attributes.Add(new KeyValuePair<string, string>("onclick", "setTextTag(\'[ol]\', \'[/ol]\')"));
            
            tools[4] = GetTagBuilder("div", "LINE-THROUGH", styleAttributes[2]);
            tools[4].Attributes.Add(new KeyValuePair<string, string>("onclick", "setTextTag(\'[lt]\', \'[/lt]\')"));
            
            tools[5] = GetTagBuilder("div", "SPOILER", 
                new KeyValuePair<string, string>("onmouseenter", "changeClass(this, 'spoiler')"));
            tools[5].AddCssClass("spoiler");
            tools[5].Attributes.Add(new KeyValuePair<string, string>(
                "onmouseleave", "changeClass(this, 'spoiler')"));
            tools[5].Attributes.Add(
                new KeyValuePair<string, string>("onclick", "setTextTag(\'[spoiler]\', \'[/spoiler]\')"));

            var btnGroup = new TagBuilder("div");
            btnGroup.AddCssClass("btn-group");
            
            for (var i = 0; i < divs.Length; ++i)
            {
                divs[i].InnerHtml = tools[i].ToString();
                btnGroup.InnerHtml += divs[i].ToString();
            }

            var col = new TagBuilder("div");
            col.AddCssClass("col");
            col.InnerHtml = btnGroup.ToString();
            
            var row = new TagBuilder("div");
            row.AddCssClass("row");
            row.InnerHtml = col.ToString();

            return new HtmlString(row.ToString());
        }

        private static TagBuilder GetTagBuilder(string tagName, string innerText,
            KeyValuePair<string, string> attribute = default)
        {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.SetInnerText(innerText);
            tagBuilder.Attributes.Add(attribute);

            return tagBuilder;
        }
    }
}