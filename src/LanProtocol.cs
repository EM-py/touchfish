using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Touchfish {
public sealed class LanCommand {public string Kind,Session,Choice,Emote;public int Revision,RequestId,Source,Position=-1;public int[] SelectedHandIds;public MatchTarget Target;}
public sealed class LanPlayerView {public MatchPlayer Public;public BattleUnit[] Board;public HandCard[] Hand;public int HandCount,DeckCount;}
public sealed class LanState {public string Session;public int Revision,Active,Turn,Winner,CardsPlayed;public LanPlayerView[] Players;public string[] Log;public string VisualSession;public MatchVisualEvent[] VisualEvents;}
public sealed class LanMessage {public string Kind,Fingerprint,Code,PoolId,Message;public int Protocol=1,ReplyId;public bool Success;public LanCommand Command;public LanState State;}
public static class LanProtocol {
 public const int Port=42671,MaxFrame=131072;
 public static bool PrivateAddress(IPAddress address){if(address.AddressFamily!=AddressFamily.InterNetwork)return false;var b=address.GetAddressBytes();return b[0]==127||b[0]==10||b[0]==192&&b[1]==168||b[0]==172&&b[1]>=16&&b[1]<=31||b[0]==169&&b[1]==254;}
 public static IPEndPoint Endpoint(string value){var parts=value.Trim().Split(':');IPAddress address;int port=Port;if(parts.Length>2||!IPAddress.TryParse(parts[0],out address)||!PrivateAddress(address)||parts.Length==2&&(!Int32.TryParse(parts[1],out port)||port<1024||port>65535))throw new InvalidOperationException("请输入局域网 IPv4 地址，例如 192.168.1.8:42671。");return new IPEndPoint(address,port);}
 public static string Addresses(int port){return String.Join(" / ",Dns.GetHostAddresses(Dns.GetHostName()).Where(a=>PrivateAddress(a)&&!IPAddress.IsLoopback(a)).Select(a=>a+":"+port));}
 public static string Fingerprint(CardCatalog catalog){using(var sha=SHA256.Create()){string data=JsonData.Serializer().Serialize(catalog.Cards.Values.OrderBy(c=>c.Id,StringComparer.Ordinal).ToArray())+JsonData.Serializer().Serialize(catalog.Pools);byte[] app=File.ReadAllBytes(typeof(LanProtocol).Assembly.Location);return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(data)))+":"+Convert.ToBase64String(sha.ComputeHash(app));}}
 public static void WriteFrame(Stream stream,string text){byte[] data=Encoding.UTF8.GetBytes(text);if(data.Length==0||data.Length>MaxFrame)throw new InvalidDataException("联机消息大小无效。");byte[] header=BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));stream.Write(header,0,4);stream.Write(data,0,data.Length);}
 static void ReadExact(Stream stream,byte[] data){int offset=0;while(offset<data.Length){int n=stream.Read(data,offset,data.Length-offset);if(n==0)throw new EndOfStreamException("对方已离开。");offset+=n;}}
 public static string ReadFrame(Stream stream){var header=new byte[4];ReadExact(stream,header);int size=IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header,0));if(size<=0||size>MaxFrame)throw new InvalidDataException("联机消息大小无效。");var data=new byte[size];ReadExact(stream,data);return new UTF8Encoding(false,true).GetString(data);}
 internal static T Copy<T>(T value){return JsonData.Serializer().Deserialize<T>(JsonData.Serializer().Serialize(value));}
 internal static void CopyPublic(MatchPlayer source,MatchPlayer target){foreach(var field in typeof(MatchPlayer).GetFields())if(!field.IsInitOnly)field.SetValue(target,field.GetValue(source));}
 public static LanState View(MatchEngine game,int seat,string session,int revision){var view=new LanState{Session=session,Revision=revision,Active=game.Active,Turn=game.Turn,Winner=game.Winner,CardsPlayed=game.CardsPlayed,Players=new LanPlayerView[2],Log=game.Log.ToArray(),VisualSession=game.VisualSession,VisualEvents=Copy(game.VisualEvents.ToArray())};for(int n=0;n<2;n++){var p=game.Players[n];var scalar=new MatchPlayer();CopyPublic(p,scalar);view.Players[n]=new LanPlayerView{Public=scalar,Board=Copy(p.Board.ToArray()),Hand=n==seat?Copy(p.Hand.ToArray()):new HandCard[0],HandCount=p.Hand.Count,DeckCount=p.Deck.Count};}return view;}
 public static MatchEngine ReadView(CardCatalog catalog,LanState view){if(view==null||view.Players==null||view.Players.Length!=2||view.Active<0||view.Active>1)throw new InvalidDataException("对局快照无效。");var game=new MatchEngine(catalog);game.Active=view.Active;game.Turn=view.Turn;game.Winner=view.Winner;game.CardsPlayed=view.CardsPlayed;if(view.Log==null)throw new InvalidDataException("对局记录缺失。");game.Log.AddRange(view.Log);if(view.VisualSession==null||view.VisualEvents==null||view.VisualEvents.Length>128)throw new InvalidDataException("视觉事件无效。");game.VisualSession=view.VisualSession;game.VisualEvents.AddRange(view.VisualEvents);for(int n=0;n<2;n++){var p=view.Players[n];if(p==null||p.Board==null||p.Board.Length>7||p.Hand==null||p.HandCount>10||p.HandCount<0||p.DeckCount<0||p.DeckCount>60||p.Public==null)throw new InvalidDataException("对局快照无效。");CopyPublic(p.Public,game.Players[n]);game.Players[n].ReportedHandCount=p.HandCount;game.Players[n].Board.AddRange(p.Board);game.Players[n].Hand.AddRange(p.Hand);for(int i=0;i<p.DeckCount;i++)game.Players[n].Deck.Add("HIDDEN");}return game;}
}
public sealed class LanHostGame {
 public readonly MatchEngine Game;public readonly string Session=Guid.NewGuid().ToString("N");public int Revision;
 public LanHostGame(CardCatalog catalog,DeckDocument host,DeckDocument guest,int seed){Game=new MatchEngine(catalog,host,guest,seed);Game.Log[0]="局域网对局开始";}
 public ActionResult Execute(int seat,LanCommand command){if(command==null||command.Session!=Session||command.Revision!=Revision)return Fail("状态已更新，请重新操作。");if(command.Target!=null&&(command.Target.Seat<0||command.Target.Seat>1||command.Target.UnitId<0)||command.Source<0||command.Position< -1||command.Position>7)return Fail("操作参数无效。");ActionResult result;if(command.Kind=="mulligan")result=Game.Mulligan(seat,command.SelectedHandIds);else if(command.Kind=="emote")result=Game.Emote(seat,command.Emote);else{if(seat!=Game.Active)return Fail("当前不是你的回合。");switch(command.Kind){case "play":result=Game.Play(seat,command.Source,command.Target,command.Position,command.Choice);break;case "attack":result=Game.Attack(seat,command.Source,command.Target);break;case "power":result=Game.HeroPower(seat,command.Target);break;case "end":result=Game.EndTurn();break;default:return Fail("未知操作。");}}if(result.Success)Revision++;return result;}
 static ActionResult Fail(string text){return new ActionResult{Message=text};}
 public LanMessage Snapshot(ActionResult result=null,int replyId=0){return new LanMessage{Kind="state",State=LanProtocol.View(Game,1,Session,Revision),ReplyId=replyId,Success=result==null||result.Success,Message=result==null?"局域网对局已连接。":result.Message};}
}
// Background transport only. Game state is mutated on the WPF dispatcher.
public sealed class LanPeer : IDisposable {
 public event Action<LanMessage> Received;public event Action<string> Disconnected;public event Action Connected;
 readonly BlockingCollection<string> outgoing=new BlockingCollection<string>(64);volatile bool closed;TcpListener listener;TcpClient client;Timer heartbeat;
 public int ListeningPort{get;private set;}public bool IsConnected{get;private set;}
 public void Host(int port){listener=new TcpListener(IPAddress.Any,port);listener.Start(1);ListeningPort=((IPEndPoint)listener.LocalEndpoint).Port;Run(()=>{client=listener.AcceptTcpClient();listener.Stop();if(!LanProtocol.PrivateAddress(((IPEndPoint)client.Client.RemoteEndPoint).Address))throw new InvalidDataException("只接受局域网连接。");StartConnection();});}
 public void Join(IPEndPoint endpoint){Run(()=>{client=new TcpClient();if(closed){client.Close();return;}var pending=client.BeginConnect(endpoint.Address,endpoint.Port,null,null);using(pending.AsyncWaitHandle){if(!pending.AsyncWaitHandle.WaitOne(5000))throw new IOException("连接超时，请检查地址与房间状态。");client.EndConnect(pending);}StartConnection();});}
 void Run(Action action){new Thread(()=>{try{action();}catch(Exception ex){if(!closed){var notify=Disconnected;Dispose();if(notify!=null)notify(ex is SocketException?"连接失败或断开，请检查局域网与防火墙。":ex.Message);}}}){IsBackground=true}.Start();}
 void StartConnection(){if(closed){client.Close();return;}client.NoDelay=true;client.ReceiveTimeout=20000;client.SendTimeout=3000;IsConnected=true;var stream=client.GetStream();Run(()=>{foreach(string text in outgoing.GetConsumingEnumerable()){if(closed)break;LanProtocol.WriteFrame(stream,text);}});heartbeat=new Timer(s=>{if(!closed)Send(new LanMessage{Kind="ping"});},null,5000,5000);var ready=Connected;if(ready!=null)ready();while(!closed){var message=JsonData.Serializer().Deserialize<LanMessage>(LanProtocol.ReadFrame(stream));if(message==null)throw new InvalidDataException("联机消息无效。");if(message.Kind=="ping")continue;var received=Received;if(received!=null)received(message);}}
 public bool Send(LanMessage message){if(closed)return false;string text=JsonData.Serializer().Serialize(message);if(Encoding.UTF8.GetByteCount(text)>LanProtocol.MaxFrame)return false;try{return outgoing.TryAdd(text);}catch(InvalidOperationException){return false;}}
 public void Dispose(){if(closed)return;closed=true;IsConnected=false;if(heartbeat!=null)heartbeat.Dispose();if(listener!=null)listener.Stop();if(client!=null)client.Close();outgoing.CompleteAdding();}
}
}
