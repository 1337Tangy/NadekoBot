﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Modules;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;
using System.Net.Http;
using Discord.Legacy;
using Discord.Commands;

namespace NadekoBot.Modules
{
    class Searches : DiscordModule
    {
        public Searches() : base()
        {

        }

        public override void Install(ModuleManager manager)
        {
            var client = NadekoBot.client;

            manager.CreateCommands("",cgb =>
            {
                cgb.CreateCommand("~yt")
                    .Parameter("query",Discord.Commands.ParameterType.Unparsed)
                    .Description("Queries youtubes and embeds the first result")
                    .Do(async e =>
                    {
                        if (!(await ValidateQuery(e.Channel, e.GetArg("query")))) return;

                        var str = ShortenUrl(FindYoutubeUrlByKeywords(e.GetArg("query")));
                        if (string.IsNullOrEmpty(str.Trim()))
                        {
                            await e.Send( "Query failed");
                            return;
                        }
                        await e.Send( str);
                    });

                cgb.CreateCommand("~ani")
                    .Alias("~anime").Alias("~aq")
                    .Parameter("query", Discord.Commands.ParameterType.Unparsed)
                    .Description("Queries anilist for an anime and shows the first result.")
                    .Do(async e =>
                    {
                        if (!(await ValidateQuery(e.Channel, e.GetArg("query")))) return;

                        var result = GetAnimeQueryResultLink(e.GetArg("query"));
                        if (result == null) { 
                            await e.Send( "Failed to find that anime.");
                            return;
                        }

                        await e.Send(result.ToString());
                    });

                cgb.CreateCommand("~mang")
                    .Alias("~manga").Alias("~mq")
                    .Parameter("query", Discord.Commands.ParameterType.Unparsed)
                    .Description("Queries anilist for a manga and shows the first result.")
                    .Do(async e =>
                    {
                        if (!(await ValidateQuery(e.Channel, e.GetArg("query")))) return;

                        var result = GetMangaQueryResultLink(e.GetArg("query"));
                        if (result == null)
                        {
                            await e.Send( "Failed to find that anime.");
                            return;
                        }
                        await e.Send( result.ToString());
                    });

                cgb.CreateCommand("~randomcat")
                    .Description("Shows a random cat image.")
                    .Do(async e => {
                        try {
                            await e.Send(JObject.Parse(new StreamReader(
                                WebRequest.Create("http://www.random.cat/meow")
                                    .GetResponse()
                                    .GetResponseStream())
                                .ReadToEnd())["file"].ToString());
                        } catch (Exception) { }
                    });

                cgb.CreateCommand("~i")
                   .Description("Pulls a first image using a search parameter.\n**Usage**: @NadekoBot img Multiword_search_parameter")
                   .Alias("img")
                   .Parameter("all", ParameterType.Unparsed)
                       .Do(async e => {
                           await e.Send("This feature is being reconstructed.");
                           /*
                           var httpClient = new System.Net.Http.HttpClient();
                           string str = e.Args[0];

                           var r = httpClient.GetAsync("http://ajax.googleapis.com/ajax/services/search/images?v=1.0&q=" + Uri.EscapeDataString(str) + "&start=0").Result;

                           dynamic obj = JObject.Parse(r.Content.ReadAsStringAsync().Result);
                           if (obj.responseData.results.Count == 0)
                           {
                               await e.Send("No results found for that keyword :\\");
                               return;
                           }
                           string s = Searches.ShortenUrl(obj.responseData.results[0].url.ToString());
                           await e.Send(s);
                           */
                       });

                cgb.CreateCommand("~ir")
                    .Description("Pulls a random image using a search parameter.\n**Usage**: @NadekoBot img Multiword_search_parameter")
                    .Alias("imgrandom")
                    .Parameter("all", ParameterType.Unparsed)
                    .Do(async e => {
                        await e.Send("This feature is being reconstructed.");
                        /*
                        var httpClient = new System.Net.Http.HttpClient();
                        string str = e.Args[0];
                        var r = httpClient.GetAsync("http://ajax.googleapis.com/ajax/services/search/images?v=1.0&q=" + Uri.EscapeDataString(str) + "&start=" + rng.Next(0, 30)).Result;
                        JObject obj = JObject.Parse(r.Content.ReadAsStringAsync().Result);
                        try
                        {
                            Console.WriteLine(obj.ToString());
                            if (obj["responseData"]["results"].Count() == 0)
                            {
                                await e.Send("No results found for that keyword :\\");
                                return;
                            }
                            int rnd = rng.Next(0, obj["responseData"]["results"].Count());
                            string s = Searches.ShortenUrl(obj["responseData"]["results"][rnd]["url"].ToString());
                            await e.Send(s);
                        }
                        catch (Exception ex) {
                            Console.WriteLine(ex.ToString());
                        }
                        */
                    });
            });
        }

        private string token = "";
        private AnimeResult GetAnimeQueryResultLink(string query)
        {
            try
            {
                var cl = new RestSharp.RestClient("https://anilist.co/api");
                var rq = new RestSharp.RestRequest("/auth/access_token", RestSharp.Method.POST);

                RefreshToken();

                rq = new RestSharp.RestRequest("/anime/search/" + Uri.EscapeUriString(query));
                rq.AddParameter("access_token", token);

                var smallObj = JArray.Parse(cl.Execute(rq).Content)[0];

                rq = new RestSharp.RestRequest("anime/" + smallObj["id"]);
                rq.AddParameter("access_token", token);
                return JsonConvert.DeserializeObject<AnimeResult>(cl.Execute(rq).Content);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private MangaResult GetMangaQueryResultLink(string query)
        {
            try
            {
                RefreshToken();

                var cl = new RestSharp.RestClient("https://anilist.co/api");
                var rq = new RestSharp.RestRequest("/auth/access_token", RestSharp.Method.POST);
                rq = new RestSharp.RestRequest("/manga/search/"+Uri.EscapeUriString(query));
                rq.AddParameter("access_token", token);
                
                var smallObj = JArray.Parse(cl.Execute(rq).Content)[0];

                rq = new RestSharp.RestRequest("manga/" + smallObj["id"]);
                rq.AddParameter("access_token", token);
                return JsonConvert.DeserializeObject<MangaResult> (cl.Execute(rq).Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private void RefreshToken()
        {
            var cl = new RestSharp.RestClient("https://anilist.co/api");
            var rq = new RestSharp.RestRequest("/auth/access_token", RestSharp.Method.POST);
            rq.AddParameter("grant_type", "client_credentials");
            rq.AddParameter("client_id", "kwoth-w0ki9");
            rq.AddParameter("client_secret", "Qd6j4FIAi1ZK6Pc7N7V4Z");
            token = JObject.Parse(cl.Execute(rq).Content)["access_token"].ToString();
        }

        private static async Task<bool> ValidateQuery(Discord.Channel ch,string query) {
            if (string.IsNullOrEmpty(query.Trim()))
            {
                await ch.Send("Please specify search parameters.");
                return false;
            }
            return true;
        }

        public static string FindYoutubeUrlByKeywords(string v)
        {
            WebRequest wr = WebRequest.Create("https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=1&q=" + Uri.EscapeDataString(v) + "&key=" + NadekoBot.GoogleAPIKey);

            var sr = new StreamReader(wr.GetResponse().GetResponseStream());

            dynamic obj = JObject.Parse(sr.ReadToEnd());
            return "http://www.youtube.com/watch?v=" + obj.items[0].id.videoId.ToString();
        }

        public static string ShortenUrl(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=" + NadekoBot.GoogleAPIKey);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"longUrl\":\"" + url + "\"}";
                streamWriter.Write(json);
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    string MATCH_PATTERN = @"""id"": ?""(?<id>.+)""";
                    return Regex.Match(responseText, MATCH_PATTERN).Groups["id"].Value;
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); return ""; }
        }
    }
}
