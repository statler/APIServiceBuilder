using System;
using System.Collections.Generic;
using System.Text;

namespace APIServiceBuilder
{

    public class DeleteEntityInfo
    {
        public string EntityName { get; set; }
        public string WherePredicate { get; set; }
        public int Priority { get; set; }
        public string Dependency { get; set; }

        public DeleteEntityInfo()
        {
        }

        public DeleteEntityInfo(string entityName, string wherePredicate, int priority)
        {
            EntityName = entityName;
            WherePredicate = wherePredicate;
            Priority = priority;
        }
        public DeleteEntityInfo(string entityName, string wherePredicate, int priority, string dependency)
        {
            EntityName = entityName;
            WherePredicate = wherePredicate;
            Priority = priority;
            Dependency = dependency;
        }

        public string EntityPlural => $"{EntityName}s".Replace("ys.", "ies.").Replace("ss.", "ses.").Replace("ynopsises.", "ynopses.");

        public string DeleteString => $@"await _context.Set<{EntityName}>().Where({WherePredicate}).DeleteAsync();";
        public string UpdateString => $@"await _context.SetAllForeignKeysToNullExceptProject_BatchAsync<{EntityName}>({WherePredicate});";
    }
}
