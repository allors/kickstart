using Allors.Extra;

namespace Allors.Database.Domain
{
    using System.Linq;
    using System.Text;
    using Allors.Database.Meta;

    public partial class EmailPolicy
    {
        private readonly ITransaction transaction;
        private readonly MetaPopulation m;

        public EmailPolicy(ITransaction transaction)
        {
            this.transaction = transaction;
            this.m = transaction.Database.Services.Get<MetaPopulation>();
        }

        public void Immediate()
        {
            var persons = new People(this.transaction).Extent();
            persons.Filter.AddEquals(this.m.Person.EmailFrequency, new EmailFrequencies(this.transaction).Immediate);

            foreach (Person person in persons)
            {
                this.ProcessNotifications(person, "Application Notifications");
                this.ProcessServiceRequests(person, "Application Service Requests update");
            }

            this.transaction.Derive();
            this.transaction.Commit();
        }

        public void Daily()
        {
            var persons = new People(this.transaction).Extent();
            persons.Filter.AddEquals(this.m.Person.EmailFrequency, new EmailFrequencies(this.transaction).Daily);

            foreach (Person person in persons)
            {
                this.ProcessNotifications(person, "Application Notifications - Daily Digest");
                this.ProcessServiceRequests(person, "Application Service Requests update - Daily Digest");
            }

            this.transaction.Derive();
            this.transaction.Commit();
        }

        public void Weekly()
        {
            var persons = new People(this.transaction).Extent();
            persons.Filter.AddEquals(this.m.Person.EmailFrequency, new EmailFrequencies(this.transaction).Weekly);

            foreach (Person person in persons)
            {
                this.ProcessNotifications(person, "Application Notifications - Weekly Digest");
                this.ProcessServiceRequests(person, "Application Service Requests update - Weekly Digest");
            }

            this.transaction.Derive();
            this.transaction.Commit();
        }

        private void ProcessNotifications(Person person, string subject)
        {
            var notifications = person.NotificationList.UnconfirmedNotifications.Where(v => v.ShouldEmail).ToArray();

            if (notifications.Length > 0)
            {
                var emailMessage = new EmailMessageBuilder(this.transaction)
                    .WithRecipient(person)
                    .WithSubject(subject).Build();

                var body = new StringBuilder();

                body.Append("<h3>Overview</h3>\n");
                body.Append($"<p>You have {notifications.Length} new notifications.</p>\n");

                body.Append("<ul>\n");
                foreach (var notification in notifications)
                {
                    body.Append($"<li>{notification.Title.Truncate(70)}\n");
                    body.Append("<p>Detail</p>\n");
                    body.Append($"<p>{notification.Description}<p>\n");
                    body.Append("<hr>\n");
                    body.Append("</li>\n");
                }

                body.Append("</ul>\n");

                emailMessage.Body = body.ToString();

                foreach (var notification in notifications)
                {
                    notification.EmailMessage = emailMessage;
                }
            }
        }

        private void ProcessServiceRequests(Person person, string subject)
        {
            var serviceRequests = new WorkRequirements(this.transaction).Extent().Where(v => v.CreatedBy.Equals(person) && v.ShouldEmail).ToArray();

            if (serviceRequests.Length > 0)
            {
                var emailMessage = new EmailMessageBuilder(this.transaction)
                    .WithRecipient(person)
                    .WithSubject(subject).Build();

                var body = new StringBuilder();

                body.Append("<h3>Overview</h3>\n");
                body.Append($"<p>You have {serviceRequests.Length} finished service request(s).</p>\n");

                body.Append("<ul>\n");
                foreach (var serviceRequest in serviceRequests)
                {
                    body.Append($"<li>{serviceRequest.Description.Truncate(70)}\n");
                    body.Append($"<p>{serviceRequest.RequirementNumber}</p>\n");
                    body.Append($"<p>{serviceRequest.Reason}<p>\n");
                    body.Append("</li>\n");
                    body.Append("<hr>\n");
                }

                body.Append("</ul>\n");

                emailMessage.Body = body.ToString();

                foreach (var serviceRequest in serviceRequests)
                {
                    serviceRequest.EmailMessage = emailMessage;
                }
            }
        }
    }
}
