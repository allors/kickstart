// <copyright file="WorkEffortSalesInvoiceItemAssignmentRule.cs" company="Allors bvba">
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
    public class WorkEffortInvoiceItemAssignmentRule : Rule
    {
        public WorkEffortInvoiceItemAssignmentRule(MetaPopulation m) : base(m, new Guid("279bcb97-586f-4b6f-a91f-3dffaeac61f2")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortInvoiceItemAssignment.RolePattern(v => v.WorkEffortInvoiceItem),
                m.WorkEffortInvoiceItem.RolePattern(v => v.InvoiceItemType , v => v.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInvoiceItemAssignment>())
            {
                if (@this.ExistWorkEffortInvoiceItem
                    && !@this.WorkEffortInvoiceItem.InvoiceItemType.IsCleaning
                    && !@this.WorkEffortInvoiceItem.InvoiceItemType.IsSundries
                    && !@this.WorkEffortInvoiceItem.InvoiceItemType.IsOther)
                {
                    validation.AddError($"{@this}, {this.M.WorkEffortInvoiceItemAssignment.WorkEffortInvoiceItem}, {ErrorMessages.InvoiceItemTypeNotAllowed}");
                }
            }
        }
    }
}
