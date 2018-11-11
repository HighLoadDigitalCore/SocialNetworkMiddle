using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Smoking.Models
{
    public class SplittedText
    {
        public SplittedText(string inp, string[] delims = null)
        {
            if (delims == null)
            {
                delims = new string[] {"{MAPSELECT}"};
            }
            HasControl = delims.Where(inp.Contains).Any();
            if (HasControl)
            {
                var first = delims.First(inp.Contains);
                int si = inp.IndexOf(first);
                if (si >= 0)
                    First = inp.Substring(0, si);
                Last = inp.Substring(si + 1 + first.Length);
            }
            else
            {
                First = inp;
            }
        }
        public string First { get; set; }
        public string Last { get; set; }
        public bool HasControl { get; set; }
    }

    public class MapModel
    {

        private DB db = new DB();

        private List<MapObjectType> _objectTypes;
        public List<MapObjectType> ObjectTypes
        {
            get { return _objectTypes ?? (_objectTypes = db.MapObjectTypes.OrderBy(x => x.OrderNum).ToList()); }
        } 
    }
}