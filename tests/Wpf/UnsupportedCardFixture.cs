using System;
using Touchfish;
namespace Touchfish {
// Isolated in-memory extension fixture; never writes the frozen package or saved decks.
public sealed class UnsupportedCardFixture : IDisposable {
 readonly CardCatalog catalog;public readonly CardRecord Card;
 public UnsupportedCardFixture(CardCatalog source){catalog=source;int dbf=2000000000;while(catalog.ByDbf.ContainsKey(dbf))dbf++;Card=new CardRecord{Id="test:unimplemented:"+dbf,BaseId="test:unimplemented:"+dbf,EffectId="test:missing@1",DbfId=dbf,Name="未实现测试牌",Text="验证未注册效果",Type="SPELL",CardClass="NEUTRAL",SetId="classic-2014",Collectible=true,Rarity="COMMON",Cost=1,Mechanics=new string[0]};catalog.Cards.Add(Card.Id,Card);catalog.ByDbf.Add(dbf,Card);}
 public void Dispose(){catalog.Cards.Remove(Card.Id);catalog.ByDbf.Remove(Card.DbfId);}
}
}
