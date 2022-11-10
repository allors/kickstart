// <copyright file="SalesOrderDerivedVatClauseDescriptionDerivation.cs" company="Allors bvba">
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
    public class SalesOrderDerivedVatClauseDescriptionRule : Rule
    {
        public SalesOrderDerivedVatClauseDescriptionRule(MetaPopulation m) : base(m, new Guid("e43b7dd9-6d5f-442d-9d3d-4859a9a0518a")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.AssignedVatClause),
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.SalesOrder.RolePattern(v => v.DerivedVatRegime),
                m.SalesOrder.RolePattern(v => v.ValidOrderItems),
                m.SalesOrder.RolePattern(v => v.SalesTerms),
                m.PostalAddress.RolePattern(v => v.Country, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.Party.Party.AsOrganisation.SalesOrdersWhereTakenBy),
                m.PostalAddress.RolePattern(v => v.Country, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.Party.Party.SalesOrdersWhereBillToCustomer),
                m.PartyContactMechanism.RolePattern(v => v.ContactPurposes, v => v.Party.Party.AsOrganisation.SalesOrdersWhereTakenBy),
                m.PartyContactMechanism.RolePattern(v => v.Party, v => v.Party.Party.AsOrganisation.SalesOrdersWhereTakenBy),
                m.SalesOrderItem.RolePattern(v => v.DerivedShipFromAddress, v => v.SalesOrderWhereSalesOrderItem),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.OrderWhereSalesTerm, m.SalesOrder),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                VatClause vatClause = null;
                if (@this.ExistDerivedVatRegime && @this.DerivedVatRegime.ExistVatClause)
                {
                    vatClause = @this.DerivedVatRegime.VatClause;
                }
                else
                {
                    string TakenbyCountry = null;

                    if (@this.ExistTakenBy && @this.TakenBy.PartyContactMechanismsWhereParty?.FirstOrDefault(v => v.ContactPurposes.Any(p => Equals(p, new ContactMechanismPurposes(transaction).RegisteredOffice)))?.ContactMechanism is PostalAddress registeredOffice)
                    {
                        TakenbyCountry = registeredOffice.Country?.IsoCode;
                    }

                    var OutsideEUCustomer = @this.DerivedVatRegime?.Equals(new VatRegimes(transaction).ZeroRated);
                    var shipFromBelgium = @this.ValidOrderItems?.Cast<SalesOrderItem>().All(v => Equals("BE", v.DerivedShipFromAddress?.Country?.IsoCode));
                    var shipToEU = @this.ExistValidOrderItems && @this.ValidOrderItems.Cast<SalesOrderItem>().Any(v => Equals(true, v.DerivedShipToAddress?.Country?.EuMemberState));
                    var sellerResponsibleForTransport = @this.ExistSalesTerms && @this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(transaction).Cif) || Equals(v.TermType, new IncoTermTypes(transaction).Cfr));
                    var buyerResponsibleForTransport = @this.ExistSalesTerms && @this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(transaction).Exw));

                    if (Equals(@this.DerivedVatRegime, new VatRegimes(transaction).ServiceB2B))
                    {
                        vatClause = new VatClauses(transaction).ServiceB2B;
                    }
                    else if (Equals(@this.DerivedVatRegime, new VatRegimes(transaction).IntraCommunautair))
                    {
                        vatClause = new VatClauses(transaction).IntraCommunautair;
                    }
                    else if (TakenbyCountry == "BE"
                             && OutsideEUCustomer.HasValue && OutsideEUCustomer.Value
                             && shipFromBelgium.HasValue && shipFromBelgium.Value
                             && @this.ExistValidOrderItems
                             && !shipToEU)
                    {
                        if (sellerResponsibleForTransport)
                        {
                            // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU and you transport the goods and importer is the customer
                            vatClause = new VatClauses(transaction).BeArt39Par1Item1;
                        }
                        else if (buyerResponsibleForTransport)
                        {
                            // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU  and the customer does the transport of the goods and importer is the customer
                            vatClause = new VatClauses(transaction).BeArt39Par1Item2;
                        }
                    }
                }

                @this.DerivedVatClause = @this.ExistAssignedVatClause ? @this.AssignedVatClause : vatClause;
            }
        }
    }
}
