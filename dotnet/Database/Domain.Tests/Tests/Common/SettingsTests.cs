// <copyright file="PurchaseInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Meta;
    using Resources;
    using System.Linq;
    using Xunit;

    public class SettingsCalculationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SettingsCalculationRuleTests(Fixture fixture) : base(fixture) {}

        [Fact]
        public void ChangedCleaningCalculationThrowExpressionError()
        {
            this.Transaction.GetSingleton().Settings.CleaningCalculation = "1 * 1M";

            var errors = this.Derive().Errors.ToList();
            Assert.Equal(new IRoleType[]
            {
                this.M.Settings.CleaningCalculation,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedCleaningCalculationThrowNotImplementedError()
        {
            this.Transaction.GetSingleton().Settings.CleaningCalculation = "[2]";

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("not implemented:"));
        }

        [Fact]
        public void ChangedSundriesCalculationThrowExpressionError()
        {
            this.Transaction.GetSingleton().Settings.SundriesCalculation = "1 * 1M";

            var errors = this.Derive().Errors.ToList();
            Assert.Equal(new IRoleType[]
            {
                this.M.Settings.SundriesCalculation,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedSundriesCalculationThrowNotImplementedError()
        {
            this.Transaction.GetSingleton().Settings.SundriesCalculation = "[2]";

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("not implemented:"));
        }
    }
}
