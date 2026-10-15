using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Touchfish {
public sealed class ArenaDraftView {
 public string Session,Phase,ClassId;
 public string[] Choices,PickedCards;
 public int Picks,OpponentPicks,Revision;
 public bool Complete,OpponentComplete;
}

// Project defaults, not a claim to reproduce Blizzard's historical probabilities.
public sealed class ArenaDraftWeights {
 public double Common=70,Rare=20,Epic=8,Legendary=2,ClassBonus=1.5,SpellWeaponBonus=1.5;
 public bool RareMilestones=true;
 internal ArenaDraftWeights Copy(){return (ArenaDraftWeights)MemberwiseClone();}
 public static string Tier(CardRecord card){return card.Rarity=="RARE"||card.Rarity=="EPIC"||card.Rarity=="LEGENDARY"?card.Rarity:"COMMON";}
 internal double TierWeight(string tier){return tier=="RARE"?Rare:tier=="EPIC"?Epic:tier=="LEGENDARY"?Legendary:Common;}
 public double CardWeight(CardRecord card){return (card.CardClass=="NEUTRAL"?1:ClassBonus)*(card.Type=="SPELL"||card.Type=="WEAPON"?SpellWeaponBonus:1);}
 internal void Validate(){foreach(double n in new[]{Common,Rare,Epic,Legendary,ClassBonus,SpellWeaponBonus})if(Double.IsNaN(n)||Double.IsInfinity(n)||n<=0)throw new ArgumentException("竞技选牌权重必须为有限正数。");}
}
public sealed class ArenaDraftSession {
 public const int DraftSize=30,OfferSize=3;
 readonly CardCatalog catalog;
 readonly PoolDefinition pool;
 readonly Func<CardRecord,bool> supported;
 readonly Random random;
 readonly ArenaDraftWeights weights;
 readonly string[][] classes=new string[2][];
 readonly string[][] offers=new string[2][];
 readonly string[] chosenClasses=new string[2];
 readonly int[] picks=new int[2],revisions=new int[2];
 readonly List<string>[] cards={new List<string>(),new List<string>()};
 public readonly string Session=Guid.NewGuid().ToString("N");

 public ArenaDraftSession(CardCatalog source,int seed,Func<CardRecord,bool> isSupported,ArenaDraftWeights draftWeights=null){
  if(source==null||isSupported==null)throw new ArgumentNullException();catalog=source;pool=catalog.Pool("classic-2014");supported=isSupported;random=new Random(seed);
  weights=(draftWeights??new ArenaDraftWeights()).Copy();weights.Validate();
  string[] classIds=pool.HeroDbfIds.Keys.OrderBy(x=>x,StringComparer.Ordinal).ToArray();if(classIds.Length<OfferSize)throw new InvalidDataException("竞技模式需要至少三个可选职业。");
  var cardsInPool=catalog.Deckable(pool);foreach(var card in cardsInPool)if(!supported(card))throw new InvalidDataException("竞技卡池含暂不可用卡牌："+card.Name);
  for(int seat=0;seat<2;seat++)classes[seat]=Sample(classIds,OfferSize);
 }
 static string[] Sample(IList<string> source,int count,Random random){var bag=source.ToList();var result=new string[count];for(int i=0;i<count;i++){int index=random.Next(bag.Count);result[i]=bag[index];bag.RemoveAt(index);}return result;}
 string[] Sample(IList<string> source,int count){return Sample(source,count,random);}
 int WeightedIndex(IList<double> values){double target=random.NextDouble()*values.Sum();for(int i=0;i<values.Count;i++){target-=values[i];if(target<0)return i;}return values.Count-1;}
 string[] CardOffer(int seat){
  bool milestone=weights.RareMilestones&&(picks[seat]==0||picks[seat]==9||picks[seat]==19||picks[seat]==29);
  var groups=catalog.Deckable(pool).Where(c=>c.CardClass=="NEUTRAL"||c.CardClass==chosenClasses[seat]).GroupBy(ArenaDraftWeights.Tier).Where(g=>g.Count()>=OfferSize&&(!milestone||g.Key!="COMMON")).OrderBy(g=>g.Key,StringComparer.Ordinal).ToArray();
  if(groups.Length==0)throw new InvalidDataException("竞技稀有度分组不足三个可选卡牌。");
  var bag=groups[WeightedIndex(groups.Select(g=>weights.TierWeight(g.Key)).ToArray())].ToList();var result=new string[OfferSize];
  for(int i=0;i<OfferSize;i++){int index=WeightedIndex(bag.Select(weights.CardWeight).ToArray());result[i]=bag[index].Id;bag.RemoveAt(index);}return result;
 }
 public ArenaDraftView View(int seat){if(seat<0||seat>1)throw new ArgumentOutOfRangeException("seat");string phase=chosenClasses[seat]==null?"class":picks[seat]>=DraftSize?"complete":"cards";return new ArenaDraftView{Session=Session,Phase=phase,ClassId=chosenClasses[seat],Choices=phase=="class"?classes[seat].ToArray():phase=="cards"?offers[seat].ToArray():new string[0],PickedCards=cards[seat].ToArray(),Picks=picks[seat],OpponentPicks=picks[1-seat],Revision=revisions[seat],Complete=picks[seat]>=DraftSize,OpponentComplete=picks[1-seat]>=DraftSize};}
 public void Pick(int seat,string session,int revision,int index){if(seat<0||seat>1)throw new InvalidOperationException("选牌席位无效。");if(session!=Session||revision!=revisions[seat])throw new InvalidOperationException("竞技选择已更新，请重新选择。");if(chosenClasses[seat]==null){if(index<0||index>=classes[seat].Length)throw new InvalidOperationException("职业选项无效。");chosenClasses[seat]=classes[seat][index];offers[seat]=CardOffer(seat);}else{if(picks[seat]>=DraftSize||offers[seat]==null||index<0||index>=offers[seat].Length)throw new InvalidOperationException("卡牌选项无效。");cards[seat].Add(offers[seat][index]);picks[seat]++;if(picks[seat]<DraftSize)offers[seat]=CardOffer(seat);}revisions[seat]++;}
 public DeckDocument Deck(int seat){if(seat<0||seat>1)throw new ArgumentOutOfRangeException("seat");if(picks[seat]!=DraftSize||chosenClasses[seat]==null)throw new InvalidOperationException("需要先完成 30 次选牌。");var deck=new DeckDocument{PoolId=pool.Id,ClassId=chosenClasses[seat],Name="竞技卡组"};foreach(string id in cards[seat]){int count;if(!deck.Cards.TryGetValue(id,out count))count=0;deck.Cards[id]=count+1;}Validate(catalog,deck,true);return deck;}
 public static void Validate(CardCatalog catalog,DeckDocument deck,bool complete){
  if(catalog==null||deck==null)throw new InvalidDataException("竞技卡组无效。");var pool=catalog.Pool("classic-2014");if(deck.PoolId!=pool.Id||!pool.HeroDbfIds.ContainsKey(deck.ClassId))throw new InvalidDataException("竞技模式只支持经典卡池和已选择职业。");if(complete&&deck.Count!=DraftSize||deck.Count>DraftSize)throw new InvalidDataException("竞技卡组必须恰好包含 30 张牌。");var rules=new MatchRules(catalog);foreach(var pair in deck.Cards){CardRecord card;if(pair.Value<1||!catalog.Cards.TryGetValue(pair.Key,out card)||!card.Collectible||!pool.CardSetIds.Contains(card.SetId)||(card.Type!="MINION"&&card.Type!="SPELL"&&card.Type!="WEAPON")||card.CardClass!="NEUTRAL"&&card.CardClass!=deck.ClassId||!rules.Supports(card))throw new InvalidDataException("竞技卡组包含不允许或暂不可用的牌。");}
 }
}
}
