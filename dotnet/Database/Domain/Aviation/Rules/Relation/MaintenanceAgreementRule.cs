// <copyright file="MaintenanceAgreementCustomDerivation.cs" company="Allors bvba">
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
    public class MaintenanceAgreementRule : Rule
    {
        public MaintenanceAgreementRule(MetaPopulation m) : base(m, new Guid("06a1e707-083d-4b2a-8845-feb9854880b4")) =>
            this.Patterns = new Pattern[]
            {
                m.MaintenanceAgreement.RolePattern(v => v.WorkEffortType),
                m.MaintenanceAgreement.RolePattern(v => v.Description),
                m.WorkEffortType.RolePattern(v => v.Description, v => v.MaintenanceAgreementsWhereWorkEffortType),
                m.CustomerRelationship.RolePattern(v => v.InternalOrganisation, v => v.Agreements, m.MaintenanceAgreement),
                m.CustomerRelationship.RolePattern(v => v.CustomerName, v => v.Agreements, m.MaintenanceAgreement),
                m.MaintenanceAgreement.AssociationPattern(v => v.PartyRelationshipWhereAgreement, v => v.PartyRelationshipWhereAgreement.PartyRelationship.AsCustomerRelationship.Agreements.Agreement.AsMaintenanceAgreement),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<MaintenanceAgreement>())
            {
                var customerRelationship = @this.PartyRelationshipWhereAgreement as CustomerRelationship;

                @this.CustomerName = customerRelationship?.CustomerName;
                @this.InternalOrganisation = customerRelationship?.InternalOrganisation;

                if (!@this.ExistDescription && @this.ExistWorkEffortType)
                {
                    @this.Description = @this.WorkEffortType.Description;
                }
            }
        }
    }
}
