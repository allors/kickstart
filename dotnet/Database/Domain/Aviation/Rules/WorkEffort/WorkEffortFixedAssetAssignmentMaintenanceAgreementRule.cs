// <copyright file="WorkEffortFixedAssetAssignmentMaintenanceAgreementDerivation.cs" company="Allors bvba">
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
    public class WorkEffortFixedAssetAssignmentMaintenanceAgreementRule : Rule
    {
        public WorkEffortFixedAssetAssignmentMaintenanceAgreementRule(MetaPopulation m) : base(m, new Guid("d700bede-7468-48ce-ae98-1e42be151055")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortFixedAssetAssignment.RolePattern(v => v.Assignment),
                m.WorkEffortFixedAssetAssignment.RolePattern(v => v.FixedAsset),
                m.WorkTask.RolePattern(v => v.MaintenanceAgreement , v => v.WorkEffortFixedAssetAssignmentsWhereAssignment),
                m.MaintenanceAgreement.RolePattern(v => v.WorkEffortType , v => v.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortFixedAssetAssignmentsWhereAssignment),
                m.WorkEffortType.RolePattern(v => v.UnifiedGood , v => v.MaintenanceAgreementsWhereWorkEffortType.MaintenanceAgreement.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortFixedAssetAssignmentsWhereAssignment),
                m.WorkEffortType.RolePattern(v => v.ProductCategory, v => v.MaintenanceAgreementsWhereWorkEffortType.MaintenanceAgreement.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortFixedAssetAssignmentsWhereAssignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortFixedAssetAssignment>())
            {
                if (@this.ExistAssignment
                    && ((WorkTask)@this.Assignment).ExistMaintenanceAgreement
                    && ((WorkTask)@this.Assignment).MaintenanceAgreement.WorkEffortType.ExistUnifiedGood
                    && ((WorkTask)@this.Assignment).MaintenanceAgreement.WorkEffortType.UnifiedGood != ((SerialisedItem)@this.FixedAsset).PartWhereSerialisedItem)
                {
                    validation.AddError($"{@this}, {this.M.WorkEffortFixedAssetAssignment.FixedAsset}, {ErrorMessages.SerialisedItemNotInMaintenanceAgreement}");
                }

                if (@this.ExistAssignment)
                {
                    var productCategory = ((WorkTask)@this.Assignment).MaintenanceAgreement?.WorkEffortType?.ProductCategory;
                    var parentCategory = productCategory?.PrimaryParent;

                    if (((WorkTask)@this.Assignment).ExistMaintenanceAgreement
                        && ((WorkTask)@this.Assignment).MaintenanceAgreement.WorkEffortType.ExistProductCategory
                        && ((SerialisedItem)@this.FixedAsset).PartWhereSerialisedItem.ProductCategoriesWhereProduct.FirstOrDefault(v => v.Equals(productCategory)) != null
                        && ((SerialisedItem)@this.FixedAsset).PartWhereSerialisedItem.ProductCategoriesWhereProduct.FirstOrDefault(v => v.Equals(parentCategory)) != null)
                    {
                        validation.AddError($"{@this}, {this.M.WorkEffortFixedAssetAssignment.FixedAsset}, {ErrorMessages.SerialisedItemNotInMaintenanceAgreement}");
                    }
                }
            }
        }
    }
}
