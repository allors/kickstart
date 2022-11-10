// <copyright file="CustomerModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.WorkTaskModel
{
    public class CustomerModel
    {
        public CustomerModel(Party customer)
        {
            if (customer != null)
            {
                this.Number = customer.Id.ToString();
                this.Name = customer.DisplayName;
            }
        }

        public string Number { get; }

        public string Name { get; }
    }
}
