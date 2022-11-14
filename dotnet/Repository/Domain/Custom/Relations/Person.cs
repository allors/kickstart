// <copyright file="Person.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Allors.Repository
{
    public partial class Person : Party
    {
        #region inherited properties
        public User CreatedBy { get; set; }
        public User LastModifiedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string DisplayName { get; set; }
        public Locale Locale { get; set; }
        public Currency PreferredCurrency { get; set; }
        #endregion

        #region inherited methods
        #endregion
    }
}