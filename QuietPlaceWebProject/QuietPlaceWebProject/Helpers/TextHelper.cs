using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Helpers
{
    public static class TextHelper
    {
        // public static MvcHtmlString CreateList(this HtmlHelper htmlHelper, IEnumerable<IPost> posts)
        // {
        //     var tagMainRowBuilder = new TagBuilder("div");
        //     tagMainRowBuilder.Attributes.Add(new KeyValuePair<string, string>("class", "row"));
        //     
        //     var count = 1;
        //
        //     foreach (var post in posts)
        //     {
        //         var tagDivContainerBuilder = new TagBuilder("div");
        //         tagDivContainerBuilder.Attributes.Add(new KeyValuePair<string, string>("class", "container"));
        //         
        //         var tagDivRowBuilders = new TagBuilder[2];
        //         var tagDivColBuilders = post.IsOriginalPoster ? new TagBuilder[8] : new TagBuilder[7];
        //
        //         for (var i = 0; i < tagDivRowBuilders.Length; ++i)
        //         {
        //             tagDivRowBuilders[i] = new TagBuilder("div");
        //             tagDivRowBuilders[i].Attributes.Add(new KeyValuePair<string, string>("class", "row"));
        //         }
        //         
        //         for (var i = 0; i < tagDivColBuilders.Length; ++i)
        //         {
        //             tagDivColBuilders[i] = new TagBuilder("div");
        //             tagDivColBuilders[i].Attributes.Add(new KeyValuePair<string, string>("class", "col-sm-auto"));
        //         }
        //
        //         tagDivColBuilders[0].SetInnerText("Аноним");
        //         tagDivColBuilders[1].SetInnerText(post.IsOriginalPoster ? "ОП" : "NON_OP");
        //         tagDivColBuilders[2].SetInnerText(post.DateOfCreation.ToString(CultureInfo.CurrentCulture));
        //         tagDivColBuilders[3].SetInnerText(post.Id.ToString());
        //         tagDivColBuilders[4].SetInnerText(count.ToString());
        //         tagDivColBuilders[5].InnerHtml = GetAnswerButton(post.ThreadId, post.Id);
        //         tagDivColBuilders[6].InnerHtml = GetRemoveButton(post.Id);
        //
        //         foreach (var tagBuilder in tagDivColBuilders)
        //             tagDivRowBuilders[0].InnerHtml += tagBuilder.ToString();
        //         
        //         tagDivColBuilders[7].SetInnerText(BuildText(post.Text));
        //         tagDivRowBuilders[1].InnerHtml = tagDivColBuilders[7].ToString();
        //
        //         foreach (var tagBuilder in tagDivRowBuilders)
        //             tagDivContainerBuilder.InnerHtml += tagBuilder.ToString();
        //
        //         tagMainRowBuilder.InnerHtml += tagDivContainerBuilder.ToString();
        //
        //         ++count;
        //     }
        //     
        //     return new MvcHtmlString(tagMainRowBuilder.ToString());
        // }

        public static string BuildText(string text)
        {
            var tagABuilder = new TagBuilder("a");
            tagABuilder.Attributes.Add(new KeyValuePair<string, string>("style", "color: #ff6600"));
            var tagSpanBuilder = new TagBuilder("span");
            tagSpanBuilder.Attributes.Add(new KeyValuePair<string, string>("style", "color: #008000"));
            var answerIdsStrings = GetAnswerIdsStrings(text);
            var greenStrings = GetGreenTextStrings(text);

            foreach (var match in answerIdsStrings)
            {
                tagABuilder.SetInnerText(match.Value);
                tagABuilder.Attributes.Add(new KeyValuePair<string, string>("href", $"#post {match.Value}"));

                text = text.Replace(match.Value, tagABuilder.ToString());
            }

            foreach (var match in greenStrings)
            {
                tagSpanBuilder.SetInnerText(match.Value);
                
                text = text.Replace(match.Value, tagSpanBuilder.ToString());
            }

            return text;
        }
        
        private static IEnumerable<Match> GetAnswerIdsStrings(string text)
        {
            var regex = new Regex(@"&gt;&gt;\d+");
            var matches = regex.Matches(text);

            return matches.ToList();
        }

        private static IEnumerable<Match> GetGreenTextStrings(string text)
        {
            var regex = new Regex(@"^&gt;\w+$", RegexOptions.Multiline);
            var matches = regex.Matches(text);

            return matches.ToList();
        }

        // private static string GetAnswerButton(int threadId, int postId)
        // {
        //     var tagBuilder = new TagBuilder("a");
        //     tagBuilder.Attributes.Add(new KeyValuePair<string, string>("asp-action", "ToAnswer"));
        //     tagBuilder.Attributes.Add(new KeyValuePair<string, string>("asp-route-threadId", threadId.ToString()));
        //     tagBuilder.Attributes.Add(new KeyValuePair<string, string>("asp-route-postId", postId.ToString()));
        //     tagBuilder.SetInnerText("Ответить");
        //
        //     return tagBuilder.ToString();
        // }
        //
        // private static string GetRemoveButton(int postId)
        // {
        //     var tagBuilder = new TagBuilder("a");
        //     tagBuilder.Attributes.Add(new KeyValuePair<string, string>("asp-action", "Remove"));
        //     tagBuilder.Attributes.Add(new KeyValuePair<string, string>("asp-route-postId", postId.ToString()));
        //     tagBuilder.SetInnerText("Удалить пост");
        //
        //     return tagBuilder.ToString();
        // }
    }
}