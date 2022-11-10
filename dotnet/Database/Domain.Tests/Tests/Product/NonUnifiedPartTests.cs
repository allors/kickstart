// <copyright file="NonUnifiedPartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Derivations;
using Resources;
using System.Collections.Generic;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class NonUnifiedPartUomNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartUomNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedUnitOfMeasureDeriveSpanishUOM()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var kilogram = new UnitsOfMeasure(this.Transaction).Kilogram;
            kilogram.RemoveLocalisedNames();
            this.Transaction.Derive(false);

            part.UnitOfMeasure = kilogram;
            this.Transaction.Derive(false);

            Assert.Contains(kilogram.Name, part.SpanishUOM);
        }

        [Fact]
        public void ChangedUnitOfMeasureNameDeriveSpanishUOM()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var kilogram = new UnitsOfMeasure(this.Transaction).Kilogram;
            kilogram.RemoveLocalisedNames();
            this.Transaction.Derive(false);

            part.UnitOfMeasure = kilogram;
            this.Transaction.Derive(false);

            kilogram.Name = "changed";
            this.Transaction.Derive(false);

            Assert.Contains("changed", part.SpanishUOM);
        }

        [Fact]
        public void ChangedUnitOfMeasureLocalisedNamesDeriveSpanishUOM()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var kilogram = new UnitsOfMeasure(this.Transaction).Kilogram;
            kilogram.RemoveLocalisedNames();
            this.Transaction.Derive(false);

            part.UnitOfMeasure = kilogram;
            this.Transaction.Derive(false);

            kilogram.AddLocalisedName(new LocalisedTextBuilder(this.Transaction).WithLocale(new Locales(this.Transaction).Spanish).WithText("kilogramo").Build());
            this.Transaction.Derive(false);

            Assert.Contains("kilogramo", part.SpanishUOM);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveSpanishUOM()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var kilogram = new UnitsOfMeasure(this.Transaction).Kilogram;
            kilogram.RemoveLocalisedNames();
            this.Transaction.Derive(false);

            part.UnitOfMeasure = kilogram;
            this.Transaction.Derive(false);

            var spanish = new LocalisedTextBuilder(this.Transaction).WithLocale(new Locales(this.Transaction).Spanish).WithText("kilogramo").Build();
            kilogram.AddLocalisedName(spanish);
            this.Transaction.Derive(false);

            spanish.Text = "changed";
            this.Transaction.Derive(false);

            Assert.Contains("changed", part.SpanishUOM);
        }

        [Fact]
        public void ChangedUnitOfMeasureLocalisedNamesDeriveDutchUOM()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var kilogram = new UnitsOfMeasure(this.Transaction).Kilogram;
            kilogram.RemoveLocalisedNames();
            this.Transaction.Derive(false);

            part.UnitOfMeasure = kilogram;
            this.Transaction.Derive(false);

            kilogram.AddLocalisedName(new LocalisedTextBuilder(this.Transaction).WithLocale(new Locales(this.Transaction).DutchNetherlands).WithText("kilogrammen").Build());
            this.Transaction.Derive(false);

            Assert.Contains("kilogrammen", part.DutchUOM);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDutchUOM()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var kilogram = new UnitsOfMeasure(this.Transaction).Kilogram;
            kilogram.RemoveLocalisedNames();
            this.Transaction.Derive(false);

            part.UnitOfMeasure = kilogram;
            this.Transaction.Derive(false);

            var spanish = new LocalisedTextBuilder(this.Transaction).WithLocale(new Locales(this.Transaction).DutchNetherlands).WithText("kilogrammen").Build();
            kilogram.AddLocalisedName(spanish);
            this.Transaction.Derive(false);

            spanish.Text = "changed";
            this.Transaction.Derive(false);

            Assert.Contains("changed", part.DutchUOM);
        }
    }

    public class NonUnifiedPartBrandRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartBrandRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBrandThrowValidationError()
        {
            var brand = new BrandBuilder(this.Transaction).Build();
            var model = new ModelBuilder(this.Transaction).Build();
            brand.AddModel(model);

            var part1 = new NonUnifiedPartBuilder(this.Transaction).WithBrand(brand).WithModel(model).Build();
            this.Transaction.Derive(false);

            var part2 = new NonUnifiedPartBuilder(this.Transaction).WithBrand(brand).WithModel(model).Build();

            var expectedMessage = $"{part2}, { this.M.NonUnifiedPart.Brand}, { ErrorMessages.ProductExists}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedModelThrowValidationError()
        {
            var brand = new BrandBuilder(this.Transaction).Build();
            var model1 = new ModelBuilder(this.Transaction).Build();
            var model2 = new ModelBuilder(this.Transaction).Build();
            brand.AddModel(model1);
            brand.AddModel(model2);

            var part1 = new NonUnifiedPartBuilder(this.Transaction).WithBrand(brand).WithModel(model1).Build();
            this.Transaction.Derive(false);

            var part2 = new NonUnifiedPartBuilder(this.Transaction).WithBrand(brand).WithModel(model2).Build();
            this.Transaction.Derive(false);

            part2.Model = model1;

            var expectedMessage = $"{part2}, { this.M.NonUnifiedPart.Brand}, { ErrorMessages.ProductExists}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
