// <copyright file="SerialisedItemCharacteristicType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SerialisedItemCharacteristicType
    {
        public bool IsOperatingHours => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).OperatingHours);
        public bool IsLength => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).Length);
        public bool IsWidth => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).Width);
        public bool IsHeight => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).Height);
        public bool IsWeight => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).Weight);
        public bool IsChassisNumber => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).ChassisNumber);
        public bool IsEngineBrand => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).EngineBrand);
        public bool IsEngineModel => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).EngineModel);
        public bool IsEngineSerialNumber => this.Equals(new SerialisedItemCharacteristicTypes(this.Strategy.Transaction).EngineSerialNumber);
    }
}
