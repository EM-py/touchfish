using System;
using System.Linq;
using System.Collections.Generic;
namespace Touchfish {
public static class FreezeMageDeck {
 // Classic list: https://www.icy-veins.com/hearthstone/classic-freeze-mage-deck
 public static DeckDocument Create(CardCatalog catalog){
  var deck=new DeckDocument{ClassId="MAGE",PoolId="classic-2014",Name="经典冰法"};
  string[] doubles={"CS2_031","CS2_027","CS2_024","CS2_023","CS2_026","EX1_289","EX1_295","CS2_029","CS2_028","NEW1_021","EX1_096","EX1_007","EX1_284"};
  foreach(string key in doubles)deck.Cards.Add(catalog.Cards.Values.Single(c=>c.Collectible&&c.SetId=="classic-2014"&&MatchRules.EffectKey(c)==key).Id,2);
  foreach(string key in new[]{"CS2_032","EX1_279","EX1_012","EX1_561"})deck.Cards.Add(catalog.Cards.Values.Single(c=>c.Collectible&&c.SetId=="classic-2014"&&MatchRules.EffectKey(c)==key).Id,1);
  DeckRules.Validate(catalog,deck,true);return deck;
 }
}
public sealed class FreezeMageAgent : IMatchAgent {
 public int LastSearchNodes{get;private set;}
 public int LastDefenseNodes{get;private set;}
 readonly FreezeMageDefense defense=new FreezeMageDefense();
 public const int SearchNodeLimit=6000;
 public int[] Mulligan(AgentObservation v){
  bool aggressive=v.Enemy.ClassId=="HUNTER"||v.Enemy.ClassId=="WARLOCK"||v.Enemy.ClassId=="PALADIN"||v.Enemy.ClassId=="ROGUE";
  bool doom=v.Hand.Any(c=>c.Key=="NEW1_021");var kept=new HashSet<string>();
  return v.Hand.Where(c=>{bool keep=c.Key=="EX1_096"||c.Key=="CS2_023"||c.Key=="NEW1_021"||aggressive&&(c.Key=="CS2_024"||c.Key=="CS2_027")||!aggressive&&c.Key=="EX1_007"||doom&&aggressive&&c.Key=="CS2_026";return !keep||!(aggressive&&c.Key=="CS2_024")&&!kept.Add(c.Key);}).Select(c=>c.Id).ToArray();
 }
 public AgentMove Choose(AgentObservation v){
  if(!v.CanAct)return null;LastDefenseNodes=0;var lethal=Lethal(v);if(lethal!=null)return lethal;
  var defend=defense.Choose(v);LastDefenseNodes=defense.Nodes;if(defend!=null)return defend;
  return v.Moves.Select((m,n)=>new{Move=m,Score=Score(v,m),Index=n}).OrderByDescending(x=>x.Score).ThenBy(x=>x.Move.Cost).ThenBy(x=>x.Index).First().Move;
 }
 static double Threat(AgentPlayer p){return p.Board.Where(u=>!u.Frozen&&u.Key!="NEW1_021"&&u.Key!="EX1_298").Sum(u=>u.Attack*(u.Windfury?2:1))+p.Board.Count(u=>u.Key=="EX1_298"&&!u.Silenced)*8+(!p.Frozen?p.HeroAttack:0);}
 static double Value(AgentUnit u){return 3+u.Attack*1.6+Math.Min(6,u.Health)*0.35+(u.Key=="EX1_608"||u.Key=="EX1_095"||u.Key=="NEW1_019"?8:0);}
 static bool Doom(AgentPlayer p){return p.Board.Any(u=>u.Key=="NEW1_021"&&!u.Silenced);}
 static bool EnemyTarget(AgentObservation v,AgentMove m){return m.Target!=null&&m.Target.Seat!=v.Seat;}
 static bool Face(AgentObservation v,AgentMove m){return EnemyTarget(v,m)&&m.Target.Hero;}
 static int Burn(string key){return key=="CS2_029"?6:key=="CS2_024"?3:key=="EX1_279"?10:0;}
 static int HandBurn(AgentObservation v){int damage=v.Hand.Where(h=>Burn(h.Key)>0).Sum(h=>Burn(h.Key)+v.SpellPower),lances=v.Hand.Count(h=>h.Key=="CS2_031");bool freeze=v.Enemy.Frozen||v.Hand.Any(h=>h.Key=="CS2_024");return damage+Math.Max(0,lances-(freeze?0:1))*(4+v.SpellPower);}
 static bool BurnReady(AgentObservation v){int hp=v.Enemy.Health+v.Enemy.Armor;if(v.Enemy.Immune||v.Enemy.Health>15&&v.Hand.Any(h=>h.Key=="EX1_561"))return false;return hp<=10||HandBurn(v)>=hp&&(hp<=15||v.Self.DeckCount<=4);}
 static double DrawScore(AgentObservation v,int count,double score){
  if(v.Self.HandCount-1+count>10)return -100;
  int excess=Math.Max(0,count-v.Self.DeckCount),fatigue=excess*(2*v.Self.Fatigue+excess+1)/2;
  if(fatigue>0)return -100-fatigue*20; // Ice Block does not protect own-turn fatigue.
  if(v.Self.DeckCount<=count)return -45;
  if(v.Self.DeckCount<=4&&(HandBurn(v)>=v.Enemy.Health+v.Enemy.Armor||v.Self.HandCount>=5))return -35;
  return score;
 }
 double Score(AgentObservation v,AgentMove m){
  double threat=Threat(v.Enemy),hp=v.Self.Health+v.Self.Armor;bool danger=threat>0&&threat>=hp-3,doom=Doom(v.Self),block=v.OwnSecrets.Contains("EX1_295");
  var target=m.Target==null||m.Target.Hero?null:(EnemyTarget(v,m)?v.Enemy:v.Self).Board.FirstOrDefault(u=>u.Id==m.Target.UnitId);
  if(m.Kind=="end")return 0;
  if(m.Kind=="attack"){
   var attacker=v.Self.Board.FirstOrDefault(u=>u.Id==m.Source);if(Face(v,m))return v.Enemy.Immune?-100:8+m.Damage;
   if(target==null)return -100;bool kills=!target.Shield&&(m.Damage>=target.Health||attacker!=null&&attacker.Poison);bool dies=attacker==null?target.Attack>=hp:!attacker.Shield&&(target.Attack>=attacker.Health||target.Poison);
   return (kills?Value(target):target.Shield?5:1)+(danger?target.Attack:0)-(dies?(attacker==null?100:Value(attacker)):0)+(doom?4:0);
  }
  if(m.Kind=="power"){
   if(Face(v,m))return v.Enemy.Immune?-100:BurnReady(v)?14:1;
   if(target==null)return -100;
   if(!EnemyTarget(v,m))return target.Key=="EX1_007"&&!target.Silenced&&target.Health>m.Damage?DrawScore(v,1,22):-100;
   return target.Shield?(target.Attack>0||target.Taunt?7:-3):target.Health<=m.Damage?Value(target)+6:danger?3:-1;
  }
  int damage=Burn(m.Key)+v.SpellPower;
  switch(m.Key){
   case "GAME_COIN":return CoinValue(v,m);
   case "EX1_561":if(!m.Target.Hero)return -100;if(EnemyTarget(v,m))return v.Enemy.Health>15?(danger&&!block?-10:40+(v.Enemy.Health-15)*1.2):-90;return Math.Min(15,v.Self.HeroMaxHealth)-v.Self.Health>3?(danger?90:35)+Math.Min(15,v.Self.HeroMaxHealth)-v.Self.Health:-90;
   case "EX1_295":return block?-100:danger?100:hp<=15||v.Self.MaxMana>=7?32:5;
   case "EX1_289":return danger?55:threat>=3?20:hp<=20?10:2;
   case "CS2_026":return FreezeValue(v,threat,doom,damage:0)+(HasDoomCombo(v,m)?35:0);
   case "CS2_028":return AreaValue(v,2+v.SpellPower)+FreezeValue(v,threat,doom,2+v.SpellPower)-12;
   case "CS2_032":return AreaValue(v,4+v.SpellPower)-10;
   case "NEW1_021":return doom?-70:v.Enemy.Board.Length==0?(v.Self.MaxMana<=2?8:-20):threat==0?45+v.Enemy.Board.Sum(u=>Value(u))*0.4:HasFreezeCombo(v,m)?65:threat<7?25+threat:danger?12:-10;
   case "CS2_027":return v.Self.Board.Length>=6?-10:danger?45:threat>=4?18:4;
   case "CS2_023":return danger&&!block?-15:DrawScore(v,2,28+(7-v.Self.HandCount)*2);
   case "EX1_096":return doom?-15:DrawScore(v,1,25);
   case "EX1_007":return doom?-15:DrawScore(v,1,26);
   case "EX1_284":return doom?-15:DrawScore(v,1,25);
   case "EX1_012":return doom?-15:v.Hand.Any(h=>Burn(h.Key)>0&&h.Cost<=v.Self.Mana-m.Cost)?26:18;
   case "CS2_031":if(target==null)return Face(v,m)&&v.Enemy.Frozen&&!v.Enemy.Immune&&BurnReady(v)?28:-35;if(!EnemyTarget(v,m))return -100;return target.Frozen?Removal(v,target,4+v.SpellPower,threat,hp):!target.Frozen&&target.Attack>0?(danger?45:target.Attack>=4?target.Attack*2:-10):-20;
   case "CS2_024":case "CS2_029":case "EX1_279":
    if(Face(v,m))return !v.Enemy.Immune&&(!danger||block)&&BurnReady(v)?28+damage*1.2:-25;
    if(target==null||!EnemyTarget(v,m))return -100;
    return Removal(v,target,damage,threat,hp)+(m.Key=="CS2_024"&&!target.Frozen?target.Attack*(danger?3:1):0)-(m.Key=="EX1_279"?15:0);
   default:return -10;
  }
 }
 static double Removal(AgentObservation v,AgentUnit u,int damage,double threat,double hp){bool urgent=threat>0&&threat>=hp-3;if(u.Attack==0&&(!u.Taunt||!v.Moves.Any(m=>m.Kind=="attack"&&m.Damage>=3)))return -20;return u.Shield?-8:damage>=u.Health?Value(u)+(urgent?25:5)-(Math.Max(0,damage-u.Health)*1.5)-(urgent?0:damage*1.7):urgent?4:-12;}
 static double AreaValue(AgentObservation v,int damage){return v.Enemy.Board.Sum(u=>u.Shield?2:damage>=u.Health?Value(u)+4:2)+(Threat(v.Enemy)>0&&Threat(v.Enemy)>=v.Self.Health+v.Self.Armor-3?20:0);}
 static double FreezeValue(AgentObservation v,double threat,bool doom,int damage){double stopped=v.Enemy.Board.Where(u=>!u.Frozen&&(u.Shield||u.Health>damage)).Sum(u=>u.Attack);if(stopped==0)return -35;return (doom?65:0)+stopped*(threat>0&&threat>=v.Self.Health+v.Self.Armor-3?5:2)-12;}
 static bool HasDoomCombo(AgentObservation v,AgentMove m){return !Doom(v.Self)&&v.Self.Board.Length<7&&v.Hand.Any(h=>h.Key=="NEW1_021"&&h.Cost<=v.Self.Mana-m.Cost)&&Threat(v.Enemy)>=4;}
 static bool HasFreezeCombo(AgentObservation v,AgentMove m){return v.Hand.Any(h=>(h.Key=="CS2_026"||h.Key=="CS2_028")&&h.Cost<=v.Self.Mana-m.Cost)&&Threat(v.Enemy)>=4;}
 double CoinValue(AgentObservation v,AgentMove coin){if(v.Self.Mana>=10)return -50;var extra=v.Hand.Where(h=>h.Cost==v.Self.Mana+1&&h.Id!=coin.Source);if(extra.Any(h=>h.Key=="EX1_561"&&v.Enemy.Health>15))return 38;if(extra.Any(h=>h.Key=="CS2_032"||h.Key=="CS2_028")&&Threat(v.Enemy)>=5)return 48;if(extra.Any(h=>h.Key=="CS2_023"||h.Key=="EX1_284"||h.Key=="EX1_096"||h.Key=="EX1_007")&&v.Self.HandCount<=7)return 24;if(extra.Any(h=>h.Key=="EX1_295")&&Threat(v.Enemy)>=v.Self.Health+v.Self.Armor-3)return 95;return -20;}
 // Bounded exact ordering search for currently known face damage. Replan after every
 // real action: draws, enemy secrets and other hidden outcomes are never simulated.
 AgentMove Lethal(AgentObservation v){
  LastSearchNodes=0;if(v.Enemy.Immune)return null;
  var resources=new List<AgentMove>();
  foreach(var h in v.Hand)if(Burn(h.Key)>0||h.Key=="CS2_031"||h.Key=="GAME_COIN"||h.Key=="EX1_012"&&v.Self.Board.Length<7)resources.Add(new AgentMove{Kind="play",Source=h.Id,Key=h.Key,Cost=h.Cost,Target=h.Key=="GAME_COIN"||h.Key=="EX1_012"?null:new MatchTarget(1-v.Seat),Position=h.Key=="EX1_012"?v.Self.Board.Length:-1});
  resources.AddRange(v.Moves.Where(m=>m.Kind=="attack"&&Face(v,m)));
  if(!v.Self.PowerUsed)resources.Add(new AgentMove{Kind="power",Cost=v.PowerCost,Damage=v.PowerDamage,Target=new MatchTarget(1-v.Seat)});
  if(resources.Count>24)return null;
  var failed=new HashSet<string>();AgentMove first=null;
  Search(v,resources,0,v.Self.Mana,v.SpellPower,v.Enemy.Frozen,v.Enemy.Health+v.Enemy.Armor,null,failed,ref first);return first;
 }
 bool Search(AgentObservation v,List<AgentMove> r,int used,int mana,int power,bool frozen,int hp,AgentMove start,HashSet<string> failed,ref AgentMove answer){
  if(LastSearchNodes>=SearchNodeLimit)return false;LastSearchNodes++;if(hp<=0){answer=start;return true;}
  string state=used+":"+mana+":"+power+":"+frozen+":"+hp;if(!failed.Add(state))return false;
  for(int n=0;n<r.Count&&LastSearchNodes<SearchNodeLimit;n++){if((used&(1<<n))!=0)continue;var m=r[n];if(m.Cost>mana)continue;if(start==null&&!v.Moves.Any(a=>Same(a,m)))continue;
   int nextMana=mana-m.Cost,nextPower=power,hit=0;bool nextFrozen=frozen;
   if(m.Kind=="attack"||m.Kind=="power")hit=m.Damage;
   else if(m.Key=="GAME_COIN"){if(mana>=10)continue;nextMana=Math.Min(10,mana+1);}
   else if(m.Key=="EX1_012")nextPower++;
   else if(m.Key=="CS2_031"){hit=frozen?4+power:0;nextFrozen=true;}
   else {hit=Burn(m.Key)+power;if(m.Key=="CS2_024")nextFrozen=true;}
   if(Search(v,r,used|(1<<n),nextMana,nextPower,nextFrozen,hp-hit,start??m,failed,ref answer))return true;
  }return false;
 }
 static bool Same(AgentMove a,AgentMove b){return a.Kind==b.Kind&&a.Source==b.Source&&(a.Target==null?b.Target==null:b.Target!=null&&a.Target.Seat==b.Target.Seat&&a.Target.UnitId==b.Target.UnitId);}
}
}
