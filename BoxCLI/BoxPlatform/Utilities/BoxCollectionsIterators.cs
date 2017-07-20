using System;
using System.Threading.Tasks;
using Box.V2;
using Box.V2.Models;
using Microsoft.Extensions.Logging;

namespace BoxCLI.BoxPlatform.Utilities
{
    public class BoxCollectionsIterators : IBoxCollectionsIterators
    {
        public string PageInConsole<T>(Action<T> print, BoxCollection<T> collection) where T : class, new()
        {
            print(collection.Entries[0]);
            collection.Entries.RemoveAt(0);
            System.Console.Write("Show next? Enter q to quit. ");
            return System.Console.ReadLine().Trim().ToLower();
        }
        public string PageInConsole<T>(Action<T> print, BoxCollectionMarkerBased<T> collection) where T : class, new()
        {
            print(collection.Entries[0]);
            collection.Entries.RemoveAt(0);
            System.Console.Write("Show next? Enter q to quit. ");
            return System.Console.ReadLine().Trim().ToLower();
        }

        public async Task ListOffsetCollectionToConsole<T>(Func<uint, Task<BoxCollection<T>>> callBox, Action<T> print) where T : BoxEntity, new()
        {
            var showNext = "";
            uint offset = 0;
            var collection = await callBox(offset);
            var all = collection.TotalCount;
            while (all > 0 && showNext != "q")
            {
                if (collection.Entries.Count > 0)
                {
                    all--;
                    showNext = PageInConsole(print, collection);
                }
                else
                {
                    offset += (uint)collection.Limit;
                    collection = await callBox(offset);
                }
            }
        }

        public async Task ListMarkerCollectionToConsole<T>(Func<string, Task<BoxCollectionMarkerBased<T>>> callBox, Action<T> print) where T : BoxEntity, new()
        {
            var keepGoing = false;
            var showNext = "";
            do
            {
                string marker = "";
                var collection = await callBox(marker);
                if (collection.Entries.Count > 0)
                {
                    while (collection.Entries.Count > 0 && showNext != "q")
                    {
                        showNext = PageInConsole(print, collection);
                    }
                }
                else
                {
                    marker = collection.NextMarker;
                    collection = await callBox(marker);
                }
                keepGoing = !string.IsNullOrEmpty(collection.NextMarker);
            }
            while (keepGoing && showNext != "q");
        }
    }
}