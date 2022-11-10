
// <copyright file="WorkRequirementStateRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class AviationWorkRequirementStateRule : Rule
    {
        public AviationWorkRequirementStateRule(MetaPopulation m) : base(m, new Guid("845f597e-9c12-42bd-97ee-db0dcd3c5de0")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.WorkEffortState, v => v.WorkRequirementFulfillmentsWhereFullfillmentOf.WorkRequirementFulfillment.FullfilledBy),
            m.WorkRequirementFulfillment.RolePattern(v => v.FullfilledBy, v => v.FullfilledBy),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                var workEffort = @this.WorkRequirementFulfillmentWhereFullfilledBy.FullfillmentOf;

                if (workEffort.WorkEffortState.IsCompleted || workEffort.WorkEffortState.IsFinished)
                {
                    @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).Finished;
                }
                else if (workEffort.WorkEffortState.IsCancelled)
                {
                    if (!@this.RequirementState.IsFinished)
                    {
                        @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).Cancelled;
                    }
                }
                else if (!@this.RequirementState.IsFinished)
                {
                    @this.RequirementState = new RequirementStates(@this.Strategy.Transaction).InProgress;
                }
            }
        }
    }
}
