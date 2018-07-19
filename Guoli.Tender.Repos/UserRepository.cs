using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guoli.Tender.Model;

namespace Guoli.Tender.Repos
{
    public sealed class UserRepository: Repository<TenderContext, User, int>
    {
        public User GetByUsername(string username)
        {
            var set = GetSet();
            return set.SingleOrDefault(u => u.Username == username);
        }
    }
}
