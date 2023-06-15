using BussinessLogic.Helpers;
using HtmlAgilityPack;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLogic.Crawler
{
    public class CrawlerBLL
    {
        private static HttpClientHandler handler = new HttpClientHandler()
        {
            UseDefaultCredentials = true,
            AllowAutoRedirect = false,
        };
        static readonly HttpClient client = new HttpClient(handler);
        private readonly SearchEngineDbContext _context;
        private List<Links> _links = new List<Links>();
        public CrawlerBLL(SearchEngineDbContext context)
        {
            _context = context;
            _links = _context.links.Where(x => x.Status.Equals((int)StaticValues.LinkStates.Valid)).ToList();
        }
        public async Task ExtractLinks(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                throw;
            }
            List<Links> returnedLinks = new List<Links>();
            while (true)
            {
                for (int i = 0; i<_links.Count; i++)
                {
                    try
                    {
                        returnedLinks = await GetPageLinks(_links[i]);
                        _links[i].Status = (int)StaticValues.LinkStates.Extracted;
                        _context.links.Update(_links[i]);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    try
                    {
                        ct.ThrowIfCancellationRequested();
                    }catch(Exception ex)
                    {
                        throw;
                    }
                }
                _links = _context.links.Where(x => x.Status.Equals((int)StaticValues.LinkStates.Valid)).ToList();
                if(_links.Count == 0)
                {
                    break;
                }
                else
                {
                    Task.Delay(1000).Wait();
                }
                try
                {
                    ct.ThrowIfCancellationRequested();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public async Task<List<Links>> GetPageLinks(Links link)
        {
            List<Links> documentLinks = new List<Links>();
            try
            {
                using HttpResponseMessage response = await client.GetAsync(link.Url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                string contentType = response.Content.Headers.GetValues("content-type").FirstOrDefault().Split(";").FirstOrDefault();
                switch (contentType)
                {
                    case "text/html":
                        HtmlDocument source = new HtmlDocument();
                        source.LoadHtml(responseBody);
                        documentLinks = await LinkExtractor(source);
                        break;
                    case "application/pdf":
                        break;
                    case "application/msword":
                        break;
                    case "text/plain":
                        break;
                    default:
                        break;
                }
                return documentLinks;
            }
            catch (Exception ex)
            {
                return new List<Links>();
            }

        }
        public async Task<List<Links>> LinkExtractor(HtmlDocument document)
        {
            List<Links> links = new List<Links>();
            List<Links> checkLinkExist = new List<Links>();
            List<HtmlNode> documentAnchors = (document.DocumentNode.SelectNodes("//a") != null) ? document.DocumentNode.SelectNodes("//a").ToList() : new List<HtmlNode>();
            foreach(HtmlNode htmlNode in documentAnchors)
            {
                checkLinkExist = _context.links.Where(x => x.Url == htmlNode.Attributes["href"].Value).ToList();
                if (checkLinkExist.Count == 0)
                {
                    Links workingLink = new Links()
                    {
                        Guid = Guid.NewGuid(),
                        Url = htmlNode.Attributes["href"].Value,
                        Status = (int)StaticValues.LinkStates.Valid,
                        Description = string.Empty,
                        Title = string.Empty,
                        Keywords = string.Empty,
                        DocumentType = 0
                    };
                    if (Uri.IsWellFormedUriString(htmlNode.Attributes["href"].Value, UriKind.Absolute))
                    {
                        try
                        {
                            workingLink = await LinkValidator(workingLink);
                            await _context.links.AddAsync(workingLink);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            workingLink.Status = (int)StaticValues.LinkStates.Invalid;
                            workingLink.Description = ex.Message;
                            await _context.links.AddAsync(workingLink);
                            await _context.SaveChangesAsync();
                        }
                        links.Add(workingLink);
                    }
                    else
                        continue;
                }
                else
                {
                    continue;
                }
            }
            return links;
        }
        public async Task<Links> LinkValidator(Links link)
        {
            Links checkingLink = link;
            try
            {
                using HttpResponseMessage response = await client.GetAsync(checkingLink.Url);
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.GetValues("content-type").FirstOrDefault() is null or "" || response.Content is null)
                {
                    checkingLink.Status = (int)StaticValues.LinkStates.Invalid;
                    checkingLink.Description = "content type or content body not found";
                }
                else {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    checkingLink.DocumentType = DetermineContentType(response.Content.Headers.GetValues("content-type").FirstOrDefault().Split(";").FirstOrDefault());
                    switch (checkingLink.DocumentType)
                    {
                        case (int)StaticValues.SupportedFileTypes.html:
                            HtmlDocument source = new HtmlDocument();
                            source.LoadHtml(responseBody);
                            checkingLink.Title = GetDocumentTitle(source);
                            checkingLink.Keywords = GetDocumentKeyWords(source);
                            break;
                        case (int)StaticValues.SupportedFileTypes.txt:
                            checkingLink.Title = checkingLink.Url.Split("/").LastOrDefault().Split(".").FirstOrDefault();
                            checkingLink.Keywords = "";
                            break;
                        case (int)StaticValues.SupportedFileTypes.pdf:
                            checkingLink.Title = checkingLink.Url.Split("/").LastOrDefault().Split(".").FirstOrDefault();
                            checkingLink.Keywords = "";
                            break;
                        case (int)StaticValues.SupportedFileTypes.MSword:
                            checkingLink.Title = checkingLink.Url.Split("/").LastOrDefault().Split(".").FirstOrDefault();
                            checkingLink.Keywords = "";
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                checkingLink.Status = (int)StaticValues.LinkStates.Invalid;
                checkingLink.Description = ex.Message;
                throw;
            }
            return checkingLink;
        }
        public string GetDocumentTitle(HtmlDocument document)
        {
            HtmlNode titleNode = document.DocumentNode.SelectNodes("//title").FirstOrDefault();
            if (titleNode != null)
                return titleNode.InnerText;
            else
                return string.Empty;
        }
        public string GetDocumentKeyWords(HtmlDocument document)
        {
            List<HtmlNode> MetaNodes = document.DocumentNode.SelectNodes("//meta").ToList();
            if (MetaNodes != null)
            {
                IEnumerable<HtmlNode> keywordsMetaNodeList = MetaNodes.Where(x => x.Attributes != null && x.Attributes["name"] != null);
                if (keywordsMetaNodeList != null)
                {
                    HtmlNode keywordsMetaNode = keywordsMetaNodeList.Where(x => x.Attributes["name"].Equals("keywords")).FirstOrDefault();
                    if (keywordsMetaNode != null)
                        return keywordsMetaNode.Attributes["content"].ToString();
                    else
                        return "";
                }
                else
                {
                    return "";
                }
            }
            else
                return string.Empty;
        }
        public int DetermineContentType(string contentType)
        {
            switch (contentType)
            {
                case "text/html":
                    return (int)StaticValues.SupportedFileTypes.html;
                case "application/pdf":
                    return (int)StaticValues.SupportedFileTypes.pdf;
                case "application/msword":
                    return (int)StaticValues.SupportedFileTypes.MSword;
                case "text/plain":
                    return (int)StaticValues.SupportedFileTypes.txt;
                default:
                    return (int)StaticValues.SupportedFileTypes.html;
            }
        }
        public async Task<Links> AddSeed(string link)
        {
            List<Links> checkLink = _context.links.Where(x => x.Url == link).ToList();
            if (checkLink.Count ==0)
            {
                Links NewLink = new Links()
                {
                    Guid = Guid.NewGuid(),
                    Url = link,
                    Status = (int)StaticValues.LinkStates.Valid,
                    DocumentType = 0,
                    Keywords = string.Empty,
                    Title = string.Empty,
                    Description = string.Empty
                };
                try
                {
                    Links GetLink = await LinkValidator(NewLink);
                    await _context.links.AddAsync(GetLink);
                    _context.SaveChanges();
                    return GetLink;
                }
                catch (Exception ex)
                {
                    NewLink.Status = (int)StaticValues.LinkStates.Invalid;
                    NewLink.Description = ex.Message;
                    await _context.links.AddAsync(NewLink);
                    _context.SaveChanges();
                    throw;
                }
            }
            else
            {
                throw new Exception("لینکی با این مشخصات قبلا ثبت شده است");
            }
        }
    }
}
