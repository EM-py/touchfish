using System;
using System.Collections.Generic;

namespace Touchfish {
// Public presentation events only: no hidden hand/deck identities.
public sealed class MatchVisualEvent {
 public int Sequence,Action,Seat,UnitId,Slot=-1,Amount,CardCost,PaidCost;public string Kind,CardName,CardId,CardText;
}
public sealed class MatchVisualCursor {
 string session;int sequence;
 public MatchVisualEvent[] Take(string nextSession,IEnumerable<MatchVisualEvent> events,bool display){
  if(session!=nextSession){session=nextSession;sequence=0;}
  var result=new List<MatchVisualEvent>();foreach(var item in events){if(item.Sequence<=sequence)continue;sequence=item.Sequence;if(display)result.Add(item);}return result.ToArray();
 }
}
public partial class MatchEngine {
 public string VisualSession=Guid.NewGuid().ToString("N");public readonly List<MatchVisualEvent> VisualEvents=new List<MatchVisualEvent>();int visualSequence,visualAction;
 public void EmitSpell(CardRecord card,int seat,int paidCost,string choice){
  EmitVisual("spell",seat,name:card.Name);var item=VisualEvents[VisualEvents.Count-1];item.CardId=card.Id;item.CardCost=card.Cost;item.PaidCost=paidCost;item.CardText=card.Text;
  if(Rules.IsChoice(card)&&choice!=null)item.CardText+="\n本次抉择："+System.Linq.Enumerable.First(Rules.Choices(card),option=>option.Key==choice).Title;
 }
 public void EmitVisual(string kind,int seat,int unitId=0,int slot=-1,int amount=0,string name=null){VisualEvents.Add(new MatchVisualEvent{Sequence=++visualSequence,Action=visualAction,Kind=kind,Seat=seat,UnitId=unitId,Slot=slot,Amount=amount,CardName=name});if(VisualEvents.Count>128)VisualEvents.RemoveAt(0);}
}
}
