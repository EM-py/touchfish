using System;
namespace Touchfish {
public sealed partial class MatchEngine {
 public void SummonAround(int seat,BattleUnit source,string tokenId){if(source==null||source.Owner!=seat||!Players[seat].Board.Contains(source))return;var board=Players[seat].Board;int left=0,right=0;bool toLeft=true;while(board.Count<7){int anchor=board.IndexOf(source);int position=toLeft?anchor-left:anchor+1+right;if(Summon(seat,tokenId,position)==null)break;if(toLeft)left++;else right++;toLeft=!toLeft;}}
}
}
