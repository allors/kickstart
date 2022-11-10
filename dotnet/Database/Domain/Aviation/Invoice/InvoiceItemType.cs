// <copyright file="InvoiceItemType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class InvoiceItemType
    {
        public bool IsRepairAndMaintenance => this.UniqueId == InvoiceItemTypes.RmId;

        public bool IsTransport => this.UniqueId == InvoiceItemTypes.TransportId;

        public bool IsGseUnmotorized => this.UniqueId == InvoiceItemTypes.GseUnmotorizedId;

        public bool IsOther => this.UniqueId == InvoiceItemTypes.OtherId;

        public bool IsCleaning => this.UniqueId == InvoiceItemTypes.CleaningId;

        public bool IsSundries => this.UniqueId == InvoiceItemTypes.SundriesId;
    }
}
