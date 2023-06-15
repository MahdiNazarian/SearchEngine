using BussinessLogic.Helpers;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLogic.Indexer
{
    public class IndexerBLL
    {
        private static HttpClientHandler handler = new HttpClientHandler()
        {
            UseDefaultCredentials = true,
            AllowAutoRedirect = false,
        };
        static readonly HttpClient client = new HttpClient(handler);
        private readonly SearchEngineDbContext _context;
        private List<Links> _IndexQue = new List<Links>();
        public IndexerBLL(SearchEngineDbContext context)
        {
            _context = context;
            _IndexQue = _context.links.Where(x => !x.Indexed && (x.Status.Equals((int)StaticValues.LinkStates.Extracted) || x.Status.Equals((int)StaticValues.LinkStates.Valid) ) ).ToList();
        }
        public async Task Indexer(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            List<string> tokens = new List<string>();
            while (true)
            {
                for(int i = 0; i < _IndexQue.Count; i++)
                {
                    try
                    {
                        tokens = await GetTokens(_IndexQue[i]);
                        await InsertTokensInPostingList(tokens, _IndexQue[i]);
                        _IndexQue[i].Indexed = true;
                        _context.links.Update(_IndexQue[i]);
                        await _context.SaveChangesAsync();
                        ct.ThrowIfCancellationRequested();
                    }
                    catch(Exception ex)
                    {
                        throw;
                    }
                }
                _IndexQue = _context.links.Where(x => !x.Indexed && (x.Status.Equals((int)StaticValues.LinkStates.Extracted) || x.Status.Equals((int)StaticValues.LinkStates.Valid))).ToList();
                if (_IndexQue.Count == 0)
                {
                    break;
                }
                else
                {
                    Task.Delay(1000).Wait();
                }
                ct.ThrowIfCancellationRequested();
            }
        }
        public async Task<List<string>> GetTokens(Links link)
        {
            List<string> tokens = new List<string>();
            try
            {
                using HttpResponseMessage response = await client.GetAsync(link.Url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                switch (link.DocumentType)
                {
                    case (int)StaticValues.SupportedFileTypes.html:
                        HtmlDocument source = new HtmlDocument();
                        source.LoadHtml(responseBody);
                        tokens.AddRange(GetHtmlTokens(source));
                        break;
                    case (int)StaticValues.SupportedFileTypes.pdf:
                        break;
                    case (int)StaticValues.SupportedFileTypes.txt:
                        List<string> TextFileTokens = responseBody.Split(new char[] { ' ', '.', '?', ',', '/', '(', ')', '\n', '\r', '\\', ':', ';', '\'', '\"', '!', '،' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        TextFileTokens = TextFileTokens.Except(StaticValues.StopWords).ToList().ConvertAll(x => x.ToLower());
                        tokens.AddRange(TextFileTokens);
                        break;
                    case (int)StaticValues.SupportedFileTypes.MSword:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return tokens;
        }
        public List<string> GetHtmlTokens(HtmlDocument document)
        {
            List<string> tokens = new List<string>();
            List<HtmlNode> documentAnchors = (document.DocumentNode.SelectNodes("//a") != null) ? document.DocumentNode.SelectNodes("//a").Where(x => x.HasChildNodes).Where(x=> x.FirstChild.Name.Equals("#text")).ToList() : new List<HtmlNode>();
            List<HtmlNode> documentParagraphs = (document.DocumentNode.SelectNodes("//p") != null) ? document.DocumentNode.SelectNodes("//p").Where(x => x.HasChildNodes).Where(x => x.FirstChild.Name.Equals("#text")).ToList() : new List<HtmlNode>();
            List<HtmlNode> documentPre = (document.DocumentNode.SelectNodes("//pre")!= null)?document.DocumentNode.SelectNodes("//pre").Where(x=> x.HasChildNodes).Where(x => x.FirstChild.Name.Equals("#text")).ToList() : new List<HtmlNode>();
            List<HtmlNode> documentSpans = (document.DocumentNode.SelectNodes("//span") != null) ? document.DocumentNode.SelectNodes("//span").Where(x => x.HasChildNodes).Where(x => x.FirstChild.Name.Equals("#text")).ToList() : new List<HtmlNode>();
            List<List<HtmlNode>> htmlNodes = new List<List<HtmlNode>>()
            {
                documentAnchors,
                documentParagraphs,
                documentPre,
                documentSpans
            };
            foreach(List<HtmlNode> htmlNode in htmlNodes)
            {
                try
                {
                    foreach (HtmlNode htmlNodeSingle in htmlNode)
                    {
                        tokens.AddRange(htmlNodeSingle.InnerText.ReplaceLineEndings().Split(new char[] { ' ', '.', '?', ',', '/', '(', ')', '\n', '\r', '\\', ':', ';', '\'', '\"', '!' , '،'}, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                catch(Exception ex)
                {
                    throw;
                }
            }
            tokens = tokens.Except(StaticValues.StopWords).ToList().ConvertAll(x => x.ToLower());
            return tokens;
        }
        public async Task InsertTokensInPostingList(List<string> tokens , Links link)
        {
            List<Dictionary<string, dynamic>> TokensCount = new List<Dictionary<string, dynamic>>();
            foreach(string token in tokens)
            {
                try
                {
                    if (TokensCount.Count > 0)
                    {
                        if (TokensCount.Where(x => x["value"] == token).Count() > 0)
                        {
                            TokensCount.Where(x => x["value"] == token).FirstOrDefault()["count"]++;
                        }
                        else
                        {
                            TokensCount.Add(new Dictionary<string, dynamic>()
                        {
                            {"value" , token },
                            {"count" , 1 }
                        });
                        }
                    }
                    else
                    {
                        TokensCount.Add(new Dictionary<string, dynamic>()
                        {
                            {"value" , token },
                            {"count" , 1 }
                        });
                    }
                }
                catch(Exception ex)
                {
                    throw;
                }
            }
            foreach(Dictionary<string, dynamic> token in TokensCount)
            {
                string value = token["value"];
                int count = token["count"];
                try
                {
                    if (_context.postingList.Where(x => x.Indexs.Term.Equals(value) && x.Links.ID.Equals(link.ID)).ToList().Count != 0)
                    {
                        PostingList existingPostingList = _context.postingList.Where(x => x.Indexs.Term.Equals(value) && x.Links.ID.Equals(link.ID)).FirstOrDefault();
                        if(count > existingPostingList.TermFrequency)
                        {
                            existingPostingList.TermFrequency = count;
                            _context.postingList.Update(existingPostingList);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        if (_context.indexes.Where(x => x.Term.Equals(value)).ToList().Count > 0)
                        {
                            Indexes index = _context.indexes.Where(x => x.Term.Equals(value)).FirstOrDefault();
                            await _context.postingList.AddAsync(new PostingList()
                            {
                                Guid = Guid.NewGuid(),
                                Indexs = index,
                                Links = link,
                                TermFrequency = count
                            });
                            _context.SaveChanges();
                        }
                        else
                        {
                            Indexes NewIndex = new Indexes()
                            {
                                Guid = Guid.NewGuid(),
                                Term = value
                            };
                            await _context.indexes.AddAsync(NewIndex);
                            _context.SaveChanges();
                            Indexes createdIndex = _context.indexes.Where(x => x.Term.Equals(value)).FirstOrDefault();
                            if (createdIndex != null)
                            {
                                await _context.postingList.AddAsync(new PostingList()
                                {
                                    Guid = Guid.NewGuid(),
                                    Indexs = createdIndex,
                                    Links = link,
                                    TermFrequency = count
                                });
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw;
                }
            }
        }
    }
}
