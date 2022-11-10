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

    public class AviationVehicleSearchStringRule : Rule
    {
        public AviationVehicleSearchStringRule(MetaPopulation m) : base(m, new Guid("2651c627-068d-4ec2-9ecc-6bfaf15bc3ef")) =>
            this.Patterns = new Pattern[]
        {
            m.Vehicle.RolePattern(v => v.LicensePlateNumber),
            m.Vehicle.RolePattern(v => v.ChassisNumber),
            m.Vehicle.RolePattern(v => v.Make),
            m.Vehicle.RolePattern(v => v.Model),
            m.FixedAsset.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.Party.RolePattern(v => v.DisplayName, v => v.PartyFixedAssetAssignmentsWhereParty.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Vehicle),
            m.AssetAssignmentStatus.RolePattern(v => v.Name, v => v.PartyFixedAssetAssignmentsWhereAssetAssignmentStatus.PartyFixedAssetAssignment.FixedAsset.FixedAsset, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset, m.Vehicle),
            m.FixedAsset.AssociationPattern(v => v.WorkRequirementsWhereFixedAsset, m.Vehicle),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Vehicle>())
            {
                @this.DeriveAviationVehicleSearchString(validation);
            }
        }
    }

    public static class AviationVehicleSearchStringRuleExtensions
    {
        public static void DeriveAviationVehicleSearchString(this Vehicle @this, IValidation validation)
        {
            var array = new string[] {
                    @this.Make,
                    @this.Model,
                    @this.LicensePlateNumber,
                    @this.ChassisNumber,
                    @this.ExistPartyFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Party?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPartyFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.PartyFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.AssetAssignmentStatus?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistWorkEffortFixedAssetAssignmentsWhereFixedAsset ? string.Join(" ", @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset?.Select(v => v.Assignment?.WorkEffortNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistWorkRequirementsWhereFixedAsset ? string.Join(" ", @this.WorkRequirementsWhereFixedAsset?.Select(v => v.RequirementNumber ?? string.Empty).ToArray()) : null,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
