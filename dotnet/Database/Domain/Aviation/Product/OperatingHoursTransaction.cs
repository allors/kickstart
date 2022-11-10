namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class OperatingHoursTransaction
    {
        public void AviationOnInit(ObjectOnInit method)
        {
            if (!this.ExistPreviousTransaction)
            {
                this.PreviousTransaction = this.SerialisedItem?.SyncedOperatingHoursTransactions?.Where(v => v.Id != this.Id).OrderByDescending(v => v.CreationDate).FirstOrDefault();
            }
            
            if (this.ExistPreviousTransaction)
            {
                this.Delta = this.Value - this.PreviousTransaction.Value;
                this.Days = (this.RecordingDate - this.PreviousTransaction.RecordingDate).Days;
            }

            //Sync
            this.SerialisedItem.AddSyncedOperatingHoursTransaction(this);

            var hoursCharacteristic = this.SerialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(new SerialisedItemCharacteristicTypes(this.Transaction()).OperatingHours));

            if (hoursCharacteristic != null)
            {
                hoursCharacteristic.Value = this.Value.ToString();
            }
        }

        public void AviationOnPostDerive(ObjectOnPostDerive method)
        {
            var derivation = method.Derivation;

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.strategy.Transaction).DefaultSecurityToken,
            };

            if (this.SerialisedItem.OwnedBy is Organisation owner)
            {
                this.AddSecurityToken(owner.ContactsSecurityToken);
            }

            if (this.SerialisedItem.RentedBy is Organisation lessee)
            {
                this.AddSecurityToken(lessee.ContactsSecurityToken);
            }
        }
    }
}