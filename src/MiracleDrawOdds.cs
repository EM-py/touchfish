using System;using System.Linq;using System.Collections.Generic;
namespace Touchfish {
// Finite-population probabilities over an owner-known multiset. Never stores a deck sequence.
public sealed class MiracleDrawOdds {
 readonly AgentDeckCard[] cards;public readonly int Total;public readonly bool Available;
 public MiracleDrawOdds(AgentDeckCard[] source,int expectedTotal){cards=(source??new AgentDeckCard[0]).Where(c=>c!=null&&c.Count>0).Select(c=>new AgentDeckCard{Key=c.Key,Type=c.Type,Count=c.Count,Cost=c.Cost}).ToArray();Total=cards.Sum(c=>c.Count);Available=source!=null&&expectedTotal>=0&&Total==expectedTotal&&source.All(c=>c!=null&&c.Count>=0&&c.Cost>=0&&!String.IsNullOrEmpty(c.Key));}
 internal IEnumerable<AgentDeckCard> Cards{get{return cards;}}
 public int Count(string key){return cards.Where(c=>c.Key==key).Sum(c=>c.Count);}
 public double Hit(int draws,params string[] keys){var wanted=new HashSet<string>(keys??new string[0]);return HitWhere(draws,c=>wanted.Contains(c.Key));}
 public double HitWhere(int draws,Func<AgentDeckCard,bool> predicate){if(!Available||Total==0)return 0;int successes=cards.Where(predicate).Sum(c=>c.Count);draws=Math.Max(0,Math.Min(Total,draws));double none=1;for(int i=0;i<draws;i++){int left=Total-successes-i;if(left<=0)return 1;none*=left/(double)(Total-i);}return 1-none;}
 public double Both(int draws,string first,string second){return first==second?Hit(draws,first):Math.Max(0,Hit(draws,first)+Hit(draws,second)-Hit(draws,first,second));}
}
}
