// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IataGseCodes.cs" company="Allors bvba">
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
namespace Allors.Database.Domain
{
    using System;

    public partial class IataGseCodes
    {
        protected override void AviationSetup(Setup setup)
        {
            new IataGseCodeBuilder(this.Transaction).WithCode("ACU").WithName("Aircraft Air Conditioning (Cooling) Unit").WithSwissportDescription("Air Conditioner").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("AFH").WithName("Aircraft Fuel Hydrant Unit").WithSwissportDescription("Aircraft Fuel Hydrant Unit").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("AFT").WithName("Aircraft Fuel Tanker").WithSwissportDescription("Aircraft Fuel Tanker").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("AHU").WithName("Aircraft Ground Heater").WithSwissportDescription("Heater").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("APB").WithName("Airport Passenger Bus").WithSwissportDescription("Ramp Bus").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("ASU").WithName("Air Start Unit").WithSwissportDescription("Air Starter – Continuous Flow").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("ATC").WithName("Tow Bar Aircraft Tractor").WithSwissportDescription("Aircraft Tractor – Conventional").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("ATL").WithName("Aircraft Nose Gear-controlled Towbarless Tractor").WithSwissportDescription("Aircraft Tractor – Tow Bar Less").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("BCT").WithName("Baggage/Cargo Cart").WithSwissportDescription("Baggage Cart").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("BTU").WithName("Ramp Equipment Tractors").WithSwissportDescription("Baggage Tractor").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CAR").WithName("Cars / Passenger Vehicles").WithSwissportDescription("CAR").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CBL").WithName("Self Propelled Conveyor Belt Loader").WithSwissportDescription("Belt Loader").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CBU").WithName("Crew Transportation Vehicle").WithSwissportDescription("Crew Bus").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CCT").WithName("Baggage/Cargo Cart").WithSwissportDescription("Cargo Cart").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CDT").WithName("Lower Deck Turntable Container Dolly").WithSwissportDescription("Container Dolly").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CLU").WithName("Catering Vehicle").WithSwissportDescription("Catering Hi-Lift Truck").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CTL").WithName("Container Transporter/Loader").WithSwissportDescription("Container Transporter/Loader").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CTT").WithName("Unit Load Device Transport Vehicle").WithSwissportDescription("Cargo Transport Truck").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("CTU").WithName("Cargo Tractor").WithSwissportDescription("Cargo Tractor").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("DIU").WithName("Aircraft De-Icing/Anti-Icing Vehicle").WithSwissportDescription("Deicer").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("FLU").WithName("Forklift").WithSwissportDescription("Forklift").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("GFU").WithName("Self Propelled Petro/Diesel Refueling Vehicle for GSE").WithSwissportDescription("GSE Refueler").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("GPU").WithName("Ground Power Unit for Aircraft Electrical System").WithSwissportDescription("Ground Power Unit").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("HLU").WithName("Incapacitated Passenger Boarding Vehicle").WithSwissportDescription("Handicap Loading Vehicle").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("LDL").WithName("Lower Deck Container/Pallet Loader").WithSwissportDescription("Pallet Loader").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("LSU").WithName("Self Propelled Lavatory Vehicle ").WithSwissportDescription("Lavatory Service Truck").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("MDL").WithName("Main Deck Container/Pallet Loader").WithSwissportDescription("Main Deck Loader").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("MLU").WithName("Maintenance Hi-Lift Truck").WithSwissportDescription("Maintenance Hi-Lift Truck").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("PBB").WithName("Passenger Boarding Bridge").WithSwissportDescription("Passenger Boarding Bridge").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("PBS").WithName("Passenger Boarding Steps ").WithSwissportDescription("Passenger Boarding Stair").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("PCT").WithName("Pallet/Container Transporter").WithSwissportDescription("Pallet Transporter").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("PDT").WithName("Pallet Dolly").WithSwissportDescription("Pallet Dolly").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("PUT").WithName("Pick Up Truck").WithSwissportDescription("Pick Up Truck").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("SPE").WithName("Special Equipment").WithSwissportDescription("Special Equipment").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("TBR").WithName("Aircraft Towbar").WithSwissportDescription("Aircraft Tow Bar").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("TSU").WithName("Tail Stanchion").WithSwissportDescription("Tail Stand").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("VAN").WithName("VAN (Personnel VAN / Cargo VAN)").WithSwissportDescription("VAN (Personnel VAN / Cargo VAN)").WithIsActive(true).Build();
            new IataGseCodeBuilder(this.Transaction).WithCode("WSU").WithName("Self Propelled Potable Water Vehicle").WithSwissportDescription("Portable Water Service Truck").WithIsActive(true).Build();
        }
    }
}