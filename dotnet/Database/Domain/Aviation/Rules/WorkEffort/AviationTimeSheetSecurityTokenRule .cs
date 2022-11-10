// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class AviationTimeSheetSecurityTokenRule : Rule
    {
        public AviationTimeSheetSecurityTokenRule(MetaPopulation m) : base(m, new Guid("314d5cc2-2a8d-4277-b17b-be43f6f41a5a")) =>
            this.Patterns = new Pattern[]
        {
            m.TimeSheet.RolePattern(v => v.Worker),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeSheet>())
            {
                @this.SecurityTokens = @this.Worker.InternalOrganisationsWhereActiveEmployee
                    .SelectMany(v => new[] { v.BlueCollarWorkerSecurityToken, v.LocalAdministratorSecurityToken, v.LocalEmployeeSecurityToken })
                    .Where(v => v != null)
                    .Append(new SecurityTokens(transaction).DefaultSecurityToken)
                    .Append(@this.Worker.OwnerSecurityToken)
                    .ToArray();
            }
        }
    }
}
