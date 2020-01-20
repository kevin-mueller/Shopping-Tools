﻿using System.Linq;
using System.Threading.Tasks;
using Shopping_Tools_Api_Services.Config;
using Shopping_Tools_Api_Services.Core;
using Shopping_Tools_Api_Services.Models;

namespace Shopping_Tools_Api_Services
{
    public static class Digitec
    {
        public static async Task<Product> GetProductInfo(string productUrl)
        {
            if (Validation.IsValidDigitecUrl(productUrl))
            {
                //Problem: CSS classes are randomized => use index

                var doc = await HttpHelper.GetDocument(productUrl);
                var productTitleNode = doc.DocumentNode.Descendants().First(x => x.HasClass(DigitecWebConstatnts.ProductDetailClassName)).Descendants("h1").ToList()[DigitecWebConstatnts.ProductNameH1Index];

                string brand = productTitleNode.Descendants("strong").TryFirst().InnerText;
                string productName = productTitleNode.Descendants("span").TryFirst().InnerText;

                //There could be a progress bar (when the product is on sale)
                //remove it.
                try
                {
                    var pbar = doc.DocumentNode.Descendants().First(x => x.HasClass(DigitecWebConstatnts.ProductDetailClassName)).Descendants("strong").First(x => x.InnerText.StartsWith("noch"));
                    if (pbar != null)
                        pbar.ParentNode.Remove();
                }
                catch
                {
                    //ignored (there is no progress bar continue)
                }

                //There could also be a label with "-15%"
                //remove it
                try
                {
                    var label = doc.DocumentNode.Descendants().First(x => x.HasClass(DigitecWebConstatnts.ProductDetailClassName)).Descendants("div").First(x => x.InnerText.EndsWith("%"));
                    if (label != null)
                        label.ParentNode.Remove();
                } catch
                {
                    //ignored (there is no label)
                }


                var productPriceNode = doc.DocumentNode.Descendants().First(x => x.HasClass(DigitecWebConstatnts.ProductDetailClassName)).Descendants("div").ToList()[DigitecWebConstatnts.ProductPriceDivIndex];

                string priceCurrent = productPriceNode.Descendants("strong").TryFirst().InnerText;
                string priceOld = productPriceNode.Descendants("span").Where(x => x.HasClass(DigitecWebConstatnts.ProductPriceOldClassName)).TryFirst().InnerText;

                Product productInfo = new Product()
                {
                    Brand = brand.Trim(),
                    Name = productName.Trim(),
                    PriceCurrent = priceCurrent.Trim(),
                    PriceOld = priceOld.Trim(),
                    ProductId = productUrl,
                    ProductIdSimple = Helpers.ExtractIdFromUrl(productUrl),
                    Url = productUrl,
                };
                return await Task.FromResult(productInfo);
            }
            return null;
        }
    }
}