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
    using Meta;
    using Derivations.Rules;

    public class VehicleOwnedByPartyNameRule : Rule
    {
        public VehicleOwnedByPartyNameRule(MetaPopulation m) : base(m, new Guid("5166f404-eed8-4e65-80a2-9123c3ef205c")) =>
            this.Patterns = new Pattern[]
            {
                m.Vehicle.RolePattern(v => v.OwnedBy),
                m.Party.RolePattern(v => v.DisplayName, v => v.VehiclesWhereOwnedBy.Vehicle),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Vehicle>())
            {
                @this.OwnedByPartyName = @this.ExistOwnedBy ? @this.OwnedBy.DisplayName : string.Empty;
            }
        }
    }
}
