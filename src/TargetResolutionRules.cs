using System;
using System.Collections.Generic;
using System.Linq;
namespace Touchfish {
public sealed partial class MatchEngine {
 // Target validity is checked again at effect resolution; command legality is checked before payment.
 public bool IsPresentTarget(MatchTarget target){
  if(target==null||target.Seat<0||target.Seat>=Players.Length)return false;
  return target.Hero?!Players[target.Seat].HeroDestroyed&&Players[target.Seat].Health>0:LivingUnit(target)!=null;
 }
 public MatchTarget[] SelectRandomTargets(IEnumerable<MatchTarget> candidates,int count){
  if(count<0)throw new ArgumentOutOfRangeException("count");
  var pool=(candidates??Enumerable.Empty<MatchTarget>()).Where(IsPresentTarget).GroupBy(t=>new{t.Seat,t.UnitId}).Select(group=>group.First()).ToList();
  var chosen=new List<MatchTarget>();while(chosen.Count<count&&pool.Count>0){int index=random.Next(pool.Count);chosen.Add(pool[index]);pool.RemoveAt(index);}return chosen.ToArray();
 }
 // Take the entire batch snapshot before its first hit. Deathrattle replacements are not new targets.
 public void DamageRandomMinions(int seat,int count,int amount){
  var targets=SelectRandomTargets(Characters(seat,false),count);
  DamageBatch(()=>{foreach(var target in targets)Damage(target,amount);});
 }
}
}
