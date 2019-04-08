using Cortana_Assistant_Alexa_Sample.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace Cortana_Assistant_Alexa_Sample
{
    public class GoldRatesParser
    {
        public static GoldRate goldRate;
        static DateTime lastUpdated = DateTime.Now;
        
        public static async Task<GoldRate> GetRates()
        {
            if (DateTime.Now.Subtract(lastUpdated).Days <= 1 && goldRate != null)
                return goldRate;

            Stream stream = await (await new HttpClient().GetAsync("http://www.goldrate24.com/")).Content.ReadAsStreamAsync();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.Load(stream);
            try
            {

                var nodes = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'now table table-striped table-hover table-bordered table-light')]");
                goldRate = new GoldRate();
                goldRate.Carat24 = nodes[0].SelectNodes("//th[contains(text(), ' Gold Gram 24K')]")[0].NextSibling.InnerHtml;
                goldRate.Carat22 = nodes[0].SelectNodes("//th[contains(text(), ' Gold Gram 22K')]")[0].NextSibling.InnerHtml;
                lastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return goldRate;
        }
    }
}
