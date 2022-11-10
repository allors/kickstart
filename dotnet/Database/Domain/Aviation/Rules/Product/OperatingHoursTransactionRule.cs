// <copyright file="OperatingHoursRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Resources;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class OperatingHoursTransactionRule : Rule
    {
        public OperatingHoursTransactionRule(MetaPopulation m) : base(m, new Guid("2456b540-1c30-44ff-9c56-888c678d0207")) =>
            this.Patterns = new Pattern[]
            {
                m.OperatingHoursTransaction.RolePattern(v => v.RecordingDate),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OperatingHoursTransaction>())
            {
                if (@this.RecordingDate.Date > transaction.Now().Date)
                {
                    // check country specific structure
                    validation.AddError(@this, @this.Meta.RecordingDate, ErrorMessages.FutureDateInvalid);
                }
            }
        }
    }
}
