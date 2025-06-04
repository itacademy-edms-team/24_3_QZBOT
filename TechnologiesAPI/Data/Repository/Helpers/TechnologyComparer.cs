using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Helpers
{
    public class TechnologyComparer : IEqualityComparer<Technology>
    {
        public bool Equals(Technology x, Technology y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(Technology obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
