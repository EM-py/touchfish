using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Touchfish {
// UI-only slots. The authoritative board order and rule timings never wait for animation.
public interface IMatchPresentationLayout {
 event Action Changed;
 void Prepare(string session,Dictionary<int,int[]> displayed,IEnumerable<MatchVisualEvent> events,bool animate);
 int[] Slots(int seat,int[] liveIds);
 bool Reserved(int seat,int slot);
 void Complete(string session,int sequence);
 void Clear();
}
public sealed class MatchPresentationLayout : IMatchPresentationLayout {
 sealed class Hold {public int Seat,Slot;public DateTime Until;}
 public event Action Changed;
 string session;int sequence;
 readonly Dictionary<int,Hold> holds=new Dictionary<int,Hold>();
 readonly Dictionary<int,int>[] positions={new Dictionary<int,int>(),new Dictionary<int,int>()};
 readonly DispatcherTimer timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(80)};
 public MatchPresentationLayout(){timer.Tick+=(s,e)=>{var expired=holds.Where(p=>p.Value.Until<=DateTime.UtcNow).Select(p=>p.Key).ToArray();if(expired.Length==0)return;foreach(int id in expired)holds.Remove(id);Notify();};}
 public void Prepare(string nextSession,Dictionary<int,int[]> displayed,IEnumerable<MatchVisualEvent> events,bool animate){
  if(session!=nextSession){Clear();session=nextSession;sequence=0;}
  if(!animate)Clear();
  foreach(var item in events){if(item.Sequence<=sequence)continue;sequence=item.Sequence;if(!animate||item.Kind!="death")continue;int[] slots;if(!displayed.TryGetValue(item.Seat,out slots))continue;int slot=Array.IndexOf(slots,item.UnitId);if(slot<0)continue;
   if(!holds.Values.Any(h=>h.Seat==item.Seat)){positions[item.Seat].Clear();for(int i=0;i<slots.Length;i++)if(slots[i]>0)positions[item.Seat][slots[i]]=i;}
   positions[item.Seat].Remove(item.UnitId);holds[item.Sequence]=new Hold{Seat=item.Seat,Slot=slot,Until=DateTime.UtcNow.AddMilliseconds(OfficeMatchVisualEffects.DeathDurationMs+600)};
  }
  if(holds.Count>0)timer.Start();
 }
 public int[] Slots(int seat,int[] liveIds){var result=new int[7];if(!holds.Values.Any(h=>h.Seat==seat)){positions[seat].Clear();for(int i=0;i<Math.Min(7,liveIds.Length);i++)result[i]=liveIds[i];return result;}
  var map=positions[seat];foreach(int id in map.Keys.Where(id=>!liveIds.Contains(id)).ToArray())map.Remove(id);
  foreach(int id in liveIds){int slot;if(map.TryGetValue(id,out slot)&&!Reserved(seat,slot)&&result[slot]==0)result[slot]=id;}
  foreach(int id in liveIds.Where(id=>!result.Contains(id))){int slot=-1;for(int i=0;i<7;i++)if(result[i]==0&&!Reserved(seat,i)){slot=i;break;}if(slot<0)continue;result[slot]=id;map[id]=slot;}
  return result;
 }
 public bool Reserved(int seat,int slot){return holds.Values.Any(h=>h.Seat==seat&&h.Slot==slot);}
 public void Complete(string ownerSession,int id){if(session!=ownerSession||!holds.Remove(id))return;Notify();}
 void Notify(){if(holds.Count==0)timer.Stop();var changed=Changed;if(changed!=null)changed();}
 public void Clear(){timer.Stop();holds.Clear();foreach(var map in positions)map.Clear();}
}
public partial class PanelWindow {
 IMatchPresentationLayout presentationLayout;
 void BuildPresentationLayout(){presentationLayout=new MatchPresentationLayout();presentationLayout.Changed+=()=>{if(match!=null&&!minimal&&currentPage=="match"){RenderMatch();}};Closed+=(s,e)=>presentationLayout.Clear();}
 void PrepareMatchLayout(){if(presentationLayout==null)return;var displayed=new Dictionary<int,int[]>{{0,new int[7]},{1,new int[7]}};foreach(var pair in visualTargets){if(visualAnchorSession!=match.VisualSession)continue;var bits=pair.Key.Split(':');int seat=int.Parse(bits[0]),id=int.Parse(bits[1]);if(id==0)continue;int slot=System.Windows.Controls.Grid.GetColumn(pair.Value);if(slot>=0&&slot<7)displayed[seat][slot]=id;}presentationLayout.Prepare(match.VisualSession,displayed,match.VisualEvents,fullEffects&&!minimal&&currentPage=="match"&&!awaitHandoff);}
 int LogicalSlotInsertion(int seat,int displaySlot,int[] slots){var board=match.Players[seat].Board;for(int i=displaySlot;i<slots.Length;i++)if(slots[i]>0){int index=board.FindIndex(u=>u.Id==slots[i]);if(index>=0)return index;}return board.Count;}
}
}
