// <copyright file="Settingses.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Settingses
    {
        protected override void CustomSetup(Setup setup)
        {
            var singleton = this.Transaction.GetSingleton();
            singleton.Settings ??= new SettingsBuilder(this.Transaction)
                .WithUseProductNumberCounter(true)
                .WithUsePartNumberCounter(true)
                .Build();

            var settings = singleton.Settings;

            var inventoryStrategy = new InventoryStrategies(this.Transaction).Standard;
            var preferredCurrency = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            settings.InventoryStrategy ??= inventoryStrategy;
            settings.SkuPrefix ??= "Sku-";
            settings.SerialisedItemPrefix ??= "S-";
            settings.ProductNumberPrefix ??= "Art-";
            settings.PartNumberPrefix ??= "Part-";
            settings.PreferredCurrency ??= preferredCurrency;
            settings.InternalLabourSurchargePercentage = 10M;
            settings.InternalPartSurchargePercentage = 10M;
            settings.PartSurchargePercentage = 20M;
            settings.InternalSubletSurchargePercentage = 10M;
            settings.SubletSurchargePercentage = 15M;

            settings.SkuCounter ??= new CounterBuilder(this.Transaction).Build();
            settings.SerialisedItemCounter ??= new CounterBuilder(this.Transaction).Build();
            settings.ProductNumberCounter ??= new CounterBuilder(this.Transaction).Build();
            settings.PartNumberCounter ??= new CounterBuilder(this.Transaction).Build();

            settings.CleaningCalculation = "[1] * 0.2 + 14.5"; // [1] = WorkEffort.BillableTimeEntries().Sum(v => v.BillableAmountOfTimeInMinutes) / 60
            settings.SundriesCalculation = "[1] * 0.9 + 7.5";
        }
    }
}
