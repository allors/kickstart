// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerialisedInventoryItemObjectStates.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Allors.Database.Meta;

namespace Allors.Database.Domain
{
    using System;

    public partial class SerialisedItemCharacteristicTypes
    {
        public static readonly Guid OperatingHoursId = new Guid("AC38D868-C541-49D5-9BCC-DCA118AA707D");
        public static readonly Guid LengthId = new Guid("F2C43FD8-9960-48D5-ACE4-D96683DC9CFB");
        public static readonly Guid WidthId = new Guid("18F3FF25-1D95-4A48-91D8-68289BC883BB");
        public static readonly Guid HeightId = new Guid("4CBE5CE8-1C0C-4738-9855-FC4BE5E56678");
        public static readonly Guid WeightId = new Guid("7A5E4EAC-4AD4-4969-AA0C-9DE7F142955F");
        public static readonly Guid ChassisNumberId = new Guid("3417E14D-746E-408A-8697-3B1142CA26CB");
        public static readonly Guid EngineBrandId = new Guid("9A90CBD7-3AF8-41E8-ACD8-4D8D62554E18");
        public static readonly Guid EngineModelId = new Guid("A4DCE977-349F-4D1B-900A-3A7462074482");
        public static readonly Guid EngineSerialNumberId = new Guid("A2F4796B-5D38-478B-977A-2042A4AA5906");

        private UniquelyIdentifiableCache<SerialisedItemCharacteristicType> cache;
        public SerialisedItemCharacteristicType OperatingHours => this.Cache[OperatingHoursId];

        public SerialisedItemCharacteristicType Length => this.Cache[LengthId];

        public SerialisedItemCharacteristicType Width => this.Cache[WidthId];

        public SerialisedItemCharacteristicType Height => this.Cache[HeightId];

        public SerialisedItemCharacteristicType Weight => this.Cache[WeightId];

        public SerialisedItemCharacteristicType ChassisNumber => this.Cache[ChassisNumberId];
        public SerialisedItemCharacteristicType EngineBrand => this.Cache[EngineBrandId];
        public SerialisedItemCharacteristicType EngineModel => this.Cache[EngineModelId];
        public SerialisedItemCharacteristicType EngineSerialNumber => this.Cache[EngineSerialNumberId];

        private UniquelyIdentifiableCache<SerialisedItemCharacteristicType> Cache => this.cache ??= new UniquelyIdentifiableCache<SerialisedItemCharacteristicType>(this.Transaction);

        protected override void AviationPrepare(Setup setup)
        {
            setup.AddDependency(this.ObjectType, M.UnitOfMeasure);
            setup.AddDependency(this.ObjectType, M.TimeFrequency);
        }

        protected override void AviationSetup(Setup setup)
        {
            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(OperatingHoursId)
                .WithName("Operating Hours")
                .WithUnitOfMeasure(new TimeFrequencies(this.Transaction).Hour)
                .WithExternalPrimaryKey("Operating Hours")
                .WithIsPublic(false)
                .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(LengthId)
                 .WithName("Length")
                 .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Millimeter)
                 .WithExternalPrimaryKey("Length")
                .WithIsPublic(true)
                 .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(WidthId)
                .WithName("Width")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Millimeter)
                .WithExternalPrimaryKey("Width")
                .WithIsPublic(true)
                .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(HeightId)
                .WithName("Height")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Millimeter)
                .WithExternalPrimaryKey("Height")
                .WithIsPublic(true)
                .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(WeightId)
                .WithName("Weight")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Kilogram)
                .WithExternalPrimaryKey("Weight")
                .WithIsPublic(true)
                .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(ChassisNumberId)
                .WithName("Chassis Number")
                .WithExternalPrimaryKey("Chassis Number")
                .WithIsPublic(false)
                .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(EngineSerialNumberId)
                .WithName("Engine Serial Number")
                .WithExternalPrimaryKey("EngineSerialNumber")
                .WithIsPublic(false)
                .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(EngineBrandId)
                .WithName("Engine Brand")
                .WithExternalPrimaryKey("EngineBrand")
                .WithIsPublic(true)
                .Build();

            new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                .WithUniqueId(EngineModelId)
                .WithName("Engine Model")
                .WithExternalPrimaryKey("EngineModel")
                .WithIsPublic(true)
                .Build();
        }
    }
}