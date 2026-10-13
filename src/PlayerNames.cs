using System;
using System.Linq;
namespace Touchfish {
public static class PlayerNames {
 public const int MaxLength=12;
 public static string Normalize(string value,int seat){value=value??"";if(value.Any(Char.IsControl))throw new InvalidOperationException("名称不能包含换行或控制字符。");value=value.Trim();if(value.Length==0)return "玩家 "+(seat+1);if(value.Length>MaxLength||value.Any(Char.IsControl))throw new InvalidOperationException("名称最多12个字符，不能包含换行或控制字符。");return value;}
}
public sealed partial class MatchEngine {
 public string PlayerLabel(int seat){string name=PlayerNames.Normalize(Players[seat].DisplayName,seat);string other=PlayerNames.Normalize(Players[1-seat].DisplayName,1-seat);return name==other?name+"（"+(seat+1)+"）":name;}
}
}
