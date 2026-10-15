using System;using System.Linq;using System.IO;using Touchfish;
public static partial class RuleTests {
 static void NaxxLibrary(string rootPath){
  var classic=catalog.Pool("classic-2014");var expanded=catalog.Pool("naxxramas-2014");var additions=catalog.BrowseDeckable("naxxramas-2014");
  Check(catalog.Deckable(classic).Count==382&&catalog.Deckable(expanded).Count==412,"Classic 382 and Naxx era 412 remain separate legal pools");
  Check(additions.Count==30&&additions.Count(c=>c.CardClass=="NEUTRAL")==21,"Naxx imports exactly 30 cards, including 21 neutral");
  Check(classic.HeroDbfIds.Keys.All(cl=>additions.Count(c=>c.CardClass==cl)==1),"Naxx has one card per classic class");
  Check(catalog.Cards.Values.Count(c=>c.SetId=="naxxramas-2014"&&!c.Collectible)==5&&catalog.Cards.Values.Where(c=>c.SetId=="naxxramas-2014").All(c=>c.Race!="UNDEAD"),"Only five summon helpers; no later Undead type");
  Check(catalog.Card("FP1_002t").Race=="BEAST"&&catalog.Card("FP1_022").Race=="DEMON","Original Beast and Demon types preserved");
  Check(catalog.Card("VAN_EX1_116").Cost==4&&catalog.Card("VAN_CS2_237").Cost==2&&catalog.Card("VAN_CS2_237").Attack==2&&catalog.Card("VAN_CS2_237").Health==1,"August cutoff excludes September Leeroy and Buzzard nerfs");
  Check(additions.All(c=>!matchRules.Supports(c)),"Unimplemented Naxx effects explicitly blocked instead of silently becoming vanilla minions");
  var deck=matchRules.TrainingDeck("HUNTER");deck.Cards.Clear();deck.Cards["VAN_EX1_536"]=2;foreach(var card in catalog.Deckable(classic).Where(c=>(c.CardClass=="NEUTRAL"||c.CardClass=="HUNTER")&&c.Rarity!="LEGENDARY"&&c.Id!="VAN_EX1_536").Take(14))deck.Cards.Add(card.Id,2);
  string originalCode=DeckCode.Encode(catalog,deck);var migrated=DeckCode.Decode(catalog,originalCode,expanded.Id);
  Check(migrated.Cards.ContainsKey("NAX_EX1_536")&&!migrated.Cards.ContainsKey("VAN_EX1_536")&&migrated.Count==30,"Classic code imported into Naxx pool resolves era-specific bow without duplicate copies");
  Check(DeckCode.Decode(catalog,DeckCode.Encode(catalog,migrated),classic.Id).Cards.ContainsKey("VAN_EX1_536"),"Naxx bow code imported into pure Classic resolves original bow");
  var newDeck=new DeckDocument{ClassId="HUNTER",PoolId=expanded.Id};Check(DeckRules.CanAdd(catalog,newDeck,catalog.Card("VAN_EX1_536"))!=null&&DeckRules.CanAdd(catalog,newDeck,catalog.Card("NAX_EX1_536"))==null,"Old and new bow cannot both enter the Naxx pool");
  var host=new LanHostGame(catalog,DeckCode.Decode(catalog,originalCode,expanded.Id),DeckCode.Decode(catalog,originalCode,expanded.Id),54321);host.Game.Equip(0,"NAX_EX1_536");var view=LanProtocol.ReadView(catalog,host.Snapshot().State);Check(view.Players[0].WeaponId=="NAX_EX1_536"&&view.Card(view.Players[0].WeaponId).FriendlySecretsOnly,"LAN snapshot preserves the era-specific card identity and resolves its rules");
  Check(DeckRules.CanAdd(catalog,deck,catalog.Card("FP1_011"))!=null,"Naxx cards cannot enter pure Classic");
  migrated.Cards.Clear();migrated.Cards["FP1_011"]=1;var rival=new DeckDocument{ClassId="MAGE",PoolId=expanded.Id};bool blocked=false;try{new MatchEngine(catalog,migrated,rival,1,testMode:true);}catch(InvalidOperationException){blocked=true;}Check(blocked,"Actual match opening rejects unsupported Naxx card");
  var g=Game();g.Equip(0,"VAN_EX1_536");g.Equip(1,"NAX_EX1_536");HunterRules.SecretRevealed(g,0);Check(g.Players[0].WeaponDurability==3&&g.Players[1].WeaponDurability==2,"Classic bow accepts any secret; Naxx bow ignores enemy reveal");HunterRules.SecretRevealed(g,1);Check(g.Players[0].WeaponDurability==4&&g.Players[1].WeaponDurability==3,"Both era bows accept their owner's reveal, original also accepts enemy reveal");
  JsonData.Write(Path.Combine(rootPath,"previews","match-rule-coverage.json"),new{SupportedCards=382,TotalCards=412,ClassicSupported=382,ClassicTotal=382,NaxxSupported=0,NaxxTotal=30,Cards=catalog.Deckable(expanded).Select(c=>new{Id=c.Id,Name=c.Name,Supported=matchRules.Supports(c)}).ToArray(),Scope="Independent Classic and August-2014 Naxx card pools; Naxx effects not implemented"});
 }
}
