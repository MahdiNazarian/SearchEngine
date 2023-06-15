using BussinessLogic.BLLModels;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLogic.Ranking
{
    public class RankingBLL
    {
        private readonly SearchEngineDbContext _context;
        public RankingBLL(SearchEngineDbContext context)
        {
            _context = context;
        }
        public List<RankedLink> Rank(string query)
        {
            List<string> queryTerms = new List<string>();
            queryTerms.AddRange(query.Split(new char[] { ' ', '.', '?', ',', '/', '(', ')', '\n', '\r', '\\', ':', ';', '\'', '\"', '!', '،' }, StringSplitOptions.RemoveEmptyEntries));
            return GetDocumentScore(queryTerms);

        }
        public int GetDocumentFrequency(string term)
        {
            return _context.postingList.Where(x => x.Indexs.Term.Equals(term)).ToList().Count;
        }
        public double GetInverseDocumentFrequency(int documentFrequency)
        {
            int totalDocumentsCount = _context.links.ToList().Count;
            return Math.Log10(totalDocumentsCount / documentFrequency);
        }
        public int GetTermFrequency(string term,Links document)
        {
            return _context.postingList.Where(x => x.Links.ID.Equals(document.ID) && x.Indexs.Term.Equals(term)).FirstOrDefault().TermFrequency;
        }
        public double GetTermWeight(int termFrequency)
        {
            if(termFrequency > 0)
            {
                return 1+Math.Log10(termFrequency);
            }else
                return 0;
        }
        public List<RankedLink> GetDocumentScore(List<string> queryTerms) 
        {
            List<RankedLink> result = new List<RankedLink>();
            List<Links> containsTerms = new List<Links>();
            double score = 0;
            foreach(string term in queryTerms)
            {
                List<PostingList> documentsContainsTerm = _context.postingList.Include(x => x.Indexs).Include(x => x.Links).Where(x => x.Indexs.Term.Equals(term)).ToList();
                foreach(PostingList document in documentsContainsTerm)
                {
                    score = GetTermWeight(document.TermFrequency) + GetInverseDocumentFrequency(GetDocumentFrequency(term));
                    if (result != null)
                    {
                        if (result.Where(x => x.link.ID.Equals(document.Links.ID)).ToList().Count > 0)
                        {
                            result.Where(x => x.link.ID.Equals(document.Links.ID)).FirstOrDefault().score += score;
                        }
                        else
                        {
                            result.Add(new RankedLink()
                            {
                                link = document.Links,
                                score = score
                            });
                        }
                    }
                    else
                    {
                        result.Add(new RankedLink()
                        {
                            link = document.Links,
                            score = score
                        });
                    }
                }
            }
            return result.OrderByDescending(x => x.score).ToList();
        }

    }
}
