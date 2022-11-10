// <copyright file="PersonLastTimeEntryRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class PersonLastTimeEntryRule : Rule
    {
        public PersonLastTimeEntryRule(MetaPopulation m) : base(m, new Guid("a50f9a96-d78b-4b39-860c-08abe34a4d80")) =>
            this.Patterns = new Pattern[]
            {
                m.TimeEntry.RolePattern(v => v.FromDate, v => v.Worker),
                m.Person.AssociationPattern(v => v.TimeEntriesWhereWorker),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.DerivePersonLastTimeEntry(validation);
            }
        }
    }

    public static class PersonLastTimeEntryRuleExtensions
    {
        public static void DerivePersonLastTimeEntry(this Person @this, IValidation validation)
        {
            @this.LastTimeEntry = @this.TimeSheetWhereWorker?.TimeEntries.OrderByDescending(v => v.FromDate).FirstOrDefault();
        }
    }
}
