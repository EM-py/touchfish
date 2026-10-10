using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Touchfish {
public static class DeckStorage {
 public static bool IsReadError(Exception ex){return ex is IOException||ex is UnauthorizedAccessException||ex is InvalidDataException||ex is ArgumentException||ex is InvalidOperationException;}
 public static string UserDirectory(){string root=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);if(String.IsNullOrWhiteSpace(root))root=AppDomain.CurrentDomain.BaseDirectory;return Path.Combine(root,"YuziPlus","Touchfish","decks");}
 static string LibraryPath(string directory){return Path.Combine(directory,"library.json");}
 static string DraftPath(string directory){return Path.Combine(directory,"draft.json");}
 static List<SavedDeck> ReadLibrary(string directory){string path=LibraryPath(directory);if(!File.Exists(path))return new List<SavedDeck>();return JsonData.Read<List<SavedDeck>>(path)??new List<SavedDeck>();}
 public static string ExistingDraftPath(string userDirectory,string legacyDirectory){string current=DraftPath(userDirectory);if(File.Exists(current))return current;string legacy=DraftPath(legacyDirectory);return File.Exists(legacy)?legacy:null;}
 public static List<SavedDeck> LoadLibrary(string userDirectory,string legacyDirectory){return File.Exists(LibraryPath(userDirectory))?ReadLibrary(userDirectory):Merge(ReadLibrary(legacyDirectory),null);}
 static List<SavedDeck> Merge(IEnumerable<SavedDeck> preferred,IEnumerable<SavedDeck> legacy){var result=new List<SavedDeck>();var ids=new HashSet<string>(StringComparer.OrdinalIgnoreCase);foreach(var source in new[]{preferred,legacy})foreach(var value in source??Enumerable.Empty<SavedDeck>()){if(value==null||String.IsNullOrWhiteSpace(value.Code))continue;var copy=new SavedDeck{Id=String.IsNullOrWhiteSpace(value.Id)?Guid.NewGuid().ToString("N"):value.Id,Name=String.IsNullOrWhiteSpace(value.Name)?"未命名卡组":value.Name,PoolId=String.IsNullOrWhiteSpace(value.PoolId)?"classic-2014":value.PoolId,Code=value.Code};if(result.Any(saved=>saved.Id==copy.Id&&saved.Code==copy.Code&&saved.PoolId==copy.PoolId))continue;if(ids.Contains(copy.Id)){copy.Id=Guid.NewGuid().ToString("N");while(ids.Contains(copy.Id))copy.Id=Guid.NewGuid().ToString("N");}ids.Add(copy.Id);result.Add(copy);}return result;}
 public static void SaveLibrary(string userDirectory,List<SavedDeck> decks){Directory.CreateDirectory(userDirectory);JsonData.Write(LibraryPath(userDirectory),Merge(decks,null));}
 public static void Save(string userDirectory,string legacyDirectory,DeckDocument draft,List<SavedDeck> decks){SaveLibrary(userDirectory,decks);if(draft!=null)JsonData.Write(DraftPath(userDirectory),draft);}
}
}
