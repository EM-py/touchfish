using System;
using System.Linq;
using System.Collections.Generic;
namespace Touchfish {
// A small public-information tactical model, not a clone of the real game. Unknown
// draws/secrets are not resolved here. Every chosen first action is revalidated by
// the real engine and the plan is rebuilt from the next observation.
public sealed class FreezeMageDefense {
 public const int NodeLimit=1800,Width=28,Depth=6;
 public int Nodes{get;private set;}
 sealed class Unit {
  public int Id,Attack,Health,SpellPower;public string Key;
  public bool Shield,Frozen,Taunt,Silenced,Windfury,Immune;
  public Unit Copy(){return (Unit)MemberwiseClone();}
 }
 sealed class State {
  public int Mana,Health,Armor,Power;public bool Block,Barrier,Immune,EnemyHeroFrozen;
  public double Spent;public HashSet<int> Used=new HashSet<int>();public bool PowerUsed;
  public List<Unit> Own=new List<Unit>(),Enemy=new List<Unit>();public AgentMove First;
  public State Copy(){return new State{Mana=Mana,Health=Health,Armor=Armor,Power=Power,Block=Block,Barrier=Barrier,Immune=Immune,EnemyHeroFrozen=EnemyHeroFrozen,Spent=Spent,PowerUsed=PowerUsed,Used=new HashSet<int>(Used),Own=Own.Select(u=>u.Copy()).ToList(),Enemy=Enemy.Select(u=>u.Copy()).ToList(),First=First};}
 }
 static Unit CopyUnit(AgentUnit u){return new Unit{Id=u.Id,Attack=u.Attack,Health=u.Health,SpellPower=u.SpellDamage,Key=u.Key,Shield=u.Shield,Frozen=u.Frozen,Taunt=u.Taunt,Silenced=u.Silenced,Windfury=u.Windfury,Immune=u.Immune};}
 public AgentMove Choose(AgentObservation v){
  Nodes=0;var root=new State{Mana=v.Self.Mana,Health=v.Self.Health,Armor=v.Self.Armor,Power=v.SpellPower,Block=v.OwnSecrets.Contains("EX1_295"),Barrier=v.OwnSecrets.Contains("EX1_289"),Immune=v.Self.Immune,EnemyHeroFrozen=v.Enemy.Frozen,PowerUsed=v.Self.PowerUsed,Own=v.Self.Board.Select(CopyUnit).ToList(),Enemy=v.Enemy.Board.Select(CopyUnit).ToList()};
  double threat=Incoming(root,v);bool urgent=threat>0&&threat>=root.Health+root.Armor-3;
  if(!urgent&&threat<Math.Max(4,(root.Health+root.Armor)*0.4)&&!root.Own.Any(u=>u.Key=="NEW1_021"&&!u.Silenced))return null;
  double baseline=Evaluate(root,v),bestScore=baseline;State best=root;var beam=new List<State>{root};
  for(int depth=0;depth<Depth&&Nodes<NodeLimit;depth++){
   var next=new List<State>();foreach(var state in beam)foreach(var move in v.Moves){if(Nodes>=NodeLimit)break;State child=Apply(state,move,v);if(child==null)continue;Nodes++;double score=Evaluate(child,v);if(score>bestScore+0.001){bestScore=score;best=child;}next.Add(child);}
   beam=next.OrderByDescending(s=>Evaluate(s,v)).Take(Width).ToList();if(beam.Count==0)break;
  }
  return best.First!=null&&bestScore>baseline+2?best.First:null;
 }
 static double Resource(string key){switch(key){case "CS2_029":return 17;case "EX1_279":return 28;case "CS2_024":return 12;case "CS2_031":return 9;case "CS2_026":return 7;case "CS2_028":return 8;case "CS2_032":return 9;case "NEW1_021":return 8;case "EX1_295":return 6;case "EX1_561":return 14;default:return 3;}}
 static State Apply(State before,AgentMove m,AgentObservation v){
  if(m.Kind=="end"||m.Cost>before.Mana||m.Kind=="play"&&before.Used.Contains(m.Source)||m.Kind=="power"&&before.PowerUsed||m.Kind=="attack")return null;
  // Attacks and unknown cards remain with the normal policy; do not invent their
  // deathrattles or changed target eligibility inside this reduced model.
  if(m.Kind=="play"&&!new[]{"GAME_COIN","CS2_024","CS2_031","CS2_029","EX1_279","CS2_026","CS2_028","CS2_032","CS2_027","NEW1_021","EX1_295","EX1_289","EX1_561","EX1_012"}.Contains(m.Key))return null;
  bool ownTarget=m.Target!=null&&m.Target.Seat==v.Seat;
  var s=before.Copy();s.Mana-=m.Cost;s.Spent+=m.Cost*1.1+(m.Kind=="power"?0:Resource(m.Key));s.First=s.First??m;
  if(m.Kind=="power")s.PowerUsed=true;else s.Used.Add(m.Source);
  var target=m.Target==null||m.Target.Hero?null:s.Enemy.FirstOrDefault(u=>u.Id==m.Target.UnitId);
  if(m.Kind=="power"){
   if(ownTarget||target==null)return null;Hit(s,target,m.Damage);return s;
  }
  switch(m.Key){
   case "GAME_COIN":if(before.Mana>=10)return null;s.Mana=Math.Min(10,before.Mana+1);break;
   case "EX1_295":if(s.Block)return null;s.Block=true;break;
   case "EX1_289":if(s.Barrier)return null;s.Barrier=true;break;
   case "EX1_561":if(!ownTarget||m.Target==null||!m.Target.Hero||s.Own.Count>=7)return null;s.Health=Math.Min(15,v.Self.HeroMaxHealth);s.Own.Add(new Unit{Id=-m.Source,Attack=8,Health=8,Key=m.Key});break;
   case "CS2_027":if(s.Own.Count>=7)return null;for(int n=0;n<2&&s.Own.Count<7;n++)s.Own.Add(new Unit{Id=-m.Source*2-n,Health=2,Taunt=true,Key="CS2_mirror"});break;
   case "NEW1_021":if(s.Own.Count>=7||s.Own.Any(u=>u.Key=="NEW1_021"&&!u.Silenced))return null;s.Own.Add(new Unit{Id=-m.Source,Health=7,Key=m.Key});break;
   case "EX1_012":if(s.Own.Count>=7)return null;s.Own.Add(new Unit{Id=-m.Source,Attack=1,Health=1,SpellPower=1,Key=m.Key});s.Power++;break;
   case "CS2_026":foreach(var u in s.Enemy)u.Frozen=true;break;
   case "CS2_028":case "CS2_032":int amount=(m.Key=="CS2_028"?2:4)+s.Power;var targets=s.Enemy.ToArray();foreach(var u in targets){if(m.Key=="CS2_028")u.Frozen=true;Hit(s,u,amount);}break;
   case "CS2_024":case "CS2_031":case "CS2_029":case "EX1_279":
    if(ownTarget||m.Target==null)return null;
    if(m.Target.Hero){if(m.Key!="CS2_024"&&m.Key!="CS2_031")return null;s.EnemyHeroFrozen=true;break;}
    if(target==null)return null;
    int hit=m.Key=="CS2_024"?3+s.Power:m.Key=="CS2_031"?(target.Frozen?4+s.Power:0):m.Key=="CS2_029"?6+s.Power:10+s.Power;
    Hit(s,target,hit);if(m.Key=="CS2_024"||m.Key=="CS2_031")target.Frozen=true;break;
  }return s;
 }
 static void Hit(State s,Unit target,int amount){
  if(amount<=0||target.Immune||!s.Enemy.Contains(target))return;if(target.Shield){target.Shield=false;return;}target.Health-=amount;if(target.Health>0)return;s.Enemy.Remove(target);
  if(!target.Silenced&&target.Key=="EX1_029"){int armor=Math.Min(s.Armor,2);s.Armor-=armor;s.Health-=2-armor;}
  // Known public deathrattle bodies. Unmodelled effects get no extra safety credit.
  if(!target.Silenced&&target.Key=="EX1_556")s.Enemy.Add(new Unit{Id=-target.Id,Attack=2,Health=1,Key="skele21"});
  if(!target.Silenced&&target.Key=="EX1_110")s.Enemy.Add(new Unit{Id=-target.Id,Attack=4,Health=5,Key="EX1_110t"});
  if(!target.Silenced&&target.Key=="EX1_534")for(int n=0;n<2&&s.Enemy.Count<7;n++)s.Enemy.Add(new Unit{Id=-target.Id*2-n,Attack=2,Health=2,Key="GAME_HYENA"});
 }
 static double Incoming(State s,AgentObservation v){
  var attacks=new List<int>();foreach(var u in s.Enemy.Where(u=>!u.Frozen&&u.Attack>0&&u.Key!="NEW1_021"&&u.Key!="EX1_298")){attacks.Add(u.Attack);if(u.Windfury)attacks.Add(u.Attack);}if(!s.EnemyHeroFrozen&&v.Enemy.HeroAttack>0)attacks.Add(v.Enemy.HeroAttack);
  foreach(var taunt in s.Own.Where(u=>u.Taunt)){if(attacks.Count==0)break;if(taunt.Shield){int lowest=attacks.Min();attacks.Remove(lowest);}
   // Minimum total attack spent on this blocker, so remaining face damage is
   // adversarial rather than assuming the opponent wastes its biggest attacker.
   var sums=new Dictionary<int,int>{{0,0}};for(int n=0;n<attacks.Count;n++)foreach(var pair in sums.ToArray()){int sum=pair.Key+attacks[n],subsetMask=pair.Value|(1<<n);if(!sums.ContainsKey(sum)||Bits(subsetMask)<Bits(sums[sum]))sums[sum]=subsetMask;}
   var enough=sums.Where(p=>p.Key>=taunt.Health).OrderBy(p=>p.Key).ToArray();if(enough.Length==0){attacks.Clear();break;}int mask=enough[0].Value;for(int n=attacks.Count-1;n>=0;n--)if((mask&(1<<n))!=0)attacks.RemoveAt(n);
  }
  return attacks.Sum()+s.Enemy.Count(u=>u.Key=="EX1_298"&&!u.Silenced)*8;
 }
 static int Bits(int mask){int n=0;while(mask!=0){mask&=mask-1;n++;}return n;}
 static double Evaluate(State s,AgentObservation v){
  if(s.Health<=0)return -100000-s.Spent;double incoming=Incoming(s,v),passive=s.Enemy.Count(u=>u.Key=="EX1_298"&&!u.Silenced)*8,hp=s.Health+s.Armor+(s.Barrier&&incoming>passive?8:0);bool dead=incoming>=hp&&!s.Block&&!s.Immune;
  if(dead)return -50000-(incoming-hp)*100-s.Spent;
  bool pops=incoming>=hp&&s.Block;
  double urgency=pops?9:incoming>=hp-3&&incoming>0?40:hp<=12?9:3;
  double loss=pops?Math.Max(0,hp-1):incoming;
  double score=-loss*urgency-s.Spent-s.Enemy.Sum(u=>u.Attack*0.7+(u.Shield?1:0));
  if(pops)score-=30;
  score+=Math.Min(30,s.Health+s.Armor)*0.8;
  // A risk allowance from public class/hand count, not knowledge of held cards.
  // Ice Barrier is only physical protection: a Soulfire can finish through it.
  int burst=v.Enemy.HandCount==0?0:v.Enemy.ClassId=="WARLOCK"?4:v.Enemy.ClassId=="MAGE"?3:2;
  if(v.Enemy.HandCount>=2)burst+=2;
  if(v.Enemy.HandCount>0)burst+=s.Enemy.Sum(u=>u.SpellPower);
  if(!s.Block&&!s.Immune&&(s.Health+s.Armor<=burst||hp-incoming<=burst))score-=220;
  var doom=s.Own.FirstOrDefault(u=>u.Key=="NEW1_021"&&!u.Silenced);
  int disruption=v.Enemy.HandCount>0?3:0;
  if(doom!=null&&incoming+disruption<doom.Health){double deathDamage=s.Enemy.Count(u=>u.Key=="EX1_029"&&!u.Silenced)*2;if(hp-incoming>deathDamage)score+=s.Enemy.Sum(u=>u.Attack*2+u.Health*0.6)+8;else score-=30000;}
  if(v.Self.DeckCount==0){double after=hp-incoming-(v.Self.Fatigue+1);if(after<=0)score-=30000;else score-=Math.Max(0,8-after)*10;}
  return score;
 }
}
}
