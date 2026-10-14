using System;using System.Linq;
namespace Touchfish {
public sealed class MiracleRogueAgent : IMatchAgent {
 readonly MiracleBurstPlanner burst=new MiracleBurstPlanner();readonly MiracleTurnPlanner turn=new MiracleTurnPlanner();public int LastSearchNodes{get{return burst.Nodes;}}public int LastTurnSearchNodes{get{return turn.Nodes;}}
 public int[] Mulligan(AgentObservation v){
  bool fast=v.Enemy.ClassId=="WARLOCK"||v.Enemy.ClassId=="HUNTER"||v.Enemy.ClassId=="PALADIN";
  bool auction=v.Hand.Any(h=>h.Key=="EX1_095"),si=v.Hand.Any(h=>h.Key=="EX1_134"),edwin=!si&&v.Hand.Any(h=>h.Key=="EX1_613")&&(v.Seat!=v.FirstSeat||v.Hand.Any(h=>h.Key=="CS2_072"));var seen=new System.Collections.Generic.HashSet<string>();
  return v.Hand.Where(h=>{bool keep=h.Key=="CS2_074"&&fast||h.Key=="CS2_072"||h.Key=="EX1_134"||h.Key=="EX1_613"&&edwin||h.Key=="CS2_117"&&fast&&!si&&!edwin||h.Key=="EX1_124"&&fast||h.Key=="EX1_095"&&!fast||h.Key=="EX1_145"&&(!fast&&auction||fast&&v.Hand.Any(c=>c.Key=="EX1_124"));return !keep||!seen.Add(h.Key);}).Select(h=>h.Id).ToArray();
 }
 public AgentMove Choose(AgentObservation v){if(!v.CanAct)return null;var lethal=burst.Choose(v);if(lethal!=null)return lethal;var planned=turn.Choose(v);if(planned!=null)return planned;return v.Moves.Select((m,n)=>new{Move=m,Index=n,Score=Score(v,m)-DrawRisk(v,m)}).OrderByDescending(x=>x.Score).ThenBy(x=>x.Move.Cost).ThenBy(x=>x.Index).First().Move;}
 static double DrawRisk(AgentObservation v,AgentMove m){if(m.Kind!="play")return 0;var card=v.Hand.First(h=>h.Id==m.Source);int n=card.Type=="SPELL"?v.Self.Board.Count(u=>u.Key=="EX1_095"&&!u.Silenced):0;n+=m.Key=="CS2_077"?4:m.Key=="EX1_129"||m.Key=="EX1_278"||m.Key=="EX1_284"?1:0;int missing=Math.Max(0,n-v.Self.DeckCount),hit=missing*(2*v.Self.Fatigue+missing+1)/2;return hit>=v.Self.Health+v.Self.Armor?10000:hit*.7+Math.Max(0,v.Self.HandCount-1+Math.Min(n,v.Self.DeckCount)-10)*8;}
 static bool Face(AgentObservation v,AgentMove m){return m.Target!=null&&m.Target.Seat!=v.Seat&&m.Target.Hero;}
 static AgentUnit Target(AgentObservation v,AgentMove m){return m.Target==null||m.Target.Hero?null:(m.Target.Seat==v.Seat?v.Self:v.Enemy).Board.FirstOrDefault(u=>u.Id==m.Target.UnitId);}
 static double Value(AgentUnit u){return u.Attack*1.8+u.Health*.6+(u.Shield?3:0)+(u.Taunt?3:0)+(u.Key=="NEW1_021"&&!u.Silenced?45:0)+(!u.Silenced&&(u.Key=="NEW1_019"||u.Key=="EX1_004"||u.Key=="EX1_162"||u.Key=="EX1_565")?12:0);}
 static double Threat(AgentObservation v){return v.Enemy.Board.Where(u=>!u.Frozen).Sum(u=>u.Attack*(u.Windfury?2:1))+(!v.Enemy.Frozen?v.Enemy.HeroAttack:0);}
 static bool Auction(AgentObservation v){return v.Self.Board.Any(u=>u.Key=="EX1_095"&&!u.Silenced);}
 static double Remove(AgentObservation v,AgentUnit u,int hit){
  if(u==null||u.Immune)return -100;bool danger=Threat(v)>=v.Self.Health+v.Self.Armor-3;
  return u.Shield?5:hit>=u.Health?Value(u)+15+(danger?15:0)-Math.Max(0,hit-u.Health):danger?u.Attack+3:-8;
 }
 double Score(AgentObservation v,AgentMove m){
  var t=Target(v,m);bool own=m.Target!=null&&m.Target.Seat==v.Seat;bool auction=Auction(v);double threat=Threat(v);bool danger=threat>=v.Self.Health+v.Self.Armor-3&&threat>0;
  double cycle=auction&&v.Self.DeckCount>0&&v.Self.HandCount<=8?26:auction&&v.Self.HandCount>=9?-12:0;
  if(m.Kind=="play"&&v.Hand.Any(h=>h.Id==m.Source&&h.Type=="SPELL")){int draw=v.Self.Board.Count(u=>u.Key=="EX1_095"&&!u.Silenced),missing=Math.Max(0,draw-v.Self.DeckCount);if(missing>0&&v.Self.Health+v.Self.Armor<=missing*(2*v.Self.Fatigue+missing+1)/2)return -200;}
  if(m.Kind=="end")return 0;
  if(m.Kind=="attack"){
   if(Face(v,m))return v.Enemy.Immune?-100:18+m.Damage*1.8;
   var unit=v.Self.Board.FirstOrDefault(u=>u.Id==m.Source);bool kill=t!=null&&!t.Shield&&m.Damage>=t.Health;bool dies=unit==null?t!=null&&t.Attack+(kill&&t.Key=="EX1_029"?2:kill&&t.Key=="EX1_097"?2:0)>=v.Self.Health+v.Self.Armor:!unit.Shield&&t!=null&&(t.Attack>=unit.Health||t.Poison);
   bool fast=v.Enemy.ClassId=="WARLOCK"||v.Enemy.ClassId=="HUNTER"||v.Enemy.ClassId=="PALADIN";
   if(t==null)return -100;return (kill?Value(t)+14+(fast?15:0):t.Shield?7+(fast?12:0):-5)+(danger?t.Attack*3:0)-(dies?(unit==null?200:Value(unit)+(unit.Key=="EX1_095"?25:0)):unit==null?t.Attack*.7:0);
  }
  if(m.Kind=="power")return v.Self.WeaponDurability==0?(v.Hand.Any(h=>h.Key=="CS2_074")?40:16): -25;
  if(m.Key=="GAME_COIN"){
   if(v.Self.Mana>=10)return -70;
   if(v.Hand.Any(h=>h.Key=="EX1_095"&&h.Cost==v.Self.Mana+1)&&v.Hand.Any(h=>h.Type=="SPELL"&&h.Key!="GAME_COIN"&&(h.Cost==0||h.Key=="EX1_145")))return 70;
   if(v.Hand.Any(h=>h.Key=="EX1_613"&&h.Cost<=v.Self.Mana+1)&&v.Self.MaxMana<=4)return 42;
   if(v.Hand.Any(h=>h.Key=="EX1_134"&&h.Cost==v.Self.Mana+1)&&v.Enemy.Board.Length>0)return 55;
   return auction&&v.Self.Mana<10?50: -20;
  }
  if(m.Key=="EX1_145"){
   if(v.PreparationDiscount>0)return -50;
   var next=v.Hand.Where(h=>h.Id!=m.Source&&h.Type=="SPELL"&&h.UndiscountedCost>0&&Math.Max(0,h.UndiscountedCost-3)<=v.Self.Mana);
   bool useful=next.Any(h=>h.Key=="CS2_077"&&v.Self.HandCount<=5&&v.Self.DeckCount>0||h.Key=="EX1_129"&&v.Enemy.Board.Count(u=>u.Health<=1+v.SpellPower&&!u.Shield)>=2||h.Key=="CS2_233"&&v.Self.WeaponAttack+v.SpellPower>=2&&v.Enemy.Board.Length>=2||h.Key=="EX1_124"&&v.Enemy.Board.Any(u=>u.Health<=4+v.SpellPower&&!u.Shield)||h.Key=="EX1_581"&&v.Enemy.Board.Any(u=>u.Attack>=4)||h.Key=="EX1_128"&&auction);
   return useful?48+cycle:auction&&next.Any()?28+cycle:-25;
  }
  if(m.Key=="EX1_144"){
   if(!own||t==null||v.Self.HandCount-1+Math.Min(v.Self.DeckCount,v.Self.Board.Count(u=>u.Key=="EX1_095"&&!u.Silenced))>=10)return -100;
   if(t.Key=="EX1_116")return t.AttacksRemaining==0?100:-55;
   if(t.AttacksRemaining>0&&!v.Enemy.Immune)return -5;
   if(v.Enemy.Board.Any(u=>u.Key=="NEW1_021"&&!u.Silenced)&&t.Cost>=3)return t.Key=="EX1_095"?90:65;
   if(t.Key=="CS2_117"&&v.Self.Health<=15&&v.Self.Mana>=1)return 40+cycle;
   if(t.Key=="EX1_134"&&v.Self.Mana>=1&&v.Enemy.Board.Any(u=>!u.Shield&&u.Health<=2))return 45+cycle;
   if(t.Key=="EX1_095"&&t.Health<=2&&!v.Hand.Any(h=>h.Key=="EX1_128")&&v.Self.Mana>=3)return 28;
   return -45;
  }
  if(m.Key=="EX1_128"){
   var protect=v.Self.Board.Where(u=>!u.Stealth&&!u.Silenced&&(u.Key=="EX1_095"||u.Key=="EX1_613"&&u.Attack>=6)).ToArray();
   if(v.Enemy.Board.Any(u=>u.Key=="NEW1_021"&&!u.Silenced))return -40;
   if(protect.Any(u=>u.AttacksRemaining>0))return -5;
   return protect.Any(u=>u.Key=="EX1_095")?60+cycle:protect.Length>0?38:-40;
  }
  if(m.Key=="CS2_077"){if(v.Self.DeckCount==0||v.Self.HandCount>=7)return -80;return 42+Math.Max(0,5-v.Self.HandCount)*5-(danger?30:0)-(auction&&v.Self.HandCount>=5?25:0);}
  if(m.Key=="CS2_074")return v.Self.WeaponDurability>0?22+v.Self.WeaponDurability*3+cycle+(v.Enemy.Board.Any(u=>u.Health<=v.Self.WeaponAttack+2)?12:0): -100;
  if(m.Key=="CS2_233"){int hit=v.Self.WeaponAttack+v.SpellPower;return v.Enemy.Board.Sum(u=>u.Shield?2:hit>=u.Health?Value(u)+10:1)-10+cycle+(danger?15:0);}
  if(m.Key=="EX1_129")return v.Enemy.Board.Sum(u=>u.Shield?3:u.Health<=1+v.SpellPower?Value(u)+12:1)-12+cycle+(v.Self.HandCount<7&&v.Self.DeckCount>0?8:0);
  if(m.Key=="EX1_581"){if(own||t==null)return -100;if(t.Key=="NEW1_021"&&!t.Silenced)return v.Self.Board.Length>0||v.Hand.Any(h=>h.Type=="MINION"&&h.Cost<=v.Self.Mana-m.Cost)?160:-35;if(t.Attack==0)return -35+cycle;if(t.Cost<=2&&!danger)return -10+cycle;return (t.Taunt?20:8)+Value(t)*.7+(danger?t.Attack*4:0)+cycle;}
  if(m.Key=="CS2_072"||m.Key=="EX1_124"||m.Key=="EX1_278"){
   int damage=(m.Key=="CS2_072"?2:m.Key=="EX1_278"?1:v.CardsPlayed>0?4:2)+v.SpellPower;
   if(own)return -100;
   if(Face(v,m))return !v.Enemy.Immune&&(v.Enemy.Health+v.Enemy.Armor<=8||auction&&v.Self.DeckCount>0)?12+cycle: -30;
   double follow=t!=null&&!t.Shield&&t.Health>damage&&t.Health<=damage+2&&v.Hand.Any(h=>h.Key=="EX1_134"&&h.Cost<=v.Self.Mana-m.Cost)?Value(t)+45:0;
   return Remove(v,t,damage)+cycle+follow-(m.Key=="EX1_124"&&v.CardsPlayed==0?12:0);
  }
  if(m.Key=="CS2_073")return own&&t!=null&&t.AttacksRemaining>0&&v.CardsPlayed>0?25+cycle+(danger&&v.Enemy.Board.Any(u=>u.Health<=t.Attack+4)?10:0):-40;
  if(m.Key=="CS2_117")return own&&m.Target.Hero?30+Math.Min(3,v.Self.HeroMaxHealth-v.Self.Health)*6+(v.Enemy.ClassId=="MAGE"&&v.Enemy.MaxMana>=8&&v.Self.Health<=15?40:0):own&&t!=null&&t.Health<t.MaxHealth?30+Math.Min(3,t.MaxHealth-t.Health)*3: -25;
  var card=v.Hand.FirstOrDefault(h=>h.Id==m.Source);if(card==null||card.Type!="MINION")return -20;
  if(v.Enemy.Board.Any(u=>u.Key=="NEW1_021"&&!u.Silenced)&&m.Key!="EX1_116"&&m.Key!="CS2_117")return -40;
  if(m.Key=="EX1_095"){if(auction&&(v.Self.HandCount>=4||v.Self.DeckCount<12))return -30;int remaining=v.Self.Mana-m.Cost;int cheap=v.Hand.Count(h=>h.Type=="SPELL"&&h.Id!=m.Source&&h.Cost<=remaining);bool prep=v.Hand.Any(h=>h.Key=="EX1_145");bool conceal=v.Hand.Any(h=>h.Key=="EX1_128"&&(h.Cost<=remaining||prep));return 35+cheap*7+(conceal?15:0)-(danger?30:0)-(v.Self.HandCount>=8?12:0);}
  if(m.Key=="EX1_613")return v.CardsPlayed>=2?52+v.CardsPlayed*4:v.CardsPlayed==1?32:v.Self.MaxMana<=3&&v.Enemy.ClassId=="MAGE"?18:-15;
  if(m.Key=="EX1_134"&&v.CardsPlayed==0&&v.Moves.Any(other=>other.Kind=="play"&&other.Source!=m.Source&&other.Cost==0&&(other.Key=="CS2_072"||other.Key=="EX1_145"||other.Key=="GAME_COIN")))return -10;
  if(m.Key=="EX1_134"){if(own)return -100;if(v.CardsPlayed==0)return 22;if(t!=null)return 35+Remove(v,t,2);return Face(v,m)&&!v.Enemy.Immune?25:-80;}
  if(m.Key=="EX1_116")return -40;
  return 25+card.Cost+(m.Key=="EX1_284"&&v.Self.HandCount<=8?12:0);
 }
}
}
