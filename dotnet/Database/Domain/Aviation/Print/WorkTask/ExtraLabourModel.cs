// <copyright file="TimeEntryByBillingRateModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.WorkTaskModel
{
    using System;
    using System.Globalization;
    using System.Linq;

    public class ExtraLabourModel
    {
        public ExtraLabourModel(WorkTask workTask)
        {
            this.BillingRate = Math.Round(workTask.MaintenanceAgreement.HourlyRate, 2).ToString("N2", new CultureInfo("nl-BE"));
            this.AmountOfTime = Math.Round(workTask.BillableAmountOfTimeInHours.Value, 2).ToString("N2", new CultureInfo("nl-BE"));
            this.BillingAmount = Math.Round(workTask.TotalLabourRevenue, 2).ToString("N2", new CultureInfo("nl-BE"));
        }

        public string AmountOfTime { get; }

        public string BillingRate { get; }

        public string BillingAmount { get; }
    }
}
