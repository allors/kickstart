// <copyright file="WorkTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class WorkTask
    {
        #region Allors
        [Id("2ffdb5a6-0357-4cff-a4a2-98230f351c5c")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public MaintenanceAgreement MaintenanceAgreement { get; set; }

        #region Allors
        [Id("e82a7e85-9b87-4682-86ce-2cb3537303bb")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PrintDocument PrintWorkerDocument { get; set; }

        #region Allors
        [Id("8d126722-f0e9-4fb1-a7e2-f630558f0d1e")]
        
        
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal BillableAmountOfTimeInHours { get; set; }

        #region Allors
        [Id("2c5fb1f7-75f8-432f-8af8-6798ad17dc4c")]
        #endregion
        [Derived]
        public string CleaningCalculation { get; set; }

        #region Allors
        [Id("f2afa956-3769-4b5a-ab35-2bd634f1fd41")]
        #endregion
        [Derived]
        public string SundriesCalculation { get; set; }

        #region Allors
        [Id("0b00c7e7-ef29-4701-b913-83321543bd63")]
        #endregion
        public string Location { get; set; }

        #region Allors
        [Id("6f7e47d3-2936-4df9-8840-ac5ac10491ba")]
        #endregion
        [Workspace(Default)]
        public void PrintForWorker() { }
    }
}
