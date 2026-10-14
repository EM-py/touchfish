using System;using System.Linq;
namespace Touchfish {
public static class MiracleRogueDeck {
 // Classic list adapted from https://www.icy-veins.com/hearthstone/classic-miracle-rogue-deck
 // One Earthen Ring Farseer is replaced by Sprint for an independent draw engine.
 public static DeckDocument Create(CardCatalog catalog){
  var deck=new DeckDocument{ClassId="ROGUE",PoolId="classic-2014",Name="经典奇迹贼"};
  foreach(string key in new[]{"CS2_072","EX1_145","EX1_144","CS2_073","EX1_128","CS2_074","EX1_124","EX1_581","EX1_129","EX1_134","EX1_284","EX1_095"})
   deck.Cards.Add(catalog.Deckable(catalog.Pool(deck.PoolId)).Single(c=>MatchRules.EffectKey(c)==key).Id,2);
  foreach(string key in new[]{"CS2_233","EX1_613","EX1_012","CS2_117","EX1_116","CS2_077"})
   deck.Cards.Add(catalog.Deckable(catalog.Pool(deck.PoolId)).Single(c=>MatchRules.EffectKey(c)==key).Id,1);
  DeckRules.Validate(catalog,deck,true);return deck;
 }
}
}
